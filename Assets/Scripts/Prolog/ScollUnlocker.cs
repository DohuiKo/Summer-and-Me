using UnityEngine;
using UnityEngine.UI;

/// 처음엔 ScrollRect를 꺼 두고, Unlock() 호출 시 세로 스크롤을 켭니다.
public class ScollUnloker : MonoBehaviour
{
    [Header("Target")]
    public ScrollRect scrollRect;           // 대상 ScrollRect (Scroll View)

    [Header("Unlock Options")]
    public bool allowHorizontal = false;    // 해제 후 가로 스크롤 허용 여부
    public bool enableInertia = true;       // 해제 시 관성 사용

    void Awake()
    {
        if (!scrollRect) scrollRect = GetComponent<ScrollRect>();
        Lock(); // 시작 시 잠금
    }

    // 버튼 OnClick에 연결: 스크롤 해제
    public void Unlock()
    {
        if (!scrollRect) return;

        // Unlock 하기 전에 현재 위치를 저장
        Vector2 currentPosition = scrollRect.normalizedPosition;

        scrollRect.enabled = true;      // 컴포넌트 켜기
        scrollRect.vertical = true;     // 세로 스크롤 허용
        scrollRect.horizontal = allowHorizontal;
        scrollRect.inertia = enableInertia;

        // 잔류 속도 제거
        scrollRect.velocity = Vector2.zero;

        // 스크롤 잠금 해제 후 원래 위치로 복원
        scrollRect.normalizedPosition = currentPosition;
    }

    // 필요하면 다시 잠그기
    public void Lock()
    {
        if (!scrollRect) return;

        scrollRect.enabled = false;     // 컴포넌트 자체 비활성화 (완전 잠금)
        scrollRect.vertical = false;
        scrollRect.horizontal = false;
        scrollRect.inertia = false;
        scrollRect.velocity = Vector2.zero;
    }
}