using UnityEngine;
using System.Collections;

public class ModalSequenceController : MonoBehaviour
{
    [Header("References")]
    public ModalView modalView;          // Modal의 ModalView
    public GameObject nextDownArrow;     // 켜줄 화살표/버튼
    public AudioSource bgm;              // ModalView가 쓰는 BGM(옵션)

    [Header("Timing")]
    public float waitSeconds = 3f;       // BGM(또는 Shown) 이후 대기

    [Header("Options")]
    public bool autoStartOnPlay = false; // 씬 시작 즉시 모달을 띄울지
    public bool requireBgmStart = true;  // BGM이 isPlaying 된 시점부터 타이머 시작

    bool started;

    void OnEnable()
    {
        if (modalView != null) modalView.Shown += OnModalShown;
    }
    void OnDisable()
    {
        if (modalView != null) modalView.Shown -= OnModalShown;
    }

    void Start()
    {
        if (nextDownArrow) nextDownArrow.SetActive(false);
        if (autoStartOnPlay) StartSequence(); // 필요 시 자동 실행
    }

    // 수동으로 원할 때 호출
    public void StartSequence()
    {
        if (started) return;
        started = true;

        if (modalView != null) modalView.Show();
        else Debug.LogError("[ModalSequenceController] modalView가 비었습니다.");
    }

    // 🔸 모달이 완전히 보이게 된 순간
    void OnModalShown()
    {
        StartCoroutine(WaitAndEnable());
    }

    IEnumerator WaitAndEnable()
    {
        // BGM 시작을 기준으로 3초 대기하고 싶다면 여기서 대기
        if (requireBgmStart && bgm != null)
        {
            float guard = 1.5f; // 최대 1.5초 대기
            while (!bgm.isPlaying && guard > 0f)
            {
                guard -= Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // 실시간 대기
        float t = 0f;
        while (t < waitSeconds)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (nextDownArrow == null)
        {
            Debug.LogError("[ModalSequenceController] nextDownArrow가 비었습니다.");
            yield break;
        }

        // 부모 비활성/정렬 문제를 잡기 위한 보강
        nextDownArrow.SetActive(true);
        var cg = nextDownArrow.GetComponentInParent<CanvasGroup>();
        if (cg && cg.alpha < 1f) cg.alpha = 1f;

        Debug.Log("[ModalSequenceController] nextDownArrow 활성화 완료");
    }
}
