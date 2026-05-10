using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private string levelSceneName = "JujutsuHigh";
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    private void Start()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);

        SceneManager.LoadScene(levelSceneName);
        Debug.Log(levelSceneName + " Loaded");

        
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToMain()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("Exit Button Pressed");
        Application.Quit(); // Only works in the actual build, not the editor

        // Debug: For testing in the editor
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void TestButtonClick()
    {
        Debug.Log("Test Button Clicked!");
        audioSource.PlayOneShot(buttonClickSound);
    }
}