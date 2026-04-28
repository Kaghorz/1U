using UnityEngine;
using Unity.Cinemachine;

public class BlueProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float initialSpeed = 30f;
    [SerializeField] private float flightDistance = 10f;
    [SerializeField] private float lifetime = 10f;

    [Header("Growth Settings")]
    [SerializeField] private float growthDuration = 1f;
    [SerializeField] private float targetScale = 10f;

    [Header("Vacuum Settings")]
    [SerializeField] private float pullRadius = 10f;
    [SerializeField] private float pullForce = 25f;
    [SerializeField] private LayerMask affectedLayers;

    private Vector3 startPosition;
    private Vector3 direction;
    private Vector3 initialScale;
    private float currentSpeed;
    private float spawnTime;
    private bool isLaunched = false;
    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
        initialScale = transform.localScale;
    }

    public void Launch(Vector3 targetDirection)
    {
        direction = targetDirection.normalized;
        startPosition = transform.position;
        currentSpeed = initialSpeed;
        spawnTime = Time.time;
        isLaunched = true;

        if (impulseSource != null) impulseSource.GenerateImpulse();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        if (!isLaunched) return;

        HandleMovement();
        HandleGrowth();
        ApplyVacuum();
    }

    private void HandleMovement()
    {
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);

        // Slow down smoothly as it approaches max flight distance
        if (distanceTraveled < flightDistance)
        {
            float distanceRatio = distanceTraveled / flightDistance;
            // Interpolate speed from initial speed down to 0
            currentSpeed = Mathf.Lerp(initialSpeed, 0f, distanceRatio);
            transform.position += direction * currentSpeed * Time.deltaTime;
        }
        else
        {
            currentSpeed = 0f; // Fully stopped
        }
    }

    private void HandleGrowth()
    {
        float timeAlive = Time.time - spawnTime;
        if (timeAlive < growthDuration)
        {
            float progress = timeAlive / growthDuration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one * targetScale, progress);
        }
        else
        {
            transform.localScale = Vector3.one * targetScale;
        }
    }

    private void ApplyVacuum()
    {
        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, pullRadius, affectedLayers);
        foreach (Collider obj in affectedObjects)
        {
            // Ignore the player or triggers
            if (obj.CompareTag("Player") || obj.isTrigger) continue;

            if (obj.TryGetComponent(out Rigidbody rb))
            {
                // Pull objects towards the center of the sphere
                Vector3 forceDirection = (transform.position - obj.transform.position).normalized;
                rb.AddForce(forceDirection * pullForce, ForceMode.Acceleration);
            }
        }
    }
}
