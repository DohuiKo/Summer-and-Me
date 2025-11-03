using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class LaundryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("스테이트 스프라이트 (0 -> 마지막)")]
    [SerializeField] private Sprite[] foldingStates;

    [Header("펄스/페이드 연출")]
    [SerializeField] private bool  usePulse        = true;
    [SerializeField] private float pulseScaleUp    = 1.08f;
    [SerializeField] private float pulseDuration   = 0.08f;
    [SerializeField] private float stateFadeTime   = 0.06f;
    [SerializeField] private float spawnFadeTime   = 0.15f;

    [Header("완료 후 연출")]
    [SerializeField] private bool  revertAfterComplete = true; // ✅ 다 접은 뒤 원래대로 되돌아가는 연출
    [SerializeField] private float revertDelay         = 0.25f; // 접기 완료 후 잠깐의 정적
    [SerializeField] private float revertStepTime      = 0.06f; // 역재생 단계당 시간
    [SerializeField] private float revertDimFactor     = 0.75f; // 역재생 중 살짝 흐려짐(밝기)
    [SerializeField] private float endFadeOutTime      = 0.25f; // 역재생 끝나고 사라지는 시간
    [SerializeField] private bool  destroyAfterRevert  = true;  // 끝나면 제거(권장)

    private Image itemImage;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Vector2 originalPosition;
    private Transform originalParent;

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

        canvasGroup.interactable   = true;
        canvasGroup.blocksRaycasts = true;
        itemImage.raycastTarget    = true;
    }

    public void Initialize(Sprite[] states, FoldingArea _)
    {
        foldingStates = states;
        currentState  = 0;

        if (itemImage != null && foldingStates != null && foldingStates.Length > 0)
            itemImage.sprite = foldingStates[0];

        // 스폰 페이드인
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(CoFade(canvasGroup, 0f, 1f, spawnFadeTime));
        }
    }

    // ── Drag & Drop ─────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isFolding) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent   = transform.parent;

        canvasGroup.blocksRaycasts = false;
        if (canvas != null) transform.SetParent(canvas.transform, true);
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
        if (currentFoldingArea != null) currentFoldingArea.ClearCurrentItem();

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

    // ── Folding ────────────────────────────────────────────
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
        if (usePulse) yield return StartCoroutine(CoScale(rectTransform, Vector3.one, Vector3.one * pulseScaleUp, pulseDuration));

        // 2) 페이드 다운 -> 스프라이트 변경 -> 페이드 업
        if (canvasGroup != null && stateFadeTime > 0f)
            yield return StartCoroutine(CoFade(canvasGroup, 1f, 0.7f, stateFadeTime * 0.5f));

        currentState = Mathf.Clamp(currentState + 1, 0, foldingStates.Length - 1);
        if (itemImage != null) itemImage.sprite = foldingStates[currentState];

        if (canvasGroup != null && stateFadeTime > 0f)
            yield return StartCoroutine(CoFade(canvasGroup, 0.7f, 1f, stateFadeTime * 0.5f));

        // 3) 펄스 다운
        if (usePulse) yield return StartCoroutine(CoScale(rectTransform, Vector3.one * pulseScaleUp, Vector3.one, pulseDuration));

        // 마지막 단계라면 완료→역재생
        if (currentState >= foldingStates.Length - 1)
        {
            // 게임 진행 신호는 즉시 보냄(점수/클리어 처리)
            LaundryGameManager.Instance?.OnLaundryCompleted();

            // 접는 영역은 즉시 비워 다음 행동 가능하게
            currentFoldingArea?.ClearCurrentItem();

            // 더 이상 상호작용 불가
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable   = false;

            if (revertAfterComplete)
            {
                // 잠깐 정적
                if (revertDelay > 0f) yield return new WaitForSeconds(revertDelay);

                // 역재생 동안 살짝 색 빠지게(무력감)
                Color orig = itemImage.color;
                Color dim  = new Color(orig.r * revertDimFactor, orig.g * revertDimFactor, orig.b * revertDimFactor, orig.a);

                // 마지막 상태에서 처음(0)까지 역재생
                for (int i = currentState; i >= 0; i--)
                {
                    if (canvasGroup != null && revertStepTime > 0f)
                        yield return StartCoroutine(CoFade(canvasGroup, 1f, 1f, revertStepTime * 1f));

                    itemImage.color  = dim;
                    itemImage.sprite = foldingStates[i];

                    if (canvasGroup != null && revertStepTime > 0f)
                        yield return StartCoroutine(CoFade(canvasGroup, 1f, 1f, revertStepTime * 1f));

                    itemImage.color = orig;
                }

                // 마지막엔 서서히 사라짐
                if (endFadeOutTime > 0f)
                    yield return StartCoroutine(CoFade(canvasGroup, canvasGroup.alpha, 2f, endFadeOutTime));
            }

            if (destroyAfterRevert) Destroy(gameObject);
            yield break;
        }

        isFolding = false;
        LaundryGameManager.Instance?.UpdateInstructionText();
    }

    // ── Helpers ──────────────────────────────────────────────
    private IEnumerator CoScale(RectTransform target, Vector3 from, Vector3 to, float time)
    {
        if (target == null || time <= 0f) { if (target) target.localScale = to; yield break; }
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
        if (cg == null || time <= 0f) { if (cg) cg.alpha = to; yield break; }
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

    // getters
    public bool IsFolding() => isFolding;
    public int GetCurrentState() => currentState;
    public int GetMaxState() => (foldingStates != null ? foldingStates.Length - 1 : 0);
}
