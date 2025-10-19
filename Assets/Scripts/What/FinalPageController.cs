using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FinalPageController : MonoBehaviour
{
    [Header("Scroll References")]
    public ScrollRect scrollRect;        // Canvas > Scroll View
    public RectTransform content;        // Scroll View > Viewport > Content
    public RectTransform finalPage;      // FinalPage(자기 자신) RectTransform

    [Header("Modal Target")]
    public MimiModal miMaiModal;         // ← 타입을 MimiModal로!

    [Header("Trigger Settings")]
    [Tooltip("FinalPage 중심이 뷰포트 중심에 얼마나 가까워야 하는지(0~1)")]
    public float centerThreshold = 0.5f;
    [Tooltip("중앙 도달 후 대기 시간(초)")]
    public float delayBeforeShowModal = 2f;
    [Tooltip("한 번만 실행할지 여부")]
    public bool triggerOnce = true;

    bool hasTriggered = false;
    Coroutine waiting;

    void Start()
    {
        if (miMaiModal != null)
            miMaiModal.gameObject.SetActive(false); // 시작 시 모달 숨김

        if (scrollRect != null)
            scrollRect.onValueChanged.AddListener(OnScrolled);

        // 씬 시작 위치가 이미 중앙일 수 있으니 첫 프레임에 강제 체크
        Invoke(nameof(InitialCheck), 0.1f);
    }

    void OnDestroy()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScrolled);
    }

    void InitialCheck() => OnScrolled(Vector2.zero);

    void OnScrolled(Vector2 _)
    {
        if (triggerOnce && hasTriggered) return;
        if (!scrollRect || !content || !finalPage) return;

        if (IsFinalPageCentered())
        {
            if (waiting == null)
                waiting = StartCoroutine(Co_FireAfterDelay());
        }
        else
        {
            // 조건 벗어나면 대기 취소
            if (waiting != null)
            {
                StopCoroutine(waiting);
                waiting = null;
            }
        }
    }

    IEnumerator Co_FireAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeShowModal);

        // 재확인 후 실행
        if (IsFinalPageCentered())
        {
            hasTriggered = true;
            if (miMaiModal != null)
            {
                miMaiModal.PlayAutoSequence();
            }
            if (triggerOnce)
            {
                // 더 이상 감시 불필요
                scrollRect.onValueChanged.RemoveListener(OnScrolled);
                enabled = false;
            }
        }
        waiting = null;
    }

    bool IsFinalPageCentered()
    {
        var vp = scrollRect.viewport;
        if (!vp) return false;

        // 뷰포트 중심(월드)과 FinalPage 중심(월드)
        Vector3 vpCenterW = vp.TransformPoint(vp.rect.center);
        Vector3 pageCenterW = finalPage.TransformPoint(finalPage.rect.center);

        // 뷰포트 로컬 기준 좌표로 변환 후 거리 정규화
        Vector2 vpCenter = (Vector2)vp.InverseTransformPoint(vpCenterW);
        Vector2 pgCenter = (Vector2)vp.InverseTransformPoint(pageCenterW);

        float dist = (pgCenter - vpCenter).magnitude;
        float norm = dist / (vp.rect.height * 0.5f); // 0=완전 중앙, 1≈화면 반 높이만큼 떨어짐

        return norm <= Mathf.Clamp01(centerThreshold);
    }
}
