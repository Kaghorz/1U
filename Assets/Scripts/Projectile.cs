using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 5f; //Timer before it disappears

    private void Start()
    {
        //Destroy this game object after the specified lifeTime
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        //Move the projectile forward in its own local space
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider healthcare)
    {
        //Optional: Destroy the projectile if it hits something
        //For now, we just destroy it to keep the world clean
        if (!healthcare.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}