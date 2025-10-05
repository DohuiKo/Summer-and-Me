using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UISprayBottle : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite idleSprite;
    public Sprite pressedSprite;

    [Header("Refs")]
    public RectTransform nozzle;          // 입구
    public Image waterSprite;             // 자식 Image(비활성 시작 권장)
    public RectTransform cactusRect;      // 선인장

    [Header("Tuning")]
    public Vector2 vfxOffsetPx = Vector2.zero;
    public float hitRadiusPx = 100f;
    public bool pickupAnywhere = false;   // true면 어디서 눌러도 픽업

    // ---- internal ----
    Canvas canvas;
    Camera uiCam;
    RectTransform rt;
    Image bottleImage;
    GraphicRaycaster raycaster;
    bool pickedUp = false;
    bool cooling = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        rt = GetComponent<RectTransform>();
        bottleImage = GetComponent<Image>();
        raycaster = canvas.GetComponent<GraphicRaycaster>();

        if (!canvas || !rt || !bottleImage || !raycaster)
        {
            Debug.LogError("[UISprayBottle] Canvas/Image/GraphicRaycaster 누락");
            enabled = false; return;
        }

        uiCam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        if (!nozzle)
        {
            var go = new GameObject("Nozzle", typeof(RectTransform));
            go.transform.SetParent(transform, false);
            nozzle = go.GetComponent<RectTransform>();
            nozzle.sizeDelta = Vector2.zero;
            nozzle.anchoredPosition = new Vector2(rt.rect.width * 0.25f, rt.rect.height * 0.15f);
        }

        // 자식 VFX가 클릭 막지 않도록
        if (waterSprite) waterSprite.raycastTarget = false;
    }

    void Update()
    {
        // 1) 픽업 시작: 마우스로 "분무기쪽" 눌렀는지 UI 레이캐스트로 판단
        if (!pickedUp && Input.GetMouseButtonDown(0))
        {
            if (pickupAnywhere || IsPointerOverBottle())
            {
                pickedUp = true;
                // 선택: 커서 숨기고 싶으면 주석 해제
                // Cursor.visible = false;
            }
        }

        // 2) 픽업 중엔 마우스 따라다니기
        if (pickedUp)
        {
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                uiCam,
                out localPos
            );
            rt.anchoredPosition = localPos;

            // 클릭하면 분사
            if (Input.GetMouseButtonDown(0) && !cooling)
                StartCoroutine(SprayOnce());
        }
    }

    System.Collections.IEnumerator SprayOnce()
    {
        cooling = true;

        if (pressedSprite) bottleImage.sprite = pressedSprite;

        // 노즐 기준으로 물줄기 표시(자식 WaterSprite 사용)
        if (waterSprite && nozzle)
        {
            waterSprite.transform.SetParent(nozzle, false);
            var r = waterSprite.rectTransform;
            r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
            r.pivot = new Vector2(0.5f, 0.5f);
            r.anchoredPosition = vfxOffsetPx;
            r.localRotation = Quaternion.identity;
            r.localScale = Vector3.one;

            waterSprite.enabled = true;
            Invoke(nameof(HideWater), 0.25f);
        }

        // 선인장 히트
        if (nozzle && cactusRect)
        {
            Vector2 nz = RectTransformUtility.WorldToScreenPoint(uiCam, nozzle.position);
            Vector2 cz = RectTransformUtility.WorldToScreenPoint(uiCam, cactusRect.position);
            if (Vector2.Distance(nz, cz) <= hitRadiusPx)
            {
                var cactus = cactusRect.GetComponent<UICactus>();
                if (cactus) cactus.AddStack(1);
            }
        }

        yield return new WaitForSeconds(0.12f);
        if (idleSprite) bottleImage.sprite = idleSprite;
        yield return new WaitForSeconds(0.15f);
        cooling = false;
    }

    void HideWater()
    {
        if (waterSprite) waterSprite.enabled = false;
    }

    // UI 레이캐스트로 현재 포인터 아래가 분무기(혹은 그 자식)인지 확인
    bool IsPointerOverBottle()
    {
        var ped = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        raycaster.Raycast(ped, results);

        foreach (var r in results)
        {
            if (r.gameObject == bottleImage.gameObject) return true;
            if (r.gameObject.transform.IsChildOf(bottleImage.transform)) return true;
        }
        return false;
    }
}
