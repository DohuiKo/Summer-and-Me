using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class Chap3SoundManager : MonoBehaviour
{
    public static Chap3SoundManager Instance;

    [Header("🔊 볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;
    private AudioSource typingSource;

    private List<TextMeshProUGUI> pwNums = new List<TextMeshProUGUI>();
    private Dictionary<TextMeshProUGUI, string> lastTexts = new Dictionary<TextMeshProUGUI, string>();
    private float _lastTypingTime = -1f;
    private bool _tapeGetPlayed = false; // ✅ 중복방지 플래그

    // ============================================================
    // 🧩 초기화
    // ============================================================
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
        typingSource = gameObject.AddComponent<AudioSource>();

        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;
        typingSource.volume = sfxVolume;

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void Start()
    {
        PlayChap3BGM();
        StartCoroutine(WatchPwSlots());
        StartCoroutine(AttachButtonClickListeners());
        StartCoroutine(WatchHiddenPanelFade()); // ✅ 자동감시
    }

    private void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    // ============================================================
    // 🧭 씬 전환 처리
    // ============================================================
    private void OnSceneChanged(Scene current, Scene next)
    {
        if (next.name.StartsWith("3_"))
        {
            StopTyping();
            StopTapeDeck();
            StartCoroutine(WatchPwSlots());
            StartCoroutine(AttachButtonClickListeners());
            StartCoroutine(WatchHiddenPanelFade()); // ✅ 유지
        }
        else StopAll();
    }

    // ============================================================
    // 🔍 PwSlot 감시 루프
    // ============================================================
    private IEnumerator WatchPwSlots()
    {
        yield return new WaitForSeconds(0.5f);
        while (true)
        {
#if UNITY_2023_1_OR_NEWER
            pwNums.Clear();
            pwNums.AddRange(FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None));
#else
            pwNums.Clear();
            pwNums.AddRange(FindObjectsOfType<TextMeshProUGUI>(true));
#endif
            foreach (var tmp in pwNums)
            {
                if (tmp.name.Equals("PwNum"))
                {
                    if (!lastTexts.ContainsKey(tmp))
                        lastTexts[tmp] = tmp.text;
                    if (tmp.text != lastTexts[tmp])
                    {
                        if (!string.IsNullOrEmpty(tmp.text))
                            PlayTyping();
                        lastTexts[tmp] = tmp.text;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ============================================================
    // 🎯 버튼 자동 연결 (Diary, Recording 등)
    // ============================================================
    private IEnumerator AttachButtonClickListeners()
    {
        yield return new WaitForSeconds(0.5f);
        Button[] buttons = FindObjectsOfType<Button>(true);
        foreach (var btn in buttons)
        {
            string btnName = btn.name.ToLower();
            if (btnName.Contains("diary") || btnName.Contains("recording") || btnName.Contains("photo"))
            {
                btn.onClick.RemoveListener(PlayMouseClick);
                btn.onClick.AddListener(PlayMouseClick);
            }
        }
    }

    // ============================================================
    // 🎁 히든패널 감시 루프 (FadeOut 순간 감지)
    // ============================================================
    private IEnumerator WatchHiddenPanelFade()
    {
        yield return new WaitForSeconds(1f);
        CanvasGroup hiddenPanel = null;
        _tapeGetPlayed = false;

        // 🔍 DrawImage 찾기
        Transform drawImage = null;
        while (drawImage == null)
        {
            foreach (var t in GameObject.FindObjectsOfType<Transform>(true))
            {
                if (t.name.ToLower() == "drawimage")
                {
                    drawImage = t;
                    Debug.Log("[Chap3SoundManager] 🗂️ DrawImage 객체 감지 완료");
                    break;
                }
            }
            if (drawImage == null)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }
        }

        // 🔍 HiddenPanel만 탐색 (pouch 무시)
        while (hiddenPanel == null)
        {
            var canvasGroups = drawImage.GetComponentsInChildren<CanvasGroup>(true);
            foreach (var cg in canvasGroups)
            {
                if (cg.name.ToLower() == "hiddenpanel")
                {
                    hiddenPanel = cg;
                    Debug.Log("[Chap3SoundManager] 🎯 HiddenPanel 감지 완료 (pouch 무시)");
                    break;
                }
            }
            yield return new WaitForSeconds(0.2f);
        }

        // ✅ HiddenPanel이 실제로 사라질 때만 획득 사운드 재생
        bool activeBefore = hiddenPanel.gameObject.activeInHierarchy;
        while (!_tapeGetPlayed)
        {
            bool nowActive = hiddenPanel.gameObject.activeInHierarchy;

            if (activeBefore && !nowActive)
            {
                PlayGetTapePiece();
                _tapeGetPlayed = true;
                Debug.Log("[Chap3SoundManager] ✅ HiddenPanel FadeOut → 획득 사운드 재생");
            }

            activeBefore = nowActive;
            yield return new WaitForSeconds(0.1f);
        }
    }


    // ============================================================
    // 🎵 사운드 관련
    // ============================================================
    public void PlayChap3BGM()
    {
        var clip = AudioManager.Instance?.soundDB?.chap3BGM;
        if (clip == null) return;
        if (bgmSource.isPlaying && bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void PlayMouseClick()
    {
        var clip = AudioManager.Instance?.soundDB?.objClickSFX;
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayDragStart()
    {
        var clip = AudioManager.Instance?.soundDB?.objClickSFX;
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void PlayTyping()
    {
        if (Time.time - _lastTypingTime < 0.05f) return;
        var clip = AudioManager.Instance?.soundDB?.typingSFX;
        if (clip == null) return;
        typingSource.pitch = Random.Range(0.95f, 1.05f);
        typingSource.PlayOneShot(clip);
        _lastTypingTime = Time.time;
    }

    public void StopTyping()
    {
        if (typingSource.isPlaying)
            typingSource.Stop();
    }

    public void PlayTapeSpinSequence()
    {
        StartCoroutine(TapeSpinRoutine());
    }

    private IEnumerator TapeSpinRoutine()
    {
        var ziziziClip = AudioManager.Instance?.soundDB?.tapeZiziziSFX;
        var wingClip = AudioManager.Instance?.soundDB?.tapeDeckSFX;
        if (ziziziClip == null || wingClip == null) yield break;

        sfxSource.volume = Mathf.Clamp(sfxVolume * 1.3f, 0f, 1f);
        for (int i = 0; i < 2; i++)
        {
            sfxSource.PlayOneShot(ziziziClip);
            yield return new WaitForSeconds(ziziziClip.length * 0.6f);
        }
        yield return new WaitForSeconds(0.2f);
        sfxSource.volume = sfxVolume;
        sfxSource.PlayOneShot(wingClip);
    }

    public void StopTapeDeck()
    {
        if (sfxSource != null && sfxSource.isPlaying)
            sfxSource.Stop();
    }

    public void PlayGetTapePiece()
    {
        var clip = AudioManager.Instance?.soundDB?.getTapePieceSFX;
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void StopAll()
    {
        StopTyping();
        StopTapeDeck();
        StopBGM();
    }
}
