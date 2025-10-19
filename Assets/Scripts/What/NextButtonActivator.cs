// NextButtonActivator.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class NextButtonActivator : MonoBehaviour
{
    [Header("References")]
    public MimiModal modal;              // MimiModal 오브젝트 드래그
    public Button nextButton;            // 보통 자기 자신 버튼을 드래그 (비워두면 자동 획득)

    [Header("Behavior")]
    [Min(1)] public int playsRequired = 2;   // 몇 번 재생 끝나면 버튼 표시할지
    [Tooltip("버튼이 나타날 때 페이드 시간(초). 0이면 즉시 표시")]
    public float showFadeTime = 0f;

    // 내부 상태
    private VideoPlayer vp;
    private int playCount = 0;
    private bool seenPlaying = false;    // 현재 사이클에서 재생이 시작되었는가
    private bool awaitingStart = true;   // 첫 재생 대기 중인가
    private CanvasGroup cg;

    void Awake()
    {
        if (!nextButton) nextButton = GetComponent<Button>();

        if (!nextButton)
        {
            Debug.LogError("[NextButtonActivator] Button 컴포넌트를 찾지 못했습니다. 버튼 오브젝트에 붙이거나 참조를 지정하세요.");
            enabled = false;
            return;
        }

        // 버튼은 활성(Active) 유지 — 스크립트가 돌아야 하므로
        nextButton.gameObject.SetActive(true);

        // 시작 시 클릭 불가 + 보이지 않게
        nextButton.interactable = false;
        cg = nextButton.GetComponent<CanvasGroup>();
        if (!cg) cg = nextButton.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;   // 숨김 중에는 클릭 차단
        cg.interactable = false;
    }

    void Start()
    {
        if (!modal)
        {
            Debug.LogError("[NextButtonActivator] MimiModal 참조가 비어 있습니다.");
            enabled = false;
            return;
        }
        vp = modal.videoPlayer;
        if (!vp)
        {
            Debug.LogError("[NextButtonActivator] MimiModal.videoPlayer가 연결되지 않았습니다.");
            enabled = false;
            return;
        }

        // 이벤트에 의존하지 않고 직접 상태 감시 (환경 차이 안전)
        StartCoroutine(Co_WatchVideoPlays());
    }

    System.Collections.IEnumerator Co_WatchVideoPlays()
    {
        // 1) 첫 재생 시작까지 대기 (시퀀스가 끝나고 재생될 때)
        while (awaitingStart)
        {
            if (vp.isPlaying)
            {
                awaitingStart = false;
                seenPlaying = true;
            }
            yield return null;
        }

        // 2) 종료 → 카운트 → (필요시) 재생 재개를 직접 관리
        while (playCount < playsRequired)
        {
            // 재생 중이었다가 멈췄고, 실제로 조금이라도 진행된 상태면 "한 번 끝난 것"으로 간주
            if (seenPlaying && !vp.isPlaying && (vp.time > 0.05f || vp.frame > 0))
            {
                playCount++;

                if (playCount >= playsRequired) break;

                // 다음 루프 강제 (VideoPlayer의 Loop 설정과 무관하게 확실히 2회 보장)
                vp.Play();
                seenPlaying = false;
            }

            // 다음 사이클에서 재생 시작 감지
            if (vp.isPlaying && !seenPlaying)
                seenPlaying = true;

            yield return null;
        }

        // 3) 조건 충족 → 버튼 표시 & 활성화
        ShowAndEnableButton();
    }

    void ShowAndEnableButton()
    {
        if (!nextButton) return;

        nextButton.interactable = true;
        if (cg)
        {
            cg.blocksRaycasts = true;
            cg.interactable = true;

            if (showFadeTime > 0f)
                StartCoroutine(Co_FadeCanvasGroup(cg, 0f, 1f, showFadeTime));
            else
                cg.alpha = 1f;
        }

        // Debug.Log("[NextButtonActivator] ✅ Next 버튼 활성화/표시 완료");
    }

    System.Collections.IEnumerator Co_FadeCanvasGroup(CanvasGroup g, float a, float b, float t)
    {
        if (!g || t <= 0f) { if (g) g.alpha = b; yield break; }
        float e = 0f;
        g.alpha = a;
        while (e < t)
        {
            e += Time.unscaledDeltaTime;
            g.alpha = Mathf.Lerp(a, b, e / t);
            yield return null;
        }
        g.alpha = b;
    }
}
