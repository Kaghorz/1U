using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LevelBook : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string nextSceneName;
    [SerializeField] private InputActionReference interactAction;

    [Header("Visual Feedback (Optional)")]
    [SerializeField] private GameObject interactPromptUI; // FUTURE: A small "Press E" popup

    private bool isPlayerNearby = false;

    private void OnEnable()
    {
        interactAction.action.Enable();
    }

    private void Update()
    {
        // Check if player is in range and pressed the button
        if (isPlayerNearby && interactAction.action.WasPressedThisFrame())
        {
            LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogError("Next Scene Name is missing on the Book object!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactPromptUI != null) interactPromptUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactPromptUI != null) interactPromptUI.SetActive(false);
        }
    }
}