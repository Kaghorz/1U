using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    [SerializeField] private GameObject gamePlayUI;

    public static PlayerPersistence Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // Tell Unity to run "OnLevelLoaded" whenever a scene changes
        SceneManager.sceneLoaded += OnLevelLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelLoaded;
    }

    void OnLevelLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the object named "MissionSpawnPoint" in the new scene
        GameObject spawnPoint = GameObject.Find("MissionSpawnPoint");

        if (spawnPoint != null)
        {
            if (spawnPoint.tag == "MissionSpawnPoint")
            {
                Debug.Log("Found MissionSpawnPoint in " + scene.name);

                // Disable CharacterController briefly because it overrides manual position changes
                CharacterController cc = GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;

                // Move and rotate the player
                transform.position = spawnPoint.transform.position;
                transform.rotation = spawnPoint.transform.rotation;

                // Re-enable the controller
                if (cc != null) cc.enabled = true;

                var playerController = GetComponent<PlayerController_v3>();
                if (playerController != null)
                {
                    playerController.enabled = true;
                }

                // Activate the GamePlay_UI if it exists
                if (gamePlayUI != null && gamePlayUI.tag == "GamePlay_UI")
                {
                    gamePlayUI.SetActive(true);

                    Debug.Log("OnLevelLoaded(): Activated GamePlay_UI in the scene.");
                }
                else
                {
                    Debug.LogWarning("OnLevelLoaded(): Could not find GamePlay_UI in the scene.");
                }

                Debug.Log("OnLevelLoaded(): Player controller state after teleport: " + (cc != null ? cc.enabled.ToString() : "No CharacterController found"));
            }
            else
            {
                Debug.LogWarning("Object named 'MissionSpawnPoint' found in " + scene.name + " but it does not have the correct tag.");
            }
            
        }
        else
        {
            Debug.LogWarning("No MissionSpawnPoint found in " + scene.name);
        }
    }
}