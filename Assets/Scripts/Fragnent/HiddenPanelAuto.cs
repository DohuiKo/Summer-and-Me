// HiddenPanelAuto.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class HiddenPanelAuto : MonoBehaviour
{
    public float fadeInTime = 0.35f;
    public float holdTime   = 3.0f;
    public float fadeOutTime= 0.35f;

    [Tooltip("패널 표시가 끝난 뒤 켤 언락 버튼(선택)")]
    public GameObject unlockButton;

    CanvasGroup cg;
    Coroutine playing;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        // 처음엔 숨김 상태 권장
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
        gameObject.SetActive(false);
        if (unlockButton) unlockButton.SetActive(false);
    }

    public void Play()
    {
        if (playing != null) StopCoroutine(playing);
        playing = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        gameObject.SetActive(true);
        cg.blocksRaycasts = false;
        cg.interactable = false;
        cg.alpha = 0f;

        // 🎵 히든패널 등장 시 테이프 획득 사운드 재생
        if (Chap3SoundManager.Instance != null)
        {
            Chap3SoundManager.Instance.PlayGetTapePiece();
            Debug.Log("[HiddenPanelAuto] 히든패널 등장 → 테이프 획득 사운드 재생");
        }

        // Fade In
        float t = 0f;
        while (t < fadeInTime)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t / Mathf.Max(0.01f, fadeInTime)));
            yield return null;
        }
        cg.alpha = 1f;

        // Hold
        float h = 0f;
        while (h < holdTime)
        {
            h += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade Out
        t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / Mathf.Max(0.01f, fadeOutTime)));
            yield return null;
        }
        cg.alpha = 0f;
        gameObject.SetActive(false);

        // Unlock 버튼 활성화
        if (unlockButton) unlockButton.SetActive(true);

        playing = null;
    }
}
