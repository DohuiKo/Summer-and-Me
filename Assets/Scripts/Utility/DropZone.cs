using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    /// <summary>웨이브 전환용: 드롭 직후(부모 변경 전) 알림</summary>
    public event Action<DraggableItem> onDropped;

    [Header("Inventory (optional)")]
    public InventoryManager inventory;   // 비워두면 카운트 안 함

    [Header("Special Notice (optional)")]
    [Tooltip("드롭하면 안내 패널을 재생할 '특정 아이템' (예: TapePiece)")]
    public DraggableItem specialItem;
    [Tooltip("히든 패널(캔버스 그룹 필수). 처음엔 비활성 권장")]
    public CanvasGroup hiddenPanel;
    [Tooltip("히든 패널 종료 뒤 켤 언락 버튼(다음 페이지 버튼 등)")]
    public GameObject unlockButton;
    public bool showOnce = true;

    [Header("Hidden Panel Timings")]
    public float fadeInTime  = 0.35f;
    public float holdTime    = 3.0f;
    public float fadeOutTime = 0.35f;

    bool infoShown = false;
    Coroutine panelCo;

    void Awake()
    {
        // 초기 상태 정리(있을 때만)
        if (hiddenPanel)
        {
            hiddenPanel.alpha = 0f;
            hiddenPanel.interactable = false;
            hiddenPanel.blocksRaycasts = false;
            hiddenPanel.gameObject.SetActive(false);
        }
        if (unlockButton) unlockButton.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var item = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (item == null) return;

        Debug.Log($"{item.name} 아이템을 가방에 챙겼습니다.");

        // 1) 웨이브 매니저에게 '먼저' 알림 (아직 원래 부모일 때)
        onDropped?.Invoke(item);

        // 2) 인벤토리 카운트(선택)
        if (inventory != null || InventoryManager.instance != null)
        {
            (inventory ?? InventoryManager.instance).ItemCollected();
        }

        // 3) 수납 처리
        item.transform.SetParent(transform, false);
        item.gameObject.SetActive(false);

        // 4) 특정 아이템이면 히든 패널 자동 재생
        if (specialItem && item == specialItem && hiddenPanel && (!showOnce || !infoShown))
        {
            infoShown = true;
            if (panelCo != null) StopCoroutine(panelCo);
            panelCo = StartCoroutine(PlayHiddenPanel());
        }
    }

    IEnumerator PlayHiddenPanel()
    {
        // 활성화 & 준비
        hiddenPanel.gameObject.SetActive(true);
        hiddenPanel.interactable = false;
        hiddenPanel.blocksRaycasts = false;
        hiddenPanel.alpha = 0f;

        // Fade In
        float t = 0f;
        float fin = Mathf.Max(0.01f, fadeInTime);
        while (t < fin)
        {
            t += Time.unscaledDeltaTime;
            hiddenPanel.alpha = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t / fin));
            yield return null;
        }
        hiddenPanel.alpha = 1f;

        // Hold
        float h = 0f;
        while (h < holdTime)
        {
            h += Time.unscaledDeltaTime;
            yield return null;
        }

        // Fade Out
        t = 0f;
        float fout = Mathf.Max(0.01f, fadeOutTime);
        while (t < fout)
        {
            t += Time.unscaledDeltaTime;
            hiddenPanel.alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(t / fout));
            yield return null;
        }
        hiddenPanel.alpha = 0f;
        hiddenPanel.gameObject.SetActive(false);

        // Unlock 버튼 켜주기
        if (unlockButton) unlockButton.SetActive(true);

        panelCo = null;
    }

    // 필요 시: 수동으로 패널 닫기 원하면 UI 버튼에서 호출
    public void CloseHiddenPanel()
    {
        if (panelCo != null) StopCoroutine(panelCo);
        if (hiddenPanel)
        {
            hiddenPanel.alpha = 0f;
            hiddenPanel.gameObject.SetActive(false);
        }
        if (unlockButton) unlockButton.SetActive(true);
        panelCo = null;
    }
}
