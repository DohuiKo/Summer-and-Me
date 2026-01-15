using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Page15Footsteps : MonoBehaviour
{
    [Header("Scroll & Page")]
    public ScrollRect scrollRect;          
    public RectTransform pageRoot;         
    public RectTransform lane;             

    [Header("Templates")]
    public Image leftTemplate;
    public Image rightTemplate;

    [Header("Activation")]
    public float activationThreshold = 50f;

    [Header("Step Settings")]
    public float verticalGap = 350f;
    [Tooltip("목표 걸음 수")]
    public int targetSteps = 10; 
    
    public Button nextButton;
    [Tooltip("버튼 페이드인 시간")]
    public float buttonFadeDuration = 1.0f; 

    [Header("Fading Footprints")]
    public float markFadeDuration = 0.8f;
    [Range(0f, 1f)] public float markStartAlpha = 0.75f;
    public float markScale = 1f;

    [Header("FX")]
    public AudioSource stepSfx;
    public CanvasGroup vignette;
    public Text statusText;

    // 내부 변수
    GameObject plantedLeft, plantedRight;
    bool leftTurn = true;
    int stepCount = 0;
    float baseLeftX, baseLeftY, baseRightX, baseRightY;
    
    bool isKeyPressed = false;
    bool isFinished = false; 

    void Awake()
    {
        if (!lane) lane = pageRoot;
        
        // --- 버튼 초기화 ---
        if (nextButton) 
        {
            nextButton.gameObject.SetActive(false);
            
            // CanvasGroup 세팅
            var cg = nextButton.GetComponent<CanvasGroup>();
            if (cg == null) cg = nextButton.gameObject.AddComponent<CanvasGroup>();
            
            cg.alpha = 0f; 
            cg.interactable = false;     // 클릭 금지
            cg.blocksRaycasts = false;   // 마우스 감지 금지
        }

        DisableNavigation(nextButton);
        if (scrollRect) DisableNavigation(scrollRect);

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
        if (scrollRect) scrollRect.enabled = false;
        InitStandingPose();
        UpdateStatus(false);
        if (vignette) vignette.alpha = 0f;
    }

    void OnDisable()
    {
        if (scrollRect) scrollRect.enabled = true;
    }

    void Update()
    {
        if (stepCount == 0 && !IsPageCentered()) return;

        float input = Input.GetAxisRaw("Vertical");
        bool keyPressedNow = input > 0.5f;
        bool mousePressedNow = Input.GetMouseButtonDown(0);

        if (mousePressedNow || (keyPressedNow && !isKeyPressed))
        {
            StepOnce();
        }

        isKeyPressed = keyPressedNow;
    }

    void StepOnce()
    {
        if (!plantedLeft || !plantedRight) InitStandingPose();
        if (isFinished) return;

        var lrt = plantedLeft.GetComponent<RectTransform>();
        var rrt = plantedRight.GetComponent<RectTransform>();

        if (leftTurn)
        {
            SpawnMark(leftTemplate, lrt.anchoredPosition);
            lrt.anchoredPosition = new Vector2(baseLeftX, lrt.anchoredPosition.y + verticalGap);
        }
        else
        {
            SpawnMark(rightTemplate, rrt.anchoredPosition);
            rrt.anchoredPosition = new Vector2(baseRightX, rrt.anchoredPosition.y + verticalGap);
        }

        if (stepSfx && stepSfx.clip) stepSfx.PlayOneShot(stepSfx.clip);
        if (vignette) StartCoroutine(PulseVignette(0.35f, 0.12f));

        UpdateStatus(true);
        stepCount++; 
        leftTurn = !leftTurn;

        Debug.Log($"현재 걸음: {stepCount} / 목표: {targetSteps}");

        // 🔥 [목표 달성]
        if (targetSteps > 0 && stepCount >= targetSteps)
        {
            isFinished = true;
            AlignFeet();
            UpdateStatus(false);

            if (nextButton) 
            {
                StartCoroutine(FadeInButton());
            }
        }
    }

    // 🔥 핵심 수정: 버튼이 완전히 켜지도록 설정 추가
    IEnumerator FadeInButton()
    {
        nextButton.gameObject.SetActive(true);

        CanvasGroup cg = nextButton.GetComponent<CanvasGroup>();
        if (!cg) cg = nextButton.gameObject.AddComponent<CanvasGroup>();
        
        cg.alpha = 0f; 

        float t = 0f;
        while (t < buttonFadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / buttonFadeDuration);
            yield return null;
        }

        // --- 여기가 수정되었습니다 ---
        cg.alpha = 1f;
        cg.interactable = true;      // 이제 클릭 허용!
        cg.blocksRaycasts = true;    // 마우스 광선 막기(클릭 감지) 허용!
        
        nextButton.interactable = true;
        Debug.Log("🎉 버튼 완전 활성화 완료!");
    }

    // --- 유틸리티 ---
    bool IsPageCentered()
    {
        if (!scrollRect || !pageRoot) return false;
        Vector3 viewportCenter = GetWorldCenter(scrollRect.viewport);
        Vector3 pageCenter = GetWorldCenter(pageRoot);
        return Mathf.Abs(viewportCenter.y - pageCenter.y) <= activationThreshold;
    }

    Vector3 GetWorldCenter(RectTransform rt)
    {
        if (rt == null) return Vector3.zero;
        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);
        return (corners[0] + corners[2]) / 2f;
    }

    void InitStandingPose()
    {
        ClearPlanted();
        float startY = Mathf.Min(baseLeftY, baseRightY);

        plantedLeft = Instantiate(leftTemplate, lane).gameObject;
        plantedRight = Instantiate(rightTemplate, lane).gameObject;
        plantedLeft.SetActive(true);
        plantedRight.SetActive(true);

        var lrt = plantedLeft.GetComponent<RectTransform>();
        var rrt = plantedRight.GetComponent<RectTransform>();
        lrt.anchoredPosition = new Vector2(baseLeftX, startY);
        rrt.anchoredPosition = new Vector2(baseRightX, startY);

        leftTurn = true;
        stepCount = 0;
        isFinished = false; 
        
        if (nextButton) nextButton.gameObject.SetActive(false);
    }

    void DisableNavigation(Selectable uiElement)
    {
        if (uiElement == null) return;
        Navigation nav = new Navigation();
        nav.mode = Navigation.Mode.None;
        uiElement.navigation = nav;
    }
    
    void DisableNavigation(ScrollRect scroll)
    {
        if (scroll) DisableNavigation(scroll.GetComponent<Selectable>());
    }

    void SpawnMark(Image template, Vector2 pos)
    {
        if (!template) return;
        var mark = Instantiate(template, lane);
        mark.gameObject.SetActive(true);
        mark.transform.SetAsFirstSibling();
        
        var rt = mark.rectTransform;
        rt.anchoredPosition = pos;
        rt.localScale = Vector3.one * markScale;
        
        var cg = mark.GetComponent<CanvasGroup>();
        if (!cg) cg = mark.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = markStartAlpha;
        
        StartCoroutine(FadeOut(mark.gameObject, cg, markFadeDuration));
    }

    IEnumerator FadeOut(GameObject go, CanvasGroup cg, float dur)
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

    IEnumerator PulseVignette(float target, float dur)
    {
        if (!vignette) yield break;
        vignette.alpha = target;
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            vignette.alpha = Mathf.Lerp(target, 0f, t / dur);
            yield return null;
        }
        vignette.alpha = 0f;
    }

    void AlignFeet()
    {
        if (!plantedLeft || !plantedRight) return;
        var lrt = plantedLeft.GetComponent<RectTransform>();
        var rrt = plantedRight.GetComponent<RectTransform>();
        float finalY = Mathf.Max(lrt.anchoredPosition.y, rrt.anchoredPosition.y);
        lrt.anchoredPosition = new Vector2(baseLeftX, finalY);
        rrt.anchoredPosition = new Vector2(baseRightX, finalY);
    }

    void ClearPlanted()
    {
        if (plantedLeft) Destroy(plantedLeft);
        if (plantedRight) Destroy(plantedRight);
    }

    void UpdateStatus(bool walking)
    {
        if (statusText) statusText.text = walking ? "걷는 중..." : "대기";
    }
}
