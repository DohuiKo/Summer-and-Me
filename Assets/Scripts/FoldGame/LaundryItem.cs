using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class LaundryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("스테이트 데이터 (Sprite + Size)")]
    [SerializeField] private LaundryPile.LaundryStateData[] foldingStates;

    [Header("사운드 DB")]
    [SerializeField] private SoundDatabase soundDB;   // ★ SoundDatabase 참조 (LaundryFailSFX 사용)

    [Header("펄스/페이드 연출")]
    [SerializeField] private bool  usePulse        = true;
    [SerializeField] private float pulseScaleUp    = 1.08f;
    [SerializeField] private float pulseDuration   = 0.08f;
    [SerializeField] private float stateFadeTime   = 0.06f;
    [SerializeField] private float spawnFadeTime   = 0.15f;

    [Header("완료 후 연출")]
    [SerializeField] private bool  revertAfterComplete = true; // 다 접은 뒤 역재생 연출
    [SerializeField] private float revertDelay         = 0.25f;
    [SerializeField] private float revertStepTime      = 0.06f;
    [SerializeField] private float revertDimFactor     = 0.75f;
    [SerializeField] private float endFadeOutTime      = 0.25f;
    [SerializeField] private bool  destroyAfterRevert  = true;

    private Image         itemImage;
    private Canvas        canvas;
    private CanvasGroup   canvasGroup;
    private RectTransform rectTransform;
    private Vector2       originalPosition;
    private Transform     originalParent;
    private Vector2       originalSize;

    private int  currentState = 0;
    private bool isInFoldingArea = false;
    private bool isFolding       = false;

    private FoldingArea currentFoldingArea;

    void Awake()
    {
        itemImage     = GetComponent<Image>();
        canvas        = GetComponentInParent<Canvas>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();

        originalSize = rectTransform.sizeDelta;

        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;
        itemImage.raycastTarget    = true;

        Debug.Log("[LaundryItem] Awake: soundDB=" + (soundDB ? soundDB.name : "NULL"));
    }

    /// <summary>
    /// LaundryPile에서 호출하는 초기화
    /// </summary>
    public void Initialize(LaundryPile.LaundryStateData[] states, FoldingArea _)
    {
        foldingStates = states;
        currentState  = 0;

        ApplyVisualState(0);

        // 스폰 페이드인
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(CoFade(canvasGroup, 0f, 1f, spawnFadeTime));
        }
    }

    /// <summary>
    /// index에 맞는 Sprite + Size 적용
    /// </summary>
    private void ApplyVisualState(int index)
    {
        if (foldingStates == null || foldingStates.Length == 0 || itemImage == null)
            return;

        index = Mathf.Clamp(index, 0, foldingStates.Length - 1);
        var data = foldingStates[index];

        // Sprite 적용
        if (data.sprite != null)
            itemImage.sprite = data.sprite;

        // Size 적용 (0,0이면 원래 사이즈 유지)
        if (data.size.x > 0f && data.size.y > 0f)
            rectTransform.sizeDelta = data.size;
        else
            rectTransform.sizeDelta = originalSize;
    }

    // ───────────── Drag & Drop ─────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isFolding) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent   = transform.parent;

        canvasGroup.blocksRaycasts = false;
        if (canvas != null)
            transform.SetParent(canvas.transform, true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isFolding) return;

        float scale = (canvas != null ? canvas.scaleFactor : 1f);
        rectTransform.anchoredPosition += eventData.delta / scale;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isInFoldingArea)
        {
            transform.SetParent(originalParent, true);
            rectTransform.anchoredPosition = originalPosition;
        }
        canvasGroup.blocksRaycasts = true;
    }

    // 드롭 영역에서 호출
    public void OnPlacedInArea(FoldingArea area)
    {
        if (currentFoldingArea != null)
            currentFoldingArea.ClearCurrentItem();

        isInFoldingArea    = true;
        currentFoldingArea = area;
        area.SetCurrentItem(this);

        var my = GetComponent<RectTransform>();
        my.SetParent(area.GetRectTransform(), false);
        my.anchoredPosition = Vector2.zero;
        my.localRotation    = Quaternion.identity;
        my.localScale       = Vector3.one;

        LaundryGameManager.Instance?.UpdateInstructionText();
    }

    // ───────────── 접기 로직 ─────────────
    public void FoldNext()
    {
        if (!isInFoldingArea) return;
        if (isFolding)        return;
        if (foldingStates == null || foldingStates.Length == 0) return;
        if (currentState >= foldingStates.Length - 1) return;

        StartCoroutine(FoldAnimation());
    }

    private IEnumerator FoldAnimation()
    {
        isFolding = true;

        // 1) 펄스 업
        if (usePulse)
            yield return StartCoroutine(CoScale(rectTransform, Vector3.one, Vector3.one * pulseScaleUp, pulseDuration));

        // 2) 페이드 다운 → 상태 변경 → 페이드 업
        if (canvasGroup != null && stateFadeTime > 0f)
            yield return StartCoroutine(CoFade(canvasGroup, 1f, 0.7f, stateFadeTime * 0.5f));

        currentState = Mathf.Clamp(currentState + 1, 0, foldingStates.Length - 1);
        ApplyVisualState(currentState);

        if (canvasGroup != null && stateFadeTime > 0f)
            yield return StartCoroutine(CoFade(canvasGroup, 0.7f, 1f, stateFadeTime * 0.5f));

        // 3) 펄스 다운
        if (usePulse)
            yield return StartCoroutine(CoScale(rectTransform, Vector3.one * pulseScaleUp, Vector3.one, pulseDuration));

        // 마지막 단계 → 완료 연출
        if (currentState >= foldingStates.Length - 1)
        {
            Debug.Log("[LaundryItem] Reached last state, revertAfterComplete=" + revertAfterComplete);

            // 점수/클리어 처리
            LaundryGameManager.Instance?.OnLaundryCompleted();

            // 접는 영역 비우기
            currentFoldingArea?.ClearCurrentItem();

            // 상호작용 비활성
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable   = false;

            if (revertAfterComplete)
            {
                // ★ 되돌아가기 시작할 때 실패 사운드 재생
                PlayRevertFailSound();

                if (revertDelay > 0f)
                    yield return new WaitForSeconds(revertDelay);

                Color orig = itemImage.color;
                Color dim  = new Color(
                    orig.r * revertDimFactor,
                    orig.g * revertDimFactor,
                    orig.b * revertDimFactor,
                    orig.a
                );

                // 마지막 상태 → 0까지 역재생
                for (int i = currentState; i >= 0; i--)
                {
                    itemImage.color = dim;
                    ApplyVisualState(i);

                    if (revertStepTime > 0f)
                        yield return new WaitForSeconds(revertStepTime);

                    itemImage.color = orig;
                }

                // 마지막엔 서서히 페이드 아웃
                if (endFadeOutTime > 0f)
                    yield return StartCoroutine(CoFade(canvasGroup, canvasGroup.alpha, 0f, endFadeOutTime));
            }

            if (destroyAfterRevert)
                Destroy(gameObject);

            yield break;
        }

        isFolding = false;
        LaundryGameManager.Instance?.UpdateInstructionText();
    }

    // ───────────── 실패 사운드 재생 ─────────────
    private void PlayRevertFailSound()
    {
        if (soundDB == null)
        {
            Debug.LogWarning("[LaundryItem] PlayRevertFailSound: soundDB is NULL");
            return;
        }

        if (soundDB.laundryFailSFX == null)
        {
            Debug.LogWarning("[LaundryItem] PlayRevertFailSound: soundDB.laundryFailSFX is NULL");
            return;
        }

        // 카메라 위치에서 재생 (2D 느낌)
        Vector3 pos = Vector3.zero;
        if (Camera.main != null)
            pos = Camera.main.transform.position;

        Debug.Log("[LaundryItem] PlayRevertFailSound: playing clip " + soundDB.laundryFailSFX.name);
        AudioSource.PlayClipAtPoint(soundDB.laundryFailSFX, pos, 1f);
    }

    // ───────────── Helpers ─────────────
    private IEnumerator CoScale(RectTransform target, Vector3 from, Vector3 to, float time)
    {
        if (target == null || time <= 0f)
        {
            if (target) target.localScale = to;
            yield break;
        }

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            target.localScale = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        target.localScale = to;
    }

    private IEnumerator CoFade(CanvasGroup cg, float from, float to, float time)
    {
        if (cg == null || time <= 0f)
        {
            if (cg) cg.alpha = to;
            yield break;
        }

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / time);
            cg.alpha = Mathf.LerpUnclamped(from, to, k);
            yield return null;
        }
        cg.alpha = to;
    }

    // ───────────── Getters ─────────────
    public bool IsFolding()       => isFolding;
    public int  GetCurrentState() => currentState;
    public int  GetMaxState()     => (foldingStates != null ? foldingStates.Length - 1 : 0);
}
