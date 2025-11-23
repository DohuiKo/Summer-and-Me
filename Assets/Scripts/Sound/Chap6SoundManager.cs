using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 🎧 Chapter 6 Sound Manager (Fixed)
/// - Chap6 BGM, Mymy Winding SFX 관리
/// - Chap6, Epilogue 씬에서만 유지됨
/// - 다른 씬으로 이동 시 모든 사운드 즉시 종료 및 매니저 삭제
/// </summary>
public class Chap6SoundManager : MonoBehaviour
{
    public static Chap6SoundManager Instance;

    [Header("Audio References")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Database")]
    public SoundDatabase soundDB;

    private bool isMymyPlaying = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 씬 이동 감지용
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged; // 🔥 씬 전환 직전 감지 추가

        // ✅ AudioSource 자동 생성
        if (!bgmSource) bgmSource = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();

        // ✅ 공통 초기화
        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;
        bgmSource.loop = true;
        sfxSource.loop = true; // 마이마이 회전 루프
    }

    void Start()
    {
        if (soundDB == null)
        {
            Debug.LogWarning("[Chap6SoundManager] ⚠️ SoundDB 연결 안됨");
            return;
        }

        PlayBGM();
    }

    // =============================================================
    // 🚪 씬 전환 감지
    // =============================================================
    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        // 씬 로드 직전 호출 — 즉시 정지
        string nextName = next.name.ToLower();

        if (!nextName.Contains("chap6") && !nextName.Contains("epilogue"))
        {
            Debug.Log($"[Chap6SoundManager] ⚠️ '{next.name}'로 이동 직전 감지 → 즉시 사운드 종료");
            StopAllSounds();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name.ToLower();

        // 씬 로드 완료 후 최종 정리 (이중 안전망)
        if (!sceneName.Contains("chap6") && !sceneName.Contains("epilogue"))
        {
            Debug.Log($"[Chap6SoundManager] 🚪 '{scene.name}' 진입 → 매니저 제거");
            StopAllSounds();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
            Destroy(gameObject);
        }
    }

    // =============================================================
    // 🎵 BGM
    // =============================================================
    public void PlayBGM()
    {
        if (soundDB == null || soundDB.chap6BGM == null)
        {
            Debug.LogWarning("[Chap6SoundManager] ⚠️ Chap6BGM 연결 안됨");
            return;
        }

        bgmSource.clip = soundDB.chap6BGM;
        bgmSource.volume = 0.7f;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log("[Chap6SoundManager] 🎵 Chap6 BGM 재생 시작");
    }

    // =============================================================
    // 🔁 마이마이 회전음
    // =============================================================
    public void PlayMymyWindingSFX()
    {
        if (isMymyPlaying)
        {
            Debug.Log("[Chap6SoundManager] ⏸️ 이미 회전음 재생 중");
            return;
        }

        if (soundDB == null || soundDB.mymyWindingSFX == null)
        {
            Debug.LogWarning("[Chap6SoundManager] ⚠️ MymyWindingSFX 연결 안됨");
            return;
        }

        sfxSource.clip = soundDB.mymyWindingSFX;
        sfxSource.volume = 0.85f;
        sfxSource.loop = true;
        sfxSource.Play();

        isMymyPlaying = true;
        Debug.Log("[Chap6SoundManager] 🔄 마이마이 회전 사운드 재생 시작");
    }

    public void StopMymyWindingSFX()
    {
        if (!isMymyPlaying || sfxSource == null) return;

        sfxSource.Stop();
        isMymyPlaying = false;
        StartCoroutine(ClearClipNextFrame());
        Debug.Log("[Chap6SoundManager] ⏹️ 회전 사운드 정지 완료");
    }

    private IEnumerator ClearClipNextFrame()
    {
        yield return null;
        if (sfxSource != null)
            sfxSource.clip = null;
    }

    // =============================================================
    // 🔇 전체 정지
    // =============================================================
    public void StopAllSounds()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }

        if (sfxSource != null)
        {
            sfxSource.Stop();
            sfxSource.clip = null;
        }

        isMymyPlaying = false;
        Debug.Log("[Chap6SoundManager] 🔇 모든 사운드 완전 정지");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
}
