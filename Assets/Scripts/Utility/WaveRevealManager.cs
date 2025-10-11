// WaveRevealManager.cs — 드롭 직후 '한 프레임 뒤'에 재계산해서 웨이브 전환
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveRevealManager : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public Transform root;
        public bool initiallyActive = true;
    }

    [Header("웨이브 세팅")]
    public List<Wave> waves = new List<Wave>();

    [Header("연출")]
    public float appearFade = 0.18f;
    public float appearScalePunch = 0.05f;

    [Header("연동(선택)")]
    public InventoryManager inventory;
    public DropZone dropZone;

    private int currentWave = 0;

    void Awake()
    {
        // 웨이브 초기 On/Off
        for (int i = 0; i < waves.Count; i++)
        {
            var w = waves[i];
            if (!w.root) continue;
            bool on = (i == 0) ? w.initiallyActive : false;
            w.root.gameObject.SetActive(on);
        }

        // 인벤토리 총 필요 개수(비활성 포함 전체 합) — 선택
        if (inventory)
        {
            int totalNeeded = 0;
            foreach (var w in waves)
                totalNeeded += CountDraggablesUnder(w.root, includeInactive: true);
            inventory.SetTotalItemsToCollect(totalNeeded);
        }

        // 드롭 알림 구독
        if (dropZone)
        {
            dropZone.onDropped -= OnItemStoredFromDropZone;
            dropZone.onDropped += OnItemStoredFromDropZone;
        }
    }

    void OnDestroy()
    {
        if (dropZone)
            dropZone.onDropped -= OnItemStoredFromDropZone;
    }

    // ⭐ 핵심 변경: 드롭 시 바로 판정하지 말고 '한 프레임 뒤'에 현재 웨이브가 비었는지 재계산
    public void OnItemStoredFromDropZone(DraggableItem _)
    {
        StartCoroutine(CoCheckAndRevealNextWave());
    }

    private IEnumerator CoCheckAndRevealNextWave()
    {
        // DropZone이 부모 변경/비활성화를 끝낼 때까지 한 프레임 대기
        yield return null;

        if (currentWave >= waves.Count) yield break;
        var root = waves[currentWave].root;
        if (!root) yield break;

        int remain = CountDraggablesUnder(root, includeInactive: false);
        // Debug.Log($"[WaveReveal] remain in wave{currentWave} = {remain}");

        if (remain == 0)
        {
            RevealNextWave();
        }
    }

    private void RevealNextWave()
    {
        currentWave++;
        if (currentWave >= waves.Count) return;

        var root = waves[currentWave].root;
        if (!root) return;

        root.gameObject.SetActive(true);
        StartCoroutine(AppearRoutine(root));
    }

    private IEnumerator AppearRoutine(Transform root)
    {
        var cgs = root.GetComponentsInChildren<CanvasGroup>(includeInactive: false);
        foreach (var cg in cgs) cg.alpha = 0f;

        float t = 0f;
        var baseScale = root.localScale;
        var startScale = baseScale * (1f + appearScalePunch);
        root.localScale = startScale;

        while (t < appearFade)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / Mathf.Max(0.01f, appearFade));
            foreach (var cg in cgs) cg.alpha = k;
            root.localScale = Vector3.Lerp(startScale, baseScale, k);
            yield return null;
        }

        foreach (var cg in cgs) cg.alpha = 1f;
        root.localScale = baseScale;
    }

    private int CountDraggablesUnder(Transform root, bool includeInactive)
    {
        if (!root) return 0;
        var items = root.GetComponentsInChildren<DraggableItem>(includeInactive);
        int cnt = 0;
        foreach (var it in items)
            if (it && it.gameObject.activeInHierarchy) cnt++;
        return cnt;
    }
}
