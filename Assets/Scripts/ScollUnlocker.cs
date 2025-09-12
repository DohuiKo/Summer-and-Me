using UnityEngine;
using UnityEngine.UI;

/// 처음엔 ScrollRect를 꺼 두고, Unlock() 호출 시 세로 스크롤을 켭니다.
public class ScollUnloker : MonoBehaviour
{
    [Header("Target")]
    public ScrollRect scrollRect;            // 대상 ScrollRect (Scroll View)

    [Header("Unlock Options")]
    public bool allowHorizontal = false;     // 해제 후 가로 스크롤 허용 여부
    public bool enableInertia = true;        // 해제 시 관성 사용
    public bool resetToTopOnUnlock = true;   // 해제 시 맨 위로 이동

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        Lock(); // 시작 시 잠금
    }

    // 버튼 OnClick에 연결: 스크롤 해제
    public void Unlock()
    {
        if (!scrollRect) return;

        scrollRect.enabled   = true;     // 컴포넌트 켜기
        scrollRect.vertical  = true;     // 세로 스크롤 허용
        scrollRect.horizontal= allowHorizontal;
        scrollRect.inertia   = enableInertia;

        // 잔류 속도 제거 + 시작 위치 보정
        scrollRect.velocity = Vector2.zero;
        if (resetToTopOnUnlock)
            scrollRect.normalizedPosition = new Vector2(0f, 1f);
    }

    // 필요하면 다시 잠그기
    public void Lock()
    {
        if (!scrollRect) return;

        scrollRect.enabled   = false;    // 컴포넌트 자체 비활성화 (완전 잠금)
        scrollRect.vertical  = false;
        scrollRect.horizontal= false;
        scrollRect.inertia   = false;
        scrollRect.velocity  = Vector2.zero;
    }
}
