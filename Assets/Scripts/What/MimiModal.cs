using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 스크롤락/트리거 시 모달을 띄우고
/// (1)기본 → (2)오픈 → (3)테이프삽입 → (4)닫힘 스프라이트를 순서대로 보여준 뒤
/// 비디오를 재생한다(옵션).
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class MimiModal : MonoBehaviour
{
    // ====== 외부에서 구독 가능한 이벤트 ======
    public event Action OnSequenceEnd;   // 시퀀스가 모두 끝났을 때
    public event Action OnVideoStarted;  // 비디오가 시작될 때

    [Header("Modal Root")]
    public CanvasGroup modalCanvasGroup;
    [Tooltip("배경 입력을 막을지 여부")]
    public bool blockBackgroundInput = true;

    [Header("Sequence Target")]
    [Tooltip("순차 이미지가 표시될 UI Image")]
    public Image sequenceImage;

    [Tooltip("순차 노출할 스프라이트 4장: (1)기본 (2)오픈 (3)테이프삽입 (4)닫힘")]
    public List<Sprite> sequenceSprites = new List<Sprite>(4);

    [Tooltip("각 스프라이트 유지 시간(초). 길이가 1이면 모든 단계 동일 시간 사용")]
    public List<float> stepDurations = new List<float> { 0.6f, 0.6f, 0.6f, 0.6f };

    [Tooltip("스프라이트 전환 크로스페이드 시간(초). 0이면 즉시 교체")]
    [Min(0f)] public float crossFadeTime = 0.15f;

    [Header("Video")]
    [Tooltip("시퀀스 이후 재생할 비디오 플레이어")]
    public VideoPlayer videoPlayer;

    [Tooltip("비디오를 표시할 RawImage (RenderTexture 출력용)")]
    public RawImage videoRawImage;

    [Tooltip("비디오 시작 시 RawImage 알파 0→1 페이드 시간")]
    public float videoFadeInTime = 0.25f;

    [Tooltip("시퀀스 끝나면 자동으로 비디오 재생")]
    public bool autoPlayVideoAfterSequence = true;

    [Tooltip("비디오 재생 전에 모달을 닫을지(별도 레이어에서 영상 띄울 때)")]
    public bool closeModalBeforeVideo = false;

    [Header("SFX (선택)")]
    public AudioSource sfx;
    public AudioClip sfxOpen;    // (2) 오픈 순간
    public AudioClip sfxInsert;  // (3) 삽입 순간
    public AudioClip sfxClose;   // (4) 닫힘 순간
    public AudioClip sfxStart;   // 비디오 시작 직전 효과음

    [Header("Scroll Lock (선택)")]
    [Tooltip("스크롤을 잠그고 싶다면 연결 (없으면 무시)")]
    public ScrollRect scrollRectToLock;

    [Header("Modal Fade")]
    public float modalFadeInTime = 0.2f;
    public float modalFadeOutTime = 0.2f;

    [Header("Debug")]
    public bool autoPlayOnStart = false;

    bool isPlaying;
    int playedCount = 0; // 비디오 재생 횟수

    void Reset()
    {
        modalCanvasGroup = GetComponent<CanvasGroup>();
        if (!sequenceImage) sequenceImage = GetComponentInChildren<Image>(true);
    }

    void Awake()
    {
        if (!modalCanvasGroup) modalCanvasGroup = GetComponent<CanvasGroup>();

        // 오브젝트는 항상 활성(Active)로 두고,
        // 알파/레이캐스트로만 숨김 처리(코루틴 에러 방지)
        modalCanvasGroup.alpha = 0f;
        modalCanvasGroup.blocksRaycasts = false;
        modalCanvasGroup.interactable = false;

        if (videoRawImage) videoRawImage.color = new Color(1, 1, 1, 0);
        if (videoPlayer)
        {
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
        }
    }

    void Start()
    {
        if (autoPlayOnStart) PlayAutoSequence();
    }

    /// <summary>외부 트리거(스크롤락 등)에서 호출</summary>
    public void PlayAutoSequence()
    {
        if (isPlaying) return;

        // 컴포넌트/오브젝트 활성 보장
        if (!enabled) enabled = true;
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // 부모가 방금 켜졌을 수 있으므로 한 프레임 대기 후 본 코루틴 시작
        StartCoroutine(Co_PlaySafely());
    }

    IEnumerator Co_PlaySafely()
    {
        yield return null; // 활성 계층 반영 대기

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[MimiModal] activeInHierarchy=false (부모 비활성). 부모를 먼저 활성화한 뒤 호출하세요.");
            yield break;
        }

        StartCoroutine(Co_Play());
    }

    IEnumerator Co_Play()
    {
        isPlaying = true;
        playedCount = 0; // 비디오 재생 횟수 리셋

        if (scrollRectToLock) scrollRectToLock.enabled = false;

        // 모달 페이드 인
        yield return StartCoroutine(Co_FadeCanvasGroup(modalCanvasGroup, 0f, 1f, modalFadeInTime));

        if (blockBackgroundInput)
        {
            modalCanvasGroup.blocksRaycasts = true;
            modalCanvasGroup.interactable = true;
        }

        // 순차 이미지
        if (sequenceImage && sequenceSprites != null && sequenceSprites.Count >= 4)
        {
            yield return StartCoroutine(Co_ShowSprite(sequenceSprites[0], GetStepDuration(0)));
            PlaySfx(sfxOpen);
            yield return StartCoroutine(Co_ShowSprite(sequenceSprites[1], GetStepDuration(1)));
            PlaySfx(sfxInsert);
            yield return StartCoroutine(Co_ShowSprite(sequenceSprites[2], GetStepDuration(2)));
            PlaySfx(sfxClose);
            yield return StartCoroutine(Co_ShowSprite(sequenceSprites[3], GetStepDuration(3)));
        }

        OnSequenceEnd?.Invoke();

        // 비디오 재생
        if (autoPlayVideoAfterSequence && videoPlayer)
        {
            if (closeModalBeforeVideo)
            {
                yield return StartCoroutine(Co_CloseInternal());
                yield return StartCoroutine(Co_StartVideo());
            }
            else
            {
                yield return StartCoroutine(Co_StartVideo());
            }
        }
    }

    // ====== 비디오 재생/루프 보장 ======
    IEnumerator Co_StartVideo()
    {
        if (sfxStart) PlaySfx(sfxStart);

        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared) yield return null;

        // Prepare 이후 루프 재설정(환경별 루프 풀림 방지)
        videoPlayer.isLooping = true;

        // 끝에 도달 시 다시 재생(2회까지만) — 필요시 숫자 변경 가능
        videoPlayer.loopPointReached -= OnVideoEndReplay; // 중복 등록 방지
        videoPlayer.loopPointReached += OnVideoEndReplay;

        if (videoRawImage)
        {
            var c = videoRawImage.color; c.a = 0f; videoRawImage.color = c;
        }

        videoPlayer.Play();
        OnVideoStarted?.Invoke();

        if (videoRawImage)
            yield return StartCoroutine(Co_FadeRawImage(videoRawImage, 0f, 1f, videoFadeInTime));
    }

    void OnVideoEndReplay(VideoPlayer vp)
    {
        playedCount++;
        if (playedCount < 2)
        {
            // 두 번째 재생
            vp.Play();
        }
        else
        {
            // 2회 재생 후 종료(원하면 여기서 모달 닫기)
            vp.loopPointReached -= OnVideoEndReplay;
            vp.isLooping = false;
            // 예: StartCoroutine(Co_CloseInternal());
        }
    }

    // ====== 유틸 ======
    IEnumerator Co_ShowSprite(Sprite sprite, float holdTime)
    {
        if (!sequenceImage) yield break;

        if (crossFadeTime <= 0f)
        {
            sequenceImage.sprite = sprite;
            if (holdTime > 0f) yield return new WaitForSeconds(holdTime);
            yield break;
        }

        var parent = sequenceImage.transform.parent;
        var tempGO = new GameObject("TempPrev", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var temp = tempGO.GetComponent<Image>();
        var rt = tempGO.GetComponent<RectTransform>();

        temp.sprite = sequenceImage.sprite;
        temp.preserveAspect = sequenceImage.preserveAspect;
        temp.raycastTarget = false;

        rt.SetParent(parent, false);
        rt.anchorMin = sequenceImage.rectTransform.anchorMin;
        rt.anchorMax = sequenceImage.rectTransform.anchorMax;
        rt.pivot = sequenceImage.rectTransform.pivot;
        rt.sizeDelta = sequenceImage.rectTransform.sizeDelta;
        rt.anchoredPosition = sequenceImage.rectTransform.anchoredPosition;
        rt.localScale = sequenceImage.rectTransform.localScale;
        temp.color = new Color(1, 1, 1, 1);

        sequenceImage.sprite = sprite;

        float t = 0f;
        while (t < crossFadeTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / crossFadeTime);
            temp.color = new Color(1, 1, 1, 1f - k);
            yield return null;
        }
        Destroy(tempGO);

        if (holdTime > 0f) yield return new WaitForSeconds(holdTime);
    }

    float GetStepDuration(int idx)
    {
        if (stepDurations == null || stepDurations.Count == 0) return 0.6f;
        if (stepDurations.Count == 1) return stepDurations[0];
        if (idx < 0 || idx >= stepDurations.Count) return stepDurations[stepDurations.Count - 1];
        return stepDurations[idx];
    }

    IEnumerator Co_FadeCanvasGroup(CanvasGroup cg, float a, float b, float time)
    {
        if (!cg || time <= 0f)
        {
            if (cg) cg.alpha = b;
            yield break;
        }

        float t = 0f;
        cg.alpha = a;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(a, b, t / time);
            yield return null;
        }
        cg.alpha = b;
    }

    IEnumerator Co_FadeRawImage(RawImage img, float a, float b, float time)
    {
        if (!img || time <= 0f)
        {
            if (img) img.color = new Color(1, 1, 1, b);
            yield break;
        }
        float t = 0f;
        var c = img.color;
        c.a = a; img.color = c;

        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            c.a = Mathf.Lerp(a, b, k);
            img.color = c;
            yield return null;
        }
        c.a = b; img.color = c;
    }

    void PlaySfx(AudioClip clip)
    {
        if (!sfx || !clip) return;
        sfx.PlayOneShot(clip);
    }

    public void CloseModal() => StartCoroutine(Co_CloseInternal());

    IEnumerator Co_CloseInternal()
    {
        yield return StartCoroutine(Co_FadeCanvasGroup(modalCanvasGroup, modalCanvasGroup.alpha, 0f, modalFadeOutTime));
        modalCanvasGroup.blocksRaycasts = false;
        modalCanvasGroup.interactable = false;
        gameObject.SetActive(false);
        if (scrollRectToLock) scrollRectToLock.enabled = true;
        isPlaying = false;
    }
}
