using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinalPageSequence : MonoBehaviour
{
    [Header("1. 기본 참조")]
    public ScrollRect scrollRect;

    [Header("2. 배경 연출 참조")]
    public Image backgroundPanel;

    [Header("3. 신발 연출 참조")]
    public Image shoesImage;
    public Button shoesButton;
    // ✨ 변경점: 목적지(shoeEndPoint)는 더 이상 필요 없으므로 삭제했습니다.

    [Header("4. 최종 결과 참조")]
    public GameObject nextDownArrow;

    [Header("애니메이션 속도 설정")]
    public float backgroundFadeDuration = 1.5f;
    public float shoeFadeOutDuration = 1.0f;
    [Header("✨ 신발 발구르기 애니메이션")]
    public int numberOfSteps = 3;       // 몇 번 발을 구를지
    public float stepHeight = 30f;      // 얼마나 높이 들지
    public float stepDuration = 0.2f;   // 각 스텝(들고 내리기)에 걸리는 시간

    // --- 내부 상태 관리 ---
    private RectTransform pageTransform;
    private enum SequenceState { WaitingForCenter, FadingBackground, WaitingForShoeClick, AnimatingShoe, Complete }
    private SequenceState currentState = SequenceState.WaitingForCenter;

    void Start()
    {
        pageTransform = GetComponent<RectTransform>();

        if (backgroundPanel != null) backgroundPanel.gameObject.SetActive(false);
        if (shoesImage != null) shoesImage.gameObject.SetActive(false);
        if (shoesButton != null) shoesButton.onClick.AddListener(OnShoeClicked);
        if (nextDownArrow != null) nextDownArrow.SetActive(false);
    }

    void Update()
    {
        if (currentState == SequenceState.WaitingForCenter && IsCentered())
        {
            currentState = SequenceState.FadingBackground;
            StartCoroutine(BackgroundSequence());
        }
    }

    private IEnumerator BackgroundSequence()
    {
        Debug.Log("시퀀스 1: 스크롤 잠금 및 배경 페이드 시작");
        if (scrollRect != null) scrollRect.enabled = false;

        backgroundPanel.color = Color.black;
        backgroundPanel.gameObject.SetActive(true);

        float timer = 0f;
        while (timer < backgroundFadeDuration)
        {
            timer += Time.deltaTime;
            backgroundPanel.color = Color.Lerp(Color.black, Color.gray, timer / backgroundFadeDuration);
            yield return null;
        }
        backgroundPanel.color = Color.gray;

        Debug.Log("시퀀스 2: 신발 등장을 위해 마우스 클릭 대기 중...");
        yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        
        Debug.Log("클릭 감지! 신발을 등장시킵니다.");
        if (shoesImage != null)
        {
            shoesImage.gameObject.SetActive(true);
        }
        currentState = SequenceState.WaitingForShoeClick;
    }

    private void OnShoeClicked()
    {
        if (currentState == SequenceState.WaitingForShoeClick)
        {
            Debug.Log("시퀀스 3: 신발 클릭됨! 발구르기 애니메이션 시작");
            currentState = SequenceState.AnimatingShoe;
            shoesButton.interactable = false; 
            StartCoroutine(ShoeAnimationSequence());
        }
    }

    // ✨ 변경점: 신발 이동 로직을 제자리에서 발 구르는 애니메이션으로 완전히 교체
    private IEnumerator ShoeAnimationSequence()
    {
        Vector3 originalPos = shoesImage.transform.position;
        Vector3 upPos = originalPos + new Vector3(0, stepHeight, 0);

        // 정해진 횟수만큼 발구르기 반복
        for (int i = 0; i < numberOfSteps; i++)
        {
            // 발 올리기
            float timer = 0f;
            while (timer < stepDuration)
            {
                timer += Time.deltaTime;
                shoesImage.transform.position = Vector3.Lerp(originalPos, upPos, timer / stepDuration);
                yield return null;
            }

            // 발 내리기
            timer = 0f;
            while (timer < stepDuration)
            {
                timer += Time.deltaTime;
                shoesImage.transform.position = Vector3.Lerp(upPos, originalPos, timer / stepDuration);
                yield return null;
            }
        }
        
        // 신발 페이드아웃
        float fadeTimer = 0f;
        Color startColor = shoesImage.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0);

        while (fadeTimer < shoeFadeOutDuration)
        {
            fadeTimer += Time.deltaTime;
            shoesImage.color = Color.Lerp(startColor, endColor, fadeTimer / shoeFadeOutDuration);
            yield return null;
        }
        shoesImage.gameObject.SetActive(false);

        // 최종 화살표 활성화
        if (nextDownArrow != null)
        {
            Debug.Log("시퀀스 4: 모든 연출 완료. NextDownArrow 활성화");
            nextDownArrow.SetActive(true);
        }
        currentState = SequenceState.Complete;
    }
    
    private bool IsCentered()
    {
        if (scrollRect == null || pageTransform == null) return false;
        Vector3 viewportCenter = scrollRect.viewport.position;
        Vector3 pageCenter = pageTransform.position;
        float distance = Mathf.Abs(pageCenter.y - viewportCenter.y);
        return distance < 20f;
    }
}