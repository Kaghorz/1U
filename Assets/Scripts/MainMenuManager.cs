using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject characterSelectPanel;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonClickSound;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        mainMenuPanel.SetActive(false);

        // TODO: Replace "SampleScene" with the actual name of level scene
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Sample Scene Loaded");

        
    }

    public void OpenCharacterSelect()
    {
        mainMenuPanel.SetActive(false);
        characterSelectPanel.SetActive(true);
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
        characterSelectPanel.SetActive(false);
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