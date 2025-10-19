using UnityEngine;

public class TuneSoundManager : MonoBehaviour
{
    public static TuneSoundManager Instance;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        Instance = this;
        
        // Create AudioSource if not exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    public void PlayBrokenSound()
    {
        // Generate broken sound programmatically
        float frequency = 220f + Random.Range(0f, 120f);
        PlayTone(frequency, 0.15f, 0.08f);
    }
    
    public void PlayErrorSound()
    {
        // Generate error sound programmatically
        PlayTone(100f, 0.25f, 0.1f, true);
    }
    
    void PlayTone(float frequency, float duration, float volume, bool sawtooth = false)
    {
        // Simple tone generation using AudioSource
        // Note: For actual implementation, you might want to use AudioClips
        // This is a simplified version
        
        if (audioSource.isPlaying) return;
        
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];
        
        for (int i = 0; i < samples; i++)
        {
            float time = i / (float)sampleRate;
            if (sawtooth)
            {
                // Sawtooth wave
                data[i] = Mathf.Repeat(frequency * time, 1f) * 2f - 1f;
            }
            else
            {
                // Sine wave
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * time);
            }
            
            // Apply envelope
            float envelope = 1f - (i / (float)samples);
            data[i] *= envelope * volume;
        }
        
        // Create and play AudioClip
        AudioClip clip = AudioClip.Create("GeneratedTone", samples, 1, sampleRate, false);
        clip.SetData(data, 0);
        audioSource.PlayOneShot(clip);
    }
    
    // Alternative: If you have audio files
    public void PlayAudioClip(AudioClip clip, float volume = 1f)
    {
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}