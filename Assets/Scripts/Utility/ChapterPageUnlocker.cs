using UnityEngine;
using System.Collections;

public class ChapterPageUnlocker : MonoBehaviour
{
    [Header("필요한 참조 (비워두면 자동 기본모드)")]
    public RectTransform targetPage;     // 감지할 페이지 (예: Chapter3Page)
    public RectTransform viewport;       // ScrollView의 Viewport
    public GameObject unlockButton;      // 예: NextDownArrow

    [Header("설정값")]
    public float centerTolerance = 100f; // 중앙 판정 허용 거리(px)
    public float gazeDuration = 3f;      // 중앙 유지 시간 (초)

    private Coroutine gazeCoroutine;
    private bool isCounting = false;
    private bool isUnlocked = false;
    private bool isSimpleMode = false;   // ✅ 자동 모드 플래그

    void Start()
    {
        if (unlockButton == null)
        {
            Debug.LogError("❌ Unlock Button이 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        // ✅ targetPage나 viewport가 비어있으면 ‘첫 페이지 모드’로 전환
        if (targetPage == null || viewport == null)
        {
            isSimpleMode = true;
            Debug.Log("🔹 타겟/뷰포트 미지정 → 기본모드로 동작 (씬 시작 후 타이머 즉시 시작)");
            StartCoroutine(GazeTimerAndUnlock());
            return;
        }

        unlockButton.SetActive(false);
    }

    void Update()
    {
        // 기본 모드에서는 Update 감지 불필요
        if (isSimpleMode || isUnlocked) return;

        // 현재 페이지와 Viewport 중심 차이 계산
        Vector3 viewCenter = viewport.TransformPoint(viewport.rect.center);
        Vector3 pageCenter = targetPage.TransformPoint(targetPage.rect.center);

        float distance = Mathf.Abs(viewCenter.y - pageCenter.y);

        // 중앙에 들어왔을 때
        if (distance < centerTolerance && !isCounting)
        {
            Debug.Log("📍 챕터 페이지 중앙 감지 → 타이머 시작");
            gazeCoroutine = StartCoroutine(GazeTimerAndUnlock());
            isCounting = true;
        }
        // 중앙에서 벗어났을 때
        else if (distance >= centerTolerance && isCounting)
        {
            Debug.Log("❌ 중앙 벗어남 → 타이머 중단");
            StopCoroutine(gazeCoroutine);
            isCounting = false;
        }
    }

    IEnumerator GazeTimerAndUnlock()
    {
        unlockButton.SetActive(false);
        yield return new WaitForSeconds(gazeDuration);

        unlockButton.SetActive(true);
        isUnlocked = true;
        Debug.Log("✅ 버튼 활성화 완료 (중앙 유지 or 기본모드)");
    }
}
