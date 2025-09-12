using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// UI 리본을 좌→우로 무한 루프.
/// 씬에는 '원본 이미지 1개'만 두면 되고, 런타임에 똑같은 타일을 (tileCount-1)개 더 만들어
/// 이어 붙여 부드럽게 흐르게 합니다. (기본 3장)
[RequireComponent(typeof(RectTransform), typeof(Image))]
public class NoteRibbonLooperNTiles : MonoBehaviour
{
    [Header("Scroll")]
    public float speed = 120f;           // px/sec (→)
    public bool runOnEnable = false;     // 켜질 때 자동 시작

    [Header("Layout")]
    [Min(2)] public int tileCount = 3;   // 동시에 이어 붙일 타일 개수(원본 포함)
    public float spacing = 0f;           // 타일 간격(px)

    [Header("Overshoot")]
    [Tooltip("오른쪽 경계 통과 후, (자기너비 * 값) 만큼 더 진행한 뒤에만 래핑")]
    public float overshootMultiplier = 1f;

    private RectTransform viewportRT;    // RectMask2D/Mask 가진 부모(경계)
    private List<RectTransform> tiles = new List<RectTransform>();
    private RectTransform rootRT;
    private Vector2 rootStartPos;
    private float tileWidth;
    private bool running;

    void Awake()
    {
        rootRT = GetComponent<RectTransform>();
        rootStartPos = rootRT.anchoredPosition;

        var img = GetComponent<Image>();
        if (img == null || img.sprite == null)
        {
            Debug.LogWarning("[LooperNTiles] Image/Sprite 필요");
            enabled = false;
            return;
        }

        viewportRT = FindNearestViewport(transform);

        // 타일 확보(원본 + 고스트들)
        EnsureTiles(tileCount);
        // 타일 가로폭 캐시
        tileWidth = rootRT.rect.width;

        // 초기 배치: 원본 기준으로 왼쪽에 연달아 붙여 놓음
        LayoutTiles();
    }

    void OnEnable()
    {
        if (runOnEnable) StartLoop();
    }

    void Update()
    {
        if (!running || tiles.Count == 0) return;

        float delta = speed * Time.deltaTime;
        // 전체 타일 이동
        for (int i = 0; i < tiles.Count; i++)
            tiles[i].anchoredPosition += Vector2.right * delta;

        // 각 타일 개별 래핑
        float rightLimit = GetViewportRightWithOvershoot();
        // 왼쪽 기준점을 잡기 위해 현재 '가장 왼쪽' 타일 찾기
        int leftmostIdx = GetLeftmostIndex();
        RectTransform leftmost = tiles[leftmostIdx];

        for (int i = 0; i < tiles.Count; i++)
        {
            var t = tiles[i];
            float tRight = t.anchoredPosition.x + (1f - t.pivot.x) * t.rect.width;
            if (tRight > rightLimit)
            {
                // 이 타일을 가장 왼쪽 타일의 '왼쪽'으로 재배치
                float newX = leftmost.anchoredPosition.x - (t.rect.width + spacing);
                var p = t.anchoredPosition; p.x = newX; t.anchoredPosition = p;

                // 방금 재배치된 타일이 새 '가장 왼쪽'이 됨
                leftmost = t;
                leftmostIdx = i;
            }
        }
    }

    // ───────── Public API ─────────
    public void StartLoop() => running = true;
    public void StopLoop()  => running = false;

    /// 시작 위치(씬에서 놓은 위치)로 재배치하고 멈춤
    public void ResetLoop()
    {
        running = false;
        // 원본을 원점으로, 나머지는 원본을 기준으로 재배치
        rootRT.anchoredPosition = rootStartPos;
        LayoutTiles();
    }

    // ───────── 내부 헬퍼 ─────────
    RectTransform FindNearestViewport(Transform t)
    {
        Transform p = t.parent;
        while (p != null)
        {
            if (p.GetComponent<RectMask2D>() != null) return p as RectTransform;
            var mask = p.GetComponent<Mask>();
            if (mask != null && mask.showMaskGraphic == false) return p as RectTransform;
            p = p.parent;
        }
        return t.parent as RectTransform; // 차선
    }

    void EnsureTiles(int count)
    {
        tiles.Clear();
        tiles.Add(rootRT);

        // 이미 존재하던 고스트 정리(이 스크립트를 재활용할 때 대비)
        for (int i = rootRT.parent.childCount - 1; i >= 0; i--)
        {
            var tr = rootRT.parent.GetChild(i);
            if (tr != rootRT && tr.name.StartsWith(rootRT.name + "_ghost"))
                Destroy(tr.gameObject);
        }

        var srcImg = GetComponent<Image>();
        for (int i = 1; i < count; i++)
        {
            var go = new GameObject(rootRT.name + "_ghost" + i, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(rootRT.parent, false);
            var rt = go.GetComponent<RectTransform>();
            var img = go.GetComponent<Image>();

            // 원본 이미지 속성 복제
            img.sprite = srcImg.sprite;
            img.type = srcImg.type;
            img.preserveAspect = srcImg.preserveAspect;
            img.color = srcImg.color;
            img.material = srcImg.material;
            img.raycastTarget = false; // 클릭 방해 방지

            // RectTransform 속성 복제
            rt.sizeDelta  = rootRT.sizeDelta;
            rt.pivot      = rootRT.pivot;
            rt.anchorMin  = rootRT.anchorMin;
            rt.anchorMax  = rootRT.anchorMax;
            rt.localScale = rootRT.localScale;
            rt.rotation   = rootRT.rotation;

            // 계층 순서는 원본과 나란히
            rt.SetSiblingIndex(rootRT.GetSiblingIndex());

            tiles.Add(rt);
        }
    }

    void LayoutTiles()
    {
        if (tiles.Count == 0) return;

        // 원본을 기준으로, 나머지 타일들을 왼쪽으로 1칸씩 붙여 둔다
        var basePos = rootRT.anchoredPosition;
        float w = rootRT.rect.width;

        // 왼쪽으로 계속 이어붙임: … [T2][T1][Root]  → 오른쪽으로 흘러들어오게
        for (int i = 1; i < tiles.Count; i++)
        {
            var rt = tiles[i];
            Vector2 pos = basePos;
            pos.x -= i * (w + spacing);
            rt.anchoredPosition = pos;
        }
    }

    float GetViewportRightWithOvershoot()
    {
        if (viewportRT == null) return 999999f;
        float vpHalfW = viewportRT.rect.width * 0.5f;
        float overshoot = tileWidth * Mathf.Max(0f, overshootMultiplier);
        return vpHalfW + overshoot + 2f; // 소폭 버퍼
    }

    int GetLeftmostIndex()
    {
        int idx = 0;
        float minX = float.MaxValue;
        for (int i = 0; i < tiles.Count; i++)
        {
            float left = tiles[i].anchoredPosition.x - tiles[i].pivot.x * tiles[i].rect.width;
            if (left < minX) { minX = left; idx = i; }
        }
        return idx;
    }
}
