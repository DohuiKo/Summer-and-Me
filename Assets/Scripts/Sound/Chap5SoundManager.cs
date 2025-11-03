using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 🎧 Chapter 5 전체 사운드 매니저
/// - Recoding 루프, Typing 효과음, BGM 관리
/// </summary>
public class Chap5SoundManager : MonoBehaviour
{
    public static Chap5SoundManager Instance;

    [Header("Audio References")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource typingSource;
    public AudioSource recordingLoopSource;

    [Header("Database")]
    public SoundDatabase soundDB;

    private bool sobbingTriggered = false;
    private bool isTypingSFXPlaying = false;
    private bool isRecordingLoopPlaying = false;

    private float savedBGMTime = 0f;
    private string currentSceneName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!bgmSource) bgmSource = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();
        if (!typingSource) typingSource = gameObject.AddComponent<AudioSource>();
        if (!recordingLoopSource) recordingLoopSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (soundDB == null)
            Debug.LogWarning("[Chap5SoundManager] ⚠️ SoundDB 연결 안됨");
        currentSceneName = SceneManager.GetActiveScene().name;
        HandleSceneBGM(currentSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        HandleSceneBGM(currentSceneName);
    }

    // =============================================================
    // 🎵 씬별 BGM 관리
    // =============================================================
    private void HandleSceneBGM(string sceneName)
    {
        if (soundDB == null) return;

        if (bgmSource.isPlaying)
            savedBGMTime = bgmSource.time;

        if (sceneName.Contains("broken"))
        {
            PlayBGM(soundDB.brokenTheTuneBGM);
        }
        else if (sceneName.Contains("5_a_last_track") || sceneName.Contains("5_recoding"))
        {
            PlayBGM(soundDB.chap5BGM, savedBGMTime);
        }
    }

    private void PlayBGM(AudioClip clip, float startTime = 0f)
    {
        if (clip == null) return;
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.time = startTime;
        bgmSource.volume = 0.7f;
        bgmSource.Play();
    }

    // =============================================================
    // 🎚️ 사운드 제어
    // =============================================================

    // 🔁 Recoding 루프 재생
    public void PlayRecordingLoop()
    {
        if (soundDB == null || soundDB.recordingSFX == null)
        {
            Debug.LogWarning("[Chap5SoundManager] ❌ recordingSFX 연결 안됨");
            return;
        }

        if (isRecordingLoopPlaying) return;

        recordingLoopSource.clip = soundDB.recordingSFX;
        recordingLoopSource.loop = true;
        recordingLoopSource.volume = 0.6f;
        recordingLoopSource.pitch = 1f;
        recordingLoopSource.Play();

        isRecordingLoopPlaying = true;
        Debug.Log("[Chap5SoundManager] 🎙️ Recording SFX 루프 재생 시작");
    }

    public void StopRecordingLoop()
    {
        if (!isRecordingLoopPlaying) return;

        recordingLoopSource.Stop();
        isRecordingLoopPlaying = false;
        Debug.Log("[Chap5SoundManager] 🛑 Recording SFX 루프 정지");
    }

    // 🎧 타이핑 사운드 (TypingEffectManager용)
    public void PlayContinueTypingSFX()
    {
        if (isTypingSFXPlaying || soundDB == null || soundDB.continueTypingSFX == null)
            return;

        isTypingSFXPlaying = true;
        typingSource.clip = soundDB.continueTypingSFX;
        typingSource.volume = 0.55f;
        typingSource.loop = false;
        typingSource.pitch = Random.Range(0.9f, 1.05f);
        typingSource.Play();

        Debug.Log("[Chap5SoundManager] 🔊 타이핑 SFX 1회 재생");
        StartCoroutine(ResetTypingFlag());
    }

    IEnumerator ResetTypingFlag()
    {
        yield return new WaitForSeconds(0.8f);
        isTypingSFXPlaying = false;
    }

    // 🎹 직접 입력용 키 입력 사운드
    public void PlayTypingSFX()
    {
        if (soundDB == null || soundDB.typingSFX == null)
        {
            Debug.LogWarning("[Chap5SoundManager] ⚠️ typingSFX 연결 안됨");
            return;
        }

        sfxSource.pitch = Random.Range(0.9f, 1.05f);
        sfxSource.volume = 0.55f;
        sfxSource.PlayOneShot(soundDB.typingSFX);
    }

    // 🎭 가을 울음
    public void PlaySobbingGaeulOnce()
    {
        if (sobbingTriggered) return;
        if (soundDB == null || soundDB.sobbingGaeulSFX == null) return;

        sfxSource.PlayOneShot(soundDB.sobbingGaeulSFX);
        sobbingTriggered = true;
        Debug.Log("[Chap5SoundManager] 😢 가을 울음 1회 재생");
    }
}
