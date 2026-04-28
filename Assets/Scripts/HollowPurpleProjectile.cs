using NUnit.Framework.Internal.Commands;
using Unity.Cinemachine;
using UnityEngine;

public class HollowPurpleProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 40f;
    [SerializeField] private float maxScale = 30f;
    [SerializeField] private float growthRate = 10f;
    [SerializeField] private float lifetime = 10f;

    [Header("Vacuum Effect")]
    [SerializeField] private float pullRadius = 20f;
    [SerializeField] private float pullForce = 40f;
    [SerializeField] private LayerMask affectedLayers; // Affect players, enemies, and destructibles

    private Vector3 direction;
    private bool isLaunched = false;
    public CinemachineImpulseSource impulseSource;

    private void Start()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Launch(Vector3 targetDirection)
    {
        direction = targetDirection;
        isLaunched = true;
        Destroy(gameObject, lifetime); // Safety cleanup
    }

    private void Update()
    {
        if (!isLaunched) return;

        // Movement
        transform.position += direction * speed * Time.deltaTime;
        
        if (transform.localScale.x < maxScale)
        {
            transform.localScale += Vector3.one * growthRate * Time.deltaTime;
        }

        ApplyVacuum();
    }

    private void ApplyVacuum()
    {
        Collider[] affectedObjects = Physics.OverlapSphere(transform.position, pullRadius, affectedLayers);
        foreach (var obj in affectedObjects)
        {
            // Calculate direction from the object towards the center of the Purple
            Vector3 forceDirection = (transform.position - obj.transform.position).normalized;

            if (obj.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                rb.AddForce(forceDirection * pullForce, ForceMode.Acceleration);
            }
        }
    }

    private void OnTriggerEnter(Collider healthcare)
    {
        Destroy(gameObject);
    }
}