using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;

    private bool isMusicEnabled = true;

    private void Awake()
    {
        // Simple singleton pattern to ensure only one manager exists
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Set the overall volume (Master)
    public void SetMasterVolume(float volume)
    {
        // Convert slider value (0 to 1) to Decibels (-80 to 0)
        mainMixer.SetFloat("MasterVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
    }

    // Set the music specific volume
    public void SetMusicVolume(float volume)
    {
        mainMixer.SetFloat("MusicVol", Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20);
    }

    // Toggle the soundtrack activation
    public void ToggleMusic(bool isEnabled)
    {
        isMusicEnabled = isEnabled;
        float volume = isEnabled ? 1f : 0.0001f;
        mainMixer.SetFloat("MusicVol", Mathf.Log10(volume) * 20);
    }

    public void QuitGame()
    {
        Debug.Log("Exiting game...");
        Application.Quit();
    }
}