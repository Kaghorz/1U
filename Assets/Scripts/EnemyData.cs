using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyData", menuName = "JJK_Game/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Health Settings")]
    public float maxHealth;

    [Header("Movement Settings")]
    public float moveSpeed;
    public float runSpeed; // For chasing the player
    public float rotationSpeed;

    [Header("Combat Settings")]
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;

    [Header("Detection Settings")]
    public float hearingRange; // To hear the player's footsteps
    public float raycastDistance; // For wall avoidance
}