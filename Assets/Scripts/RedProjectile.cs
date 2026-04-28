using UnityEngine;
using Unity.Cinemachine;

public class RedProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 60f;
    [SerializeField] private float lifeTime = 0.5f;

    [Header("Combat Settings")]
    [SerializeField] private float damage = 45f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float repulsionForce = 25f;
    [SerializeField] private LayerMask affectedLayers; // Set this to Enemy or Destructible layers in the Inspector

    [Header("Visuals")]
    [SerializeField] private GameObject impactEffectPrefab;
    public CinemachineImpulseSource impulseSource;

    private Vector3 direction;
    private bool isLaunched = false;

    private void Awake()
    {
        if (impulseSource == null) impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Launch(Vector3 targetDirection)
    {
        direction = targetDirection;
        isLaunched = true;

        // Safety: Ensure it doesn't live forever
        Destroy(gameObject, lifeTime);

        //Debug
        Debug.Log($"Red Projectile Launched in direction: {direction}");
    }

    private void Update()
    {
        if (!isLaunched) return;

        // Use Transform movement like Hollow Purple (more reliable for high speed)
        transform.position += direction * speed * Time.deltaTime;
        transform.forward = direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug
        Debug.Log($"Touch detected with: {other.name} (Tag: {other.tag})");

        // Check if we hit the player or something else to ignore
        if (other.CompareTag("Player") || other.isTrigger) return;

        Explode();
    }

    private void Explode()
    {
        // Visuals/Shake
        if (impactEffectPrefab != null) Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        if (impulseSource != null) impulseSource.GenerateImpulse();

        // Impact Physics
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, affectedLayers);
        foreach (Collider hit in colliders)
        {
            // Apply Repulsion to anything with a Rigidbody
            if (hit.TryGetComponent(out Rigidbody rb))
            {
                Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                pushDir.y += 0.5f; // Slight lift for better "Gojo" feel
                rb.AddForce(pushDir * repulsionForce, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);

        //Debug
        Debug.Log($"Red Projectile exploded at: {transform.position} with {colliders.Length} affected objects");
    }

    private void OnDestroy()
    {
        // If it hasn't exploded yet, why is it being destroyed?
        if (isLaunched) Debug.Log("Projectile destroyed. If you didn't see 'Explode', it timed out or was hit by something else.");
    }
}