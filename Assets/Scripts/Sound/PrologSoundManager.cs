using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class PrologSoundManager : MonoBehaviour
{
    public static PrologSoundManager Instance { get; private set; }

    [Header("ScrollRect 설정")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform viewport;

    [Header("페이지 및 사운드 매핑")]
    [SerializeField] private List<RectTransform> pages = new List<RectTransform>();
    [SerializeField] private List<AudioClip> sfxClips = new List<AudioClip>();

    [Header("감지 설정")]
    [Range(0.05f, 0.3f)] [SerializeField] private float centerTolerance = 0.15f;
    [SerializeField] private float detectionDelay = 0.7f;

    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float maxSFXVolume = 0.4f;
    [SerializeField] private float fadeDuration = 1.0f;

    private Dictionary<RectTransform, AudioSource> activeSources = new Dictionary<RectTransform, AudioSource>();
    private Dictionary<RectTransform, bool> pageCenterState = new Dictionary<RectTransform, bool>();
    private bool canDetectCenter = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;  // 씬 변경 감지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    IEnumerator Start()
    {
        for (int i = 0; i < pages.Count && i < sfxClips.Count; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = true;
            src.volume = 0f;
            src.clip = sfxClips[i];
            activeSources[pages[i]] = src;
            pageCenterState[pages[i]] = false;
        }

        yield return new WaitForSeconds(detectionDelay);
        foreach (var page in pages)
            pageCenterState[page] = IsPageInCenter(page);

        canDetectCenter = true;
    }

    void Update()
    {
        if (!canDetectCenter) return;
        CheckAllPagesCenter();
    }

    void CheckAllPagesCenter()
    {
        foreach (var page in pages)
        {
            bool isInCenter = IsPageInCenter(page);

            if (isInCenter && !pageCenterState[page])
            {
                StartCoroutine(FadeInSFX(page));
                pageCenterState[page] = true;
            }
            else if (!isInCenter && pageCenterState[page])
            {
                StartCoroutine(FadeOutSFX(page));
                pageCenterState[page] = false;
            }
        }
    }

    bool IsPageInCenter(RectTransform page)
    {
        Vector3[] pageCorners = new Vector3[4];
        page.GetWorldCorners(pageCorners);
        float pageMiddleY = (pageCorners[0].y + pageCorners[1].y) / 2f;

        Vector3[] viewportCorners = new Vector3[4];
        viewport.GetWorldCorners(viewportCorners);
        float viewportMiddleY = (viewportCorners[0].y + viewportCorners[1].y) / 2f;

        float distance = Mathf.Abs(viewportMiddleY - pageMiddleY);
        float threshold = viewport.rect.height * centerTolerance;
        return distance < threshold;
    }

    IEnumerator FadeInSFX(RectTransform page)
    {
        if (!activeSources.ContainsKey(page)) yield break;
        AudioSource src = activeSources[page];
        if (!src.isPlaying) src.Play();

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(0f, maxSFXVolume, t / fadeDuration);
            yield return null;
        }
        src.volume = maxSFXVolume;
    }

    IEnumerator FadeOutSFX(RectTransform page)
    {
        if (!activeSources.ContainsKey(page)) yield break;
        AudioSource src = activeSources[page];

        float startVol = src.volume;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / fadeDuration);
            yield return null;
        }

        src.volume = 0f;
        src.Stop();
    }

    public void PlayPrologBGM()
    {
        if (AudioManager.Instance == null) return;
        var bgmClip = AudioManager.Instance.soundDB?.prologBGM;
        if (bgmClip == null) return;
        AudioManager.Instance.PlayBGM(bgmClip);
    }

    public void PlayCoffee()
    {
        if (AudioManager.Instance == null) return;
        var clip = AudioManager.Instance.soundDB?.coffeeSFX;
        if (clip == null) return;
        AudioManager.Instance.PlaySFX(clip);
    }

    public void PlayObjClick()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayObjClick();
    }

    public void PlayGetTapePiece()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayGetTapePiece();
    }

    // 🔻 씬 전환 시 사운드 완전 종료
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StopAllSounds();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAllSounds();
    }

    private void StopAllSounds()
    {
        foreach (var src in activeSources.Values)
        {
            if (src == null) continue;
            src.Stop();
            src.volume = 0f;
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.StopBGM();

        Debug.Log("🔇 프롤로그 사운드 완전 종료");
    }
}
