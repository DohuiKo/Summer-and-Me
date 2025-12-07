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

    Coroutine gazeCoroutine;
    bool isCounting = false;
    bool isUnlocked = false;
    bool isSimpleMode = false;   // 기본 모드 플래그

    void Start()
    {
        if (unlockButton == null)
        {
            Debug.LogError("❌ Unlock Button이 할당되지 않았습니다.");
            enabled = false;
            return;
        }

        // 시작할 때는 항상 버튼 숨김
        unlockButton.SetActive(false);

        // targetPage나 viewport가 없으면 기본 모드
        if (targetPage == null || viewport == null)
        {
            isSimpleMode = true;
            Debug.Log("🔹 타겟/뷰포트 미지정 → 기본모드로 동작 (씬 시작 후 타이머 즉시 시작)");
            gazeCoroutine = StartCoroutine(GazeTimerAndUnlock());
        }
    }

    void Update()
    {
        if (isSimpleMode || isUnlocked) return;

        if (targetPage == null || viewport == null) return;

        Vector3 viewCenter = viewport.TransformPoint(viewport.rect.center);
        Vector3 pageCenter = targetPage.TransformPoint(targetPage.rect.center);

        float distance = Mathf.Abs(viewCenter.y - pageCenter.y);

        if (distance < centerTolerance && !isCounting)
        {
            gazeCoroutine = StartCoroutine(GazeTimerAndUnlock());
            isCounting = true;
        }
        else if (distance >= centerTolerance && isCounting)
        {
            if (gazeCoroutine != null)
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
        isCounting = false;

        Debug.Log("✅ 버튼 활성화 완료 (중앙 유지 or 기본모드)");
    }
}
