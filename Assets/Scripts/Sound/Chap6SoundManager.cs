using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 🎧 Chapter 6 Sound Manager
/// - Chap6 BGM, Mymy Winding SFX 관리
/// - Chap6, Epilogue 씬에서만 유지됨
/// - 다른 씬으로 넘어가면 모든 사운드 종료 후 자동 파괴
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
        SceneManager.sceneLoaded += OnSceneLoaded;

        // ✅ AudioSource 자동 추가 (없을 경우)
        if (!bgmSource) bgmSource = gameObject.AddComponent<AudioSource>();
        if (!sfxSource) sfxSource = gameObject.AddComponent<AudioSource>();

        // ✅ 공통 초기 설정
        bgmSource.playOnAwake = false;
        sfxSource.playOnAwake = false;

        bgmSource.loop = true;
        sfxSource.loop = true; // 🔁 회전음은 루프 필요
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
    // 🚪 씬 이동 시 자동 정리
    // =============================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name.ToLower();

        // ✅ Chap6 또는 Epilogue 외의 씬이면 사운드 종료 후 제거
        if (!sceneName.Contains("chap6") && !sceneName.Contains("epilogue"))
        {
            Debug.Log($"[Chap6SoundManager] 🚪 '{scene.name}' 진입 감지 → 사운드 종료 및 매니저 제거");
            StopAllSounds();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Destroy(gameObject);
        }
    }

    // =============================================================
    // 🎵 BGM 재생
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
    // 🔁 마이마이 회전 사운드
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
        sfxSource.pitch = 1f;
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

    private System.Collections.IEnumerator ClearClipNextFrame()
    {
        yield return null;
        sfxSource.clip = null;
    }

    // =============================================================
    // 🔇 전체 정지 (씬 전환 시 호출됨)
    // =============================================================
    public void StopAllSounds()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();

        if (sfxSource != null && sfxSource.isPlaying)
            sfxSource.Stop();

        isMymyPlaying = false;

        Debug.Log("[Chap6SoundManager] 🔇 모든 사운드 정지됨");
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
