using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject player;
    public Vector3 position;
    [SerializeField] private int health = 100;
    [SerializeField] private float teleportLimit = 25f;
    [SerializeField] private float interval = 2f;
    private float timer = 0.0f;
    
    void Update()
    {
        timer += Time.deltaTime;

        if (health > 0 && timer >= interval)
        {
            //teleport the player if still alive
            TeleportPlayer();

            //apply random damage
            int randomDamage = Random.Range(25, 50);
            updateHealth(randomDamage);

            timer = 0.0f;
        }
    }

    void updateHealth(int delta)
    {
        health -= delta;

        if (health <= 0)
            Debug.Log("player died");
    }

    void TeleportPlayer()
    {
        //take a random value within the boundaries
        float randomX = Random.Range(-teleportLimit, teleportLimit);
        float randomZ = Random.Range(-teleportLimit, teleportLimit);

        Vector3 newPos = new(randomX, player.transform.position.y, randomZ);
        player.transform.position = newPos;

        Debug.Log("Player teleported to " + newPos);
    }    
}
