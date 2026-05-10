using UnityEngine;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    public static MissionManager Instance;

    [Header("Spawn Settings")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject portalPrefab;
    [SerializeField] private Transform portalSpawnPoint;

    private List<EnemyBase> activeEnemies = new List<EnemyBase>();
    private bool bossSpawned = false;
    private bool bossDead = false;

    private void Awake()
    {
        Instance = this;
    }

    // Enemies call this in their Awake/Start
    public void RegisterEnemy(EnemyBase enemy)
    {
        activeEnemies.Add(enemy);
    }

    // Enemies call this when they die
    public void UnregisterEnemy(EnemyBase enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }

        CheckMissionProgress();
    }

    private void CheckMissionProgress()
    {
        if (activeEnemies.Count == 0 && !bossSpawned)
        {
            SpawnMiniBoss();
        }
    }

    private void SpawnMiniBoss()
    {
        bossSpawned = true;
        Debug.Log("All curses exorcised. A Special Grade is appearing...");

        if (bossPrefab != null && bossSpawnPoint != null)
        {
            GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }
    }

    // This is called when the Boss's health reaches zero
    public void OnBossDefeated()
    {
        if (!bossDead)
        {
            bossDead = true;
            Debug.Log("Special Grade Exorcised. Portal to Jujutsu College opening...");
            SpawnPortal();
        }
    }

    private void SpawnPortal()
    {
        if (portalPrefab != null && portalSpawnPoint != null)
        {
            Instantiate(portalPrefab, portalSpawnPoint.position, portalSpawnPoint.rotation);
        }
    }
}