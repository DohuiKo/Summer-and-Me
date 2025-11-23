using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SittingPageLockInitializer : MonoBehaviour
{
    [Header("Required")]
    public ScrollRect scrollRect;      // Canvas/Scroll View
    public RectTransform targetPage;   // SittingPage RectTransform

    [Header("Unlock Trigger")]
    public Button unlockButton;        // NextDownArrow (눌러야 다음으로 넘어가는 버튼)

    // 내부 저장 상태
    private bool wasVertical;
    private bool wasHorizontal;
    private bool wasInertia;
    private bool wasEnabled;

    void Start()
    {
        StartCoroutine(Co_ApplyInitialLock());
    }

    private IEnumerator Co_ApplyInitialLock()
    {
        // 1) Layout / ScrollRect 초기화 대기 (필수)
        yield return null;  
        yield return null;  

        // 2) 스크롤 위치 고정하기 (여기서는 그대로 중앙 유지)
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);
        }

        // 3) 현재 ScrollRect 상태 저장
        SaveScrollSettings();

        // 4) 스크롤 완전 잠그기
        LockScroll();

        // 5) 버튼 누르면 풀리도록 이벤트 연결
        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveListener(UnlockScroll);
            unlockButton.onClick.AddListener(UnlockScroll);
        }
    }

    // ScrollRect 상태 백업
    private void SaveScrollSettings()
    {
        wasEnabled    = scrollRect.enabled;
        wasVertical   = scrollRect.vertical;
        wasHorizontal = scrollRect.horizontal;
        wasInertia    = scrollRect.inertia;
    }

    // 스크롤 잠금
    private void LockScroll()
    {
        if (!scrollRect) return;

        scrollRect.enabled = false;
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        scrollRect.inertia = false;
    }

    // 버튼 클릭 시 스크롤 해제
    private void UnlockScroll()
    {
        if (!scrollRect) return;

        scrollRect.enabled = wasEnabled;
        scrollRect.vertical = wasVertical;
        scrollRect.horizontal = wasHorizontal;
        scrollRect.inertia = wasInertia;
    }
}
