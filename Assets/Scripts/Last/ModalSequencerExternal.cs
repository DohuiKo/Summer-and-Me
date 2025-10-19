// ModalSequencerExternal.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// RoomMainPage 중앙 도달 → delayAfterCenter 대기 → 모달 표시
/// → lockHoldSeconds 지난 뒤 BGM 시작 + Next 화살표 활성화
/// → 화살표 클릭 시 모달 닫고 BGM 정지, CLM 해제
public class ModalSequencerExternal : MonoBehaviour
{
    [Header("Refs")]
    public CenterLockObserver centerObserver; // Target=RoomMainPage 권장
    public ContentLockManager clm;            // 기존 CLM(수정 금지)
    public ModalView modal;                   // 단일 모달
    public Button nextDownArrow;              // 마지막에만 ON
    public AudioSource bgm;                   // BGM (선택, PlayOnAwake 꺼짐)

    [Header("Assets")]
    public Sprite page;                       // 모달에 보여줄 확대 컷(선택)

    [Header("Timings (Realtime)")]
    [Tooltip("중앙 락 후 모달 표시까지 지연(초)")]
    public float delayAfterCenter = 2f;
    [Tooltip("모달 표시 후 유지 시간(초). 이 시간이 지난 뒤 BGM 시작 + Next 활성화")]
    public float lockHoldSeconds = 3f;

    private bool started;

    void OnEnable()
    {
        if (centerObserver) centerObserver.onCenteredOnce.AddListener(BeginSequence);
        if (nextDownArrow) nextDownArrow.onClick.AddListener(OnNext);
        if (nextDownArrow) nextDownArrow.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (centerObserver) centerObserver.onCenteredOnce.RemoveListener(BeginSequence);
        if (nextDownArrow) nextDownArrow.onClick.RemoveListener(OnNext);
    }

    void BeginSequence()
    {
        if (started) return;
        started = true;
        StartCoroutine(Sequence());
    }

    IEnumerator Sequence()
    {
        // 1) 중앙 도달 후 대기 → 모달 표시
        yield return new WaitForSecondsRealtime(delayAfterCenter);
        if (modal) modal.Show(page);

        // 2) 모달 표시 후 lockHoldSeconds 동안 락 유지 (BGM은 아직 X)
        yield return new WaitForSecondsRealtime(lockHoldSeconds);

        // 3) 이제 BGM 시작
        if (bgm && !bgm.isPlaying) bgm.Play();

        // 4) 다음 페이지로 넘어가도록 화살표 활성화
        if (nextDownArrow) nextDownArrow.gameObject.SetActive(true);
    }

    void OnNext()
    {
        if (modal) modal.Hide();
        if (bgm && bgm.isPlaying) bgm.Stop();
        if (clm) clm.UnlockContent();                 // 스크롤락 해제
        if (nextDownArrow) nextDownArrow.gameObject.SetActive(false);
    }
}
