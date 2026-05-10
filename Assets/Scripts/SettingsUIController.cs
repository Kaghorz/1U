using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private InputActionReference escapeAction;

    private void OnEnable()
    {
        if (escapeAction != null && escapeAction.action != null)
            escapeAction.action.Enable();
    }

    private void OnDisable()
    {
        if (escapeAction != null && escapeAction.action != null)
            escapeAction.action.Disable();
    }

    private void Start()
    {
        // Ensure the panel is hidden at the start
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Link UI elements to the SettingsManager functions
        masterSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMasterVolume);
        musicSlider.onValueChanged.AddListener(SettingsManager.Instance.SetMusicVolume);
        musicToggle.onValueChanged.AddListener(SettingsManager.Instance.ToggleMusic);
    }

    private void Update()
    {
        // Toggle settings menu with Escape key
        if (escapeAction.action.WasPressedThisFrame())
        {
            ToggleSettingsMenu();
        }
    }

    public void ToggleSettingsMenu()
    {
        bool isActive = !settingsPanel.activeSelf;
        settingsPanel.SetActive(isActive);

        // Pause or resume time when menu is open
        Time.timeScale = isActive ? 0f : 1f;

        // Unlock/Lock cursor for UI interaction
        Cursor.lockState = isActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isActive;
    }

    public void OnQuitButtonClicked()
    {
        SettingsManager.Instance.QuitGame();
    }
}