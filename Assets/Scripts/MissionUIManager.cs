using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MissionUIManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private TextMeshProUGUI missionTitleText;

    private string pendingSceneName;

    void Start()
    {
        //if (Application.isPlaying) gameObject.SetActive(false);
        if (confirmationPanel != null) confirmationPanel.SetActive(false);
    }

    // Now accepts both the scene name AND the display name
    public void RequestMission(string sceneName, string missionDisplayName)
    {
        pendingSceneName = sceneName;

        // Update the text on the panel
        if (missionTitleText != null)
        {
            missionTitleText.text = missionDisplayName;
        }

        confirmationPanel.SetActive(true);
    }

    public void ConfirmMission()
    {
        if (!string.IsNullOrEmpty(pendingSceneName))
        {
            // FORCE the player back into a playable state before the scene switch
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var controller = player.GetComponent<CharacterController>();
                if (controller != null)
                {
                    controller.enabled = true;
                }

                var playerController = player.GetComponent<PlayerController_v3>();
                if (playerController != null)
                {
                    playerController.enabled = true;
                }

                // Also ensure the cursor is relocked
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Debug
                Debug.Log("ConfirmMission(): Player controller state: " + (controller != null && controller.enabled));
            }

            SceneManager.LoadScene(pendingSceneName);
        }
    }

    public void CancelMission()
    {
        confirmationPanel.SetActive(false);
    }
}