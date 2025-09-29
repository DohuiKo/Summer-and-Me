using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Page15Footsteps : MonoBehaviour
{
    [Header("Scroll & Page")]
    public ScrollRect scrollRect;          // Canvas/Scroll View
    public RectTransform pageRoot;         // page_15
    public RectTransform lane;             // 발자국 부모(없으면 page_15)

    [Header("Templates (비활성 템플릿)")]
    public Image leftTemplate;             // foot-left
    public Image rightTemplate;            // foot-right

    [Header("Step Settings")]
    [Tooltip("스페이스를 1번 누를 때 발을 얼마나 위로 전진시키는지(px)")]
    public float verticalGap = 240f;
    [Tooltip("최대 스텝 수(도달 시 멈추고 Next 켜려면 > 0으로)")]
    public int targetSteps = 0;            // 0이면 무제한
    public Button nextButton;              // 완료 시 켜질 버튼(선택)

    [Header("Fading Footprint (자취)")]
    [Tooltip("떠난 자리의 '발자국 자취'가 사라지기까지 걸리는 시간")]
    public float markFadeDuration = 1.2f;
    [Tooltip("자취 초기 투명도(0~1)")]
    [Range(0f,1f)] public float markStartAlpha = 0.75f;
    [Tooltip("자취를 약간 작게/크게 표현하고 싶을 때 1=원본")]
    public float markScale = 1f;

    [Header("FX (optional)")]
    public AudioSource stepSfx;            // 한 번 누를 때 재생할 소리
    public CanvasGroup vignette;           // page_15 영역의 CanvasGroup
    public Text statusText;                // "상태: 대기/걷는 중"

    // 내부 상태
    GameObject plantedLeft, plantedRight;  // 현재 바닥에 디딘 '두 발'(항상 유지)
    bool leftTurn = true;                  // 이번에 움직일 발 (왼발부터 시작)
    int stepCount = 0;

    // 템플릿 기준 좌표(시작 포즈에서 사용)
    float baseLeftX, baseLeftY, baseRightX, baseRightY;

    void Awake()
    {
        if (!lane) lane = pageRoot;
        if (nextButton) nextButton.interactable = false;

        // 템플릿 좌표 기억 + 템플릿 숨김
        if (leftTemplate)
        {
            baseLeftX = leftTemplate.rectTransform.anchoredPosition.x;
            baseLeftY = leftTemplate.rectTransform.anchoredPosition.y;
            leftTemplate.gameObject.SetActive(false);
        }
        if (rightTemplate)
        {
            baseRightX = rightTemplate.rectTransform.anchoredPosition.x;
            baseRightY = rightTemplate.rectTransform.anchoredPosition.y;
            rightTemplate.gameObject.SetActive(false);
        }
    }

    void OnEnable()
    {
        if (scrollRect) scrollRect.enabled = false; // page_15 들어오면 스크롤락
        InitStandingPose();                         // 처음: 두 발 같은 Y에서 정지
        UpdateStatus(false);
        if (vignette) vignette.alpha = 0f;
    }

    void OnDisable()
    {
        if (scrollRect) scrollRect.enabled = true;
    }

    void Update()
    {
        // 스페이스 누를 때마다 한 발만 전진
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StepOnce();
        }
    }

    // --- 처음 두 발 서있는 포즈(템플릿 좌표 그대로, 같은 Y) ---
    void InitStandingPose()
    {
        ClearPlanted();

        // 시작 Y는 두 템플릿 중 같다면 그대로, 다르면 더 낮은 값으로 통일
        float startY = Mathf.Abs(baseLeftY - baseRightY) < 0.001f ? baseLeftY : Mathf.Min(baseLeftY, baseRightY);

        plantedLeft  = Instantiate(leftTemplate,  lane).gameObject;
        plantedRight = Instantiate(rightTemplate, lane).gameObject;
        plantedLeft.SetActive(true);
        plantedRight.SetActive(true);

        var lrt = plantedLeft.GetComponent<RectTransform>();
        var rrt = plantedRight.GetComponent<RectTransform>();
        lrt.anchoredPosition = new Vector2(baseLeftX,  startY);
        rrt.anchoredPosition = new Vector2(baseRightX, startY);

        leftTurn  = true;   // 왼발부터
        stepCount = 0;
    }

    // --- 핵심: 한 번 누를 때 '한 발만' 앞으로 이동 + 떠난 자리 자취 생성 ---
    void StepOnce()
    {
        if (!plantedLeft || !plantedRight) InitStandingPose();

        // 목표 걸음수 제한이 있으면 체크
        if (targetSteps > 0 && stepCount >= targetSteps)
        {
            // 마지막엔 두 발을 같은 Y로 맞춰 세워주고 Next 켜기
            AlignBothFeetSameY();
            if (nextButton) nextButton.interactable = true;
            return;
        }

        var lrt = plantedLeft.GetComponent<RectTransform>();
        var rrt = plantedRight.GetComponent<RectTransform>();

        if (leftTurn)
        {
            // 떠나는 자리(왼발 현재 위치)에 '자취' 남기고 페이드아웃
            SpawnFadingMark(leftTemplate, lrt.anchoredPosition);

            // 왼발만 앞으로 이동
            lrt.anchoredPosition = new Vector2(baseLeftX, lrt.anchoredPosition.y + verticalGap);
        }
        else
        {
            // 떠나는 자리(오른발 현재 위치)에 '자취' 남기고 페이드아웃
            SpawnFadingMark(rightTemplate, rrt.anchoredPosition);

            // 오른발만 앞으로 이동
            rrt.anchoredPosition = new Vector2(baseRightX, rrt.anchoredPosition.y + verticalGap);
        }

        // 효과음/비네트
        if (stepSfx && stepSfx.clip) stepSfx.PlayOneShot(stepSfx.clip);
        if (vignette) StartCoroutine(PulseVignette(0.35f, 0.12f)); // 살짝 어두워졌다 원복

        // 상태 갱신
        UpdateStatus(true);

        // 카운트/턴 전환
        stepCount++;
        leftTurn = !leftTurn;

        // 목표 도달 시 정지 포즈 + Next
        if (targetSteps > 0 && stepCount >= targetSteps)
        {
            AlignBothFeetSameY();
            if (nextButton) nextButton.interactable = true;
            UpdateStatus(false);
        }
    }

    // 떠난 자리의 '발자국 자취' 생성 → 천천히 사라지고 파괴
    void SpawnFadingMark(Image template, Vector2 pos)
    {
        if (!template) return;

        var mark = Instantiate(template, lane);
        mark.gameObject.SetActive(true);

        var rt = mark.rectTransform;
        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one * Mathf.Max(0.01f, markScale);

        var cg = mark.GetComponent<CanvasGroup>();
        if (!cg) cg = mark.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = Mathf.Clamp01(markStartAlpha);

        StartCoroutine(FadeAndDestroy(mark.gameObject, cg, markFadeDuration));
    }

    IEnumerator FadeAndDestroy(GameObject go, CanvasGroup cg, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            if (cg) cg.alpha = Mathf.Lerp(markStartAlpha, 0f, t / dur);
            yield return null;
        }
        if (go) Destroy(go);
    }

    // 살짝 어두워졌다가 빠르게 복귀(비네트 펄스)
    IEnumerator PulseVignette(float target, float backDur)
    {
        if (!vignette) yield break;
        vignette.alpha = target;
        float t = 0f;
        float start = target;
        while (t < backDur)
        {
            t += Time.deltaTime;
            vignette.alpha = Mathf.Lerp(start, 0f, t / backDur);
            yield return null;
        }
        vignette.alpha = 0f;
    }

    // 마무리: 두 발을 같은 Y로 정렬(서 있는 포즈)
    void AlignBothFeetSameY()
    {
        if (!plantedLeft || !plantedRight) return;
        var lrt = plantedLeft.GetComponent<RectTransform>();
        var rrt = plantedRight.GetComponent<RectTransform>();
        float targetY = Mathf.Max(lrt.anchoredPosition.y, rrt.anchoredPosition.y); // 더 앞선 Y로 맞춤
        lrt.anchoredPosition = new Vector2(baseLeftX,  targetY);
        rrt.anchoredPosition = new Vector2(baseRightX, targetY);
    }

    void ClearPlanted()
    {
        if (plantedLeft)  Destroy(plantedLeft);
        if (plantedRight) Destroy(plantedRight);
        plantedLeft = plantedRight = null;
    }

    void UpdateStatus(bool isWalking)
    {
        if (statusText)
            statusText.text = "상태: " + (isWalking ? "<b>걷는 중</b>" : "<b>대기</b>");
    }
}
