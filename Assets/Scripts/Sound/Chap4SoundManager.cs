using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 🎧 Chapter 4 전용 사운드 매니저
/// - AudioManager에서 사운드 리소스를 직접 호출
/// - 5챕터 진입 시 BGM 완전 중단 및 매니저 제거
/// - 마이마이 비디오 감지 시 BGM 교체
/// - RoomMainPage 중앙 도달 시 알람 재생
/// </summary>
public class Chap4SoundManager : MonoBehaviour
{
    public static Chap4SoundManager Instance;

    private bool mimiPlaySoundTriggered = false;
    private bool alarmTriggered = false;
    private VideoPlayer mimiPlayer;
    private RectTransform roomMainPage;
    private RectTransform viewport;

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
    }

    void Start()
    {
        TryPlayChap4BGM();
        StartCoroutine(WatchMimiVideoPlay());
        StartCoroutine(WatchRoomMainPageCenter());
        StartCoroutine(WatchMirrorBrokenAuto());
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =============================================================
    // 🎵 BGM 관련
    // =============================================================
    public void TryPlayChap4BGM()
    {
        if (AudioManager.Instance == null)
        {
            StartCoroutine(DelayedTryPlayChap4BGM());
            return;
        }

        var clip = AudioManager.Instance.soundDB?.chap4BGM;
        if (clip == null) return;

        if (!AudioManager.Instance.IsBGMPlaying(clip))
        {
            AudioManager.Instance.PlayChap4BGM();
            Debug.Log("[Chap4SoundManager] 챕터4 BGM 시작");
        }
    }

    private IEnumerator DelayedTryPlayChap4BGM()
    {
        yield return new WaitForSeconds(0.5f);
        TryPlayChap4BGM();
    }

    public void PlayChap6BGM()
    {
        AudioManager.Instance.PlayChap6BGM();
        Debug.Log("[Chap4SoundManager] Chap6 (마이마이 BGM) 시작");
    }

    // =============================================================
    // 🎚️ 사운드 이펙트
    // =============================================================
    public void PlayAlarmPipipipi() => AudioManager.Instance.PlayAlarmPipipipi();
    public void PlayFoldLaundry() => AudioManager.Instance.PlayFoldLaundry();
    public void PlayMirrorBroken() => AudioManager.Instance.PlayMirrorBroken();
    public void PlayMymyWinding() => AudioManager.Instance.PlayMymyWinding();
    public void PlayCassetteGoingIn() => AudioManager.Instance.PlayCassetteGoingIn();
    public void PlayMymyOpen() => AudioManager.Instance.PlayMymyOpen();
    public void PlayMymyDoorClose() => AudioManager.Instance.PlayMymyDoorClose();

    // =============================================================
    // 🪞 거울 깨짐 자동 감지
    // =============================================================
    private IEnumerator WatchMirrorBrokenAuto()
    {
        yield return new WaitForSeconds(0.3f);

        GameObject brokenMirrorObj = GameObject.Find("BrokenMirror");
        if (brokenMirrorObj == null)
        {
            Debug.LogWarning("[Chap4SoundManager] BrokenMirror 오브젝트를 찾을 수 없습니다.");
            yield break;
        }

        bool wasActive = brokenMirrorObj.activeSelf;
        while (true)
        {
            if (!wasActive && brokenMirrorObj.activeSelf)
            {
                PlayMirrorBroken();
                Debug.Log("[Chap4SoundManager] 거울 깨짐 사운드 재생");
                yield break;
            }

            wasActive = brokenMirrorObj.activeSelf;
            yield return null;
        }
    }

    // =============================================================
    // 🎬 MimiModal 시퀀스 단계별 사운드
    // =============================================================
    public void OnMimiSequenceChanged(int index)
    {
        switch (index)
        {
            case 1: PlayMymyOpen(); break;
            case 2: PlayCassetteGoingIn(); break;
            case 3: PlayMymyDoorClose(); break;
        }
    }

    // =============================================================
    // 🎥 비디오 감시 및 사운드 전환
    // =============================================================
    private IEnumerator WatchMimiVideoPlay()
    {
        yield return new WaitForSeconds(0.5f);

        mimiPlayer = FindObjectOfType<VideoPlayer>(true);
        if (mimiPlayer == null)
        {
            Debug.LogWarning("[Chap4SoundManager] MimiPlayer를 찾을 수 없습니다.");
            yield break;
        }

        while (mimiPlayer != null)
        {
            if (mimiPlayer.isPlaying && !mimiPlaySoundTriggered)
            {
                AudioManager.Instance.StopBGM();
                PlayMymyWinding();
                PlayChap6BGM();

                mimiPlaySoundTriggered = true;
                Debug.Log("[Chap4SoundManager] 마이마이 비디오 감지 → 회전 + BGM 교체 완료");
            }

            yield return null;
        }
    }

    // =============================================================
    // 🧭 RoomMainPage 중앙 도달 감지
    // =============================================================
    private IEnumerator WatchRoomMainPageCenter()
    {
        yield return new WaitUntil(() => FindRoomTargets());
        while (!alarmTriggered)
        {
            if (roomMainPage == null || viewport == null) yield break;

            float distance = Mathf.Abs(roomMainPage.anchoredPosition.y);

            if (distance < 10f)
            {
                PlayAlarmPipipipi();
                alarmTriggered = true;
                Debug.Log("[Chap4SoundManager] RoomMainPage 중앙 도달 → 알람 삐삐삐삐 재생");
            }

            yield return null;
        }
    }

    private bool FindRoomTargets()
    {
        if (roomMainPage != null && viewport != null) return true;

        foreach (var obj in Resources.FindObjectsOfTypeAll<RectTransform>())
        {
            if (obj.name == "RoomMainPage") roomMainPage = obj;
            if (obj.name == "Viewport") viewport = obj;
        }

        return (roomMainPage != null && viewport != null);
    }

    // =============================================================
    // 🚪 씬 이동 감지 및 사운드 정리
    // =============================================================
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name.ToLower();

        // ✅ 5챕터 또는 이후 챕터 진입 시
        if (sceneName.Contains("chapter5") || sceneName.StartsWith("5") || sceneName.Contains("chap5"))
        {
            Debug.Log("[Chap4SoundManager] Chapter5 진입 감지 → BGM 및 사운드 정리 시작");
            StartCoroutine(StopSoundsAfterSceneChange());
        }
    }

    // 씬 변경 직후 프레임 타이밍 문제 방지용 코루틴
    private IEnumerator StopSoundsAfterSceneChange()
    {
        yield return new WaitForSeconds(0.2f); // 씬 로드 후 안정화 대기
        StopAllChap4Sounds();
        Destroy(gameObject);
    }

    private void StopAllChap4Sounds()
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.StopBGM();
        AudioManager.Instance.StopAllSFX();

        var bgm = AudioManager.Instance.CurrentBGM;
        if (bgm != null)
        {
            var player = AudioManager.Instance.GetSFXPlayer();
            if (player != null)
            {
                player.Stop();
                player.clip = null;
            }
        }

        mimiPlaySoundTriggered = false;
        alarmTriggered = false;

        Debug.Log("🔇 챕터4 관련 사운드 완전 종료");
    }
}
