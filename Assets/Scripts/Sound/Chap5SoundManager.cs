using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 🎧 Chapter 5 전체 사운드 매니저
/// - Recoding 루프, Typing 효과음, BGM, 기타 SFX 관리
/// - Chap5 → 다른 씬 전환 시 자동 정지 및 파괴
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

    // =============================================================
    // 🧱 초기화
    // =============================================================
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

        // ✅ 오디오 기본 세팅 (볼륨·루프 포함)
        bgmSource.playOnAwake = false;
        bgmSource.loop = true;
        bgmSource.volume = 0.7f;

        sfxSource.playOnAwake = false;
        typingSource.playOnAwake = false;
        recordingLoopSource.playOnAwake = false;
    }

    void Start()
    {
        if (soundDB == null)
            Debug.LogWarning("[Chap5SoundManager] ⚠️ SoundDB 연결 안됨");
        else
            Debug.Log("[Chap5SoundManager] ✅ SoundDB 연결됨");

        currentSceneName = SceneManager.GetActiveScene().name;
        HandleSceneBGM(currentSceneName);
    }

    // =============================================================
    // 🎬 씬 로드 감지 (자동 정지 + 파괴)
    // =============================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;

        // ✅ Chap5 아닌 씬으로 이동 시 자동 종료
        if (!currentSceneName.Contains("Chap5") && !currentSceneName.Contains("5_"))
        {
            StopAllSounds();
            Destroy(gameObject);
            Debug.Log($"[Chap5SoundManager] 🚪 씬 이동 감지됨 → '{currentSceneName}'로 전환됨, 모든 사운드 정지 및 매니저 삭제됨");
            return;
        }

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

        string lower = sceneName.ToLower();

        if (lower.Contains("broken"))
        {
            PlayBGM(soundDB.brokenTheTuneBGM);
        }
        else if (lower.Contains("chap5") || lower.Contains("5_"))
        {
            PlayBGM(soundDB.chap5BGM, savedBGMTime);
        }
        else
        {
            Debug.Log($"[Chap5SoundManager] 🎵 '{sceneName}'은 Chap5 관련 씬이 아니므로 BGM 미재생");
        }
    }

    private void PlayBGM(AudioClip clip, float startTime = 0f)
    {
        if (clip == null)
        {
            Debug.LogWarning("[Chap5SoundManager] ⚠️ BGM 클립이 null입니다.");
            return;
        }

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.time = startTime;
        bgmSource.volume = 0.7f;
        bgmSource.Play();

        Debug.Log($"[Chap5SoundManager] 🎶 BGM 재생 시작: {clip.name}");
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

    // 📼 테이프 조각 획득
    public void PlayGetTapePieceSFX()
    {
        if (soundDB == null || soundDB.getTapePieceSFX == null)
        {
            Debug.LogWarning("[Chap5SoundManager] ⚠️ GetTapePieceSFX가 SoundDB에 연결되지 않음");
            return;
        }

        sfxSource.volume = 0.8f;
        sfxSource.pitch = 1f;
        sfxSource.PlayOneShot(soundDB.getTapePieceSFX);

        Debug.Log("[Chap5SoundManager] 📼 테이프 조각 획득 SFX 재생");
    }

    // 🎧 공용 SFX 재생용 함수
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("[Chap5SoundManager] ⚠️ 전달된 SFX 클립이 null입니다.");
            return;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.volume = 0.9f;
        }

        sfxSource.pitch = 1f;
        sfxSource.volume = 0.9f;
        sfxSource.PlayOneShot(clip);

        Debug.Log($"[Chap5SoundManager] 🔊 SFX 재생됨: {clip.name}");
    }

    // ✅ 모든 사운드 정지
    public void StopAllSounds()
    {
        if (bgmSource != null && bgmSource.isPlaying) bgmSource.Stop();
        if (sfxSource != null && sfxSource.isPlaying) sfxSource.Stop();
        if (typingSource != null && typingSource.isPlaying) typingSource.Stop();
        if (recordingLoopSource != null && recordingLoopSource.isPlaying) recordingLoopSource.Stop();

        Debug.Log("[Chap5SoundManager] 🔇 모든 사운드 정지됨");
    }
}
