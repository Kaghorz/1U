using UnityEngine;
using UnityEngine.SceneManagement;

public class CollegePortal : MonoBehaviour
{
    [Header("Scene Settings")]
    public string hubSceneName = "JujutsuCollege"; // Ensure this matches the college scene name exactly

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the portal is the Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Entering Portal... Returning to College.");
            LoadHubScene();
        }
    }

    private void LoadHubScene()
    {
        // Your PlayerPersistence script will handle the spawn point in the next scene
        SceneManager.LoadScene(hubSceneName);
    }
}