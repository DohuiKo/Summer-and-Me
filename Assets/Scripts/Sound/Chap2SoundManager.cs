using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class Chap2SoundManager : MonoBehaviour
{
    public static Chap2SoundManager Instance;

    [Header("🔊 볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("🌿 특정 페이지 감지 (펜 사운드 중단 트리거)")]
    public bool enablePencilStopDetection = true;
    public string stopTriggerPageName = "DiaryPage"; // 버튼 이름 or 페이지 이름에 포함되면 펜 사운드 중단

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource pencilSource;
    private AudioSource tapePlaySource;

    private float _lastPencilPlayTime = -1f;
    private Coroutine tapeRoutine; // 🎬 테이프 순차재생 관리용

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        pencilSource = gameObject.AddComponent<AudioSource>();
        tapePlaySource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;

        sfxSource.volume = sfxVolume;
        pencilSource.volume = sfxVolume;
        tapePlaySource.volume = sfxVolume;

        SceneManager.activeSceneChanged += OnSceneChanged;
        RegisterAllButtonsInScene();
        Debug.Log("✅ Chap2SoundManager 초기화 완료");
    }

    private void Start() => PlayChap2BGM();

    private void OnDestroy() => SceneManager.activeSceneChanged -= OnSceneChanged;

    private void OnSceneChanged(Scene current, Scene next)
    {
        // 챕터2가 아닌 씬으로 넘어가면 모든 사운드 정지
        if (!next.name.Contains("chap2") && !next.name.Contains("Chap2"))
        {
            StopBGM();
            StopPencilWrite();
            StopTapeAll();
            Debug.Log($"🌀 씬 전환 감지: {next.name} / 챕터2 종료 → 모든 사운드 중단");
            return;
        }

        // 챕터2 내부 페이지 전환일 때는 펜소리만 중단
        StopPencilWrite();
        Debug.Log($"🌀 씬 전환 감지: {next.name} / 펜소리 중단 (BGM 유지)");
        RegisterAllButtonsInScene();
    }


    private void RegisterAllButtonsInScene()
    {
        var buttons = FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
        {
            btn.onClick.RemoveListener(OnAnyButtonClicked);
            btn.onClick.AddListener(OnAnyButtonClicked);
        }
    }

    private void OnAnyButtonClicked()
    {
        if (!enablePencilStopDetection) return;
        string clickedName = UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject?.name ?? "Unknown";

        if (clickedName.Contains(stopTriggerPageName) || clickedName.ToLower().Contains("next"))
        {
            StopPencilWrite();
            StopTapeAll();
            Debug.Log($"✏️ 버튼 클릭 '{clickedName}' 감지 → 펜·테이프 사운드 중단");
        }
    }

    // 🎵 BGM 재생
    private void PlayChap2BGM()
    {
        var clip = AudioManager.Instance?.soundDB?.chap2BGM;
        if (clip == null) return;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource && bgmSource.isPlaying)
            bgmSource.Stop();
    }

    // 📼 테이프 지지직 → 덜컥 → 윙 시퀀스
    public void PlayTapeSequence()
    {
        if (tapeRoutine != null)
            StopCoroutine(tapeRoutine);
        tapeRoutine = StartCoroutine(TapeSequenceRoutine());
    }

    private IEnumerator TapeSequenceRoutine()
    {
        var db = AudioManager.Instance?.soundDB;
        if (db == null)
        {
            Debug.LogWarning("⚠️ SoundDB가 존재하지 않습니다!");
            yield break;
        }

        var ziziziClip = db.tapeZiziziSFX; // 지지직
        var deckClip = db.tapeDeckSFX;     // 덜컥
        var playClip = db.tapePlaySFX;     // 윙—

        // 1️⃣ 지지직
        if (ziziziClip != null)
        {
            sfxSource.PlayOneShot(ziziziClip);
            Debug.Log("⚡ 테이프 지지직 소리 재생");
        }
        yield return new WaitForSeconds(0.6f);

        // 2️⃣ 덜컥
        if (deckClip != null)
        {
            sfxSource.PlayOneShot(deckClip);
            Debug.Log("📼 테이프 덜컥 소리");
        }
        yield return new WaitForSeconds(0.4f);

        // 3️⃣ 윙— (루프 시작)
        if (playClip != null)
        {
            tapePlaySource.clip = playClip;
            tapePlaySource.loop = true;
            tapePlaySource.Play();
            Debug.Log("📼 테이프 회전 소리 (루프 시작)");
        }
    }

    // ⏹️ 전체 테이프 사운드 정지
    public void StopTapeAll()
    {
        if (tapeRoutine != null)
        {
            StopCoroutine(tapeRoutine);
            tapeRoutine = null;
        }

        if (tapePlaySource != null && tapePlaySource.isPlaying)
            tapePlaySource.Stop();

        Debug.Log("⏹️ 테이프 전체 사운드 중단 완료");
    }

    // ✏️ 펜 쓰는 소리
    public void PlayPencilWrite()
    {
        var clip = AudioManager.Instance?.soundDB?.pencilWriteSFX;
        if (clip == null) return;

        if (Time.time - _lastPencilPlayTime < 0.3f)
            return;

        pencilSource.clip = clip;
        pencilSource.loop = false;
        pencilSource.Play();
        _lastPencilPlayTime = Time.time;
    }

    public void StopPencilWrite()
    {
        if (pencilSource && pencilSource.isPlaying)
            pencilSource.Stop();
    }

    // 💬 기타 SFX
    public void PlayBoxOpen()
    {
        var clip = AudioManager.Instance?.soundDB?.boxOpenSFX;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayDiaryClose()
    {
        var clip = AudioManager.Instance?.soundDB?.diaryCloseSFX;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    public void PlayMouseClick()
    {
        var clip = AudioManager.Instance?.soundDB?.objClickSFX;
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    
}
