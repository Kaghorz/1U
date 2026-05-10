using UnityEngine;

public class PlayerAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioSource backgroundMusicSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private AudioClip backgroundMusicClip;

    private void Start()
    {
        if (backgroundMusicSource != null && backgroundMusicClip != null)
        {
            backgroundMusicSource.clip = backgroundMusicClip;
            backgroundMusicSource.loop = true;
            backgroundMusicSource.Play();
        }
    }

    // This method will be called by the Animation Event
    public void PlayFootstep()
    {
        if (footstepSource == null || footstepClip == null) return;

        // Slightly randomize pitch so every step sounds unique
        footstepSource.pitch = Random.Range(0.9f, 1.1f);
        footstepSource.PlayOneShot(footstepClip);

        // FUTURE: Link this to Noise Emitter for enemies to hear it
        // NoiseEmitter noise = GetComponent<NoiseEmitter>();
        // if (noise != null) noise.EmitFootstepNoise();
    }
}