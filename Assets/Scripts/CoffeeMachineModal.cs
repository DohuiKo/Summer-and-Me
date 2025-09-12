using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CoffeeMachineModal : MonoBehaviour
{
    [Header("Refs (auto)")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private CanvasGroup modalLayer;   // "ModalLayer"
    [SerializeField] private RectTransform modeBoard;  // "ModeBoard"
    [SerializeField] private Image cupHot;             // "BtnHot"
    [SerializeField] private Image cupIced;            // "BtnIced"
    [SerializeField] private Button btnHot;
    [SerializeField] private Button btnIced;

    [Header("Anim")]
    [SerializeField] private float fadeDur = 0.25f;
    [SerializeField] private float popScale = 1.08f;

    private bool isOpen;

    // ---------- Utility ----------
#if UNITY_2023_1_OR_NEWER
    private static T FindAny<T>() where T : Object => Object.FindFirstObjectByType<T>();
#else
    private static T FindAny<T>() where T : Object => Object.FindObjectOfType<T>();
#endif

    private static Transform FindByName(Transform root, string name)
    {
        if (!root) return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }

    private void AutoAssign()
    {
        if (!scrollRect) scrollRect = GetComponentInParent<ScrollRect>(true) ?? FindAny<ScrollRect>();

        // 전체 씬 루트 기준으로 이름으로 찾기 (계층 어디에 있어도 OK)
        var sceneRoot = transform.root;

        if (!modalLayer)
            modalLayer = FindByName(sceneRoot, "ModalLayer")?.GetComponent<CanvasGroup>();

        if (!modeBoard)
            modeBoard = FindByName(sceneRoot, "ModeBoard") as RectTransform;

        if (!cupHot)
            cupHot = FindByName(sceneRoot, "BtnHot")?.GetComponent<Image>();

        if (!cupIced)
            cupIced = FindByName(sceneRoot, "BtnIced")?.GetComponent<Image>();

        if (!btnHot && cupHot)   btnHot  = cupHot.GetComponent<Button>();
        if (!btnIced && cupIced) btnIced = cupIced.GetComponent<Button>();
    }

    private void OnValidate() { AutoAssign(); }   // 에디터에서 자동 채움
    private void Reset()      { AutoAssign(); }   // 컴포넌트 붙일 때 자동 채움

    private void Awake()
    {
        AutoAssign(); // 런타임에서도 최종 보정

        // 초기 상태: ModalLayer는 항상 Active, CanvasGroup으로만 숨김
        if (modalLayer)
        {
            modalLayer.gameObject.SetActive(true);
            modalLayer.alpha = 0f;
            modalLayer.interactable = false;
            modalLayer.blocksRaycasts = false;
        }
        if (cupIced) cupIced.gameObject.SetActive(false);
        if (cupHot)  cupHot.gameObject.SetActive(true);
    }

    // ---------- 공개 API ----------
    public void OpenModes()
    {
        if (isOpen || !modalLayer) return;
        isOpen = true;

        modalLayer.interactable = true;
        modalLayer.blocksRaycasts = true;
        if (scrollRect) scrollRect.enabled = false;

        StartCoroutine(FadeInModal());
    }

    public void CloseModes()
    {
        if (!isOpen || !modalLayer) return;
        StartCoroutine(FadeOutModal());
    }

    public void SelectHot()
    {
        if (cupIced) cupIced.gameObject.SetActive(false);
        if (cupHot)  StartCoroutine(ShowCup(cupHot));
        CloseModes();
    }

    public void SelectIced()
    {
        if (cupHot)  cupHot.gameObject.SetActive(false);
        if (cupIced) StartCoroutine(ShowCup(cupIced));
        CloseModes();
    }

    // ---------- 코루틴 ----------
    private IEnumerator FadeInModal()
    {
        modalLayer.alpha = 0f;

        if (modeBoard)
        {
            var from = Vector3.one * popScale;
            var to   = Vector3.one;
            modeBoard.localScale = from;

            float t = 0f;
            while (t < fadeDur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.SmoothStep(0f, 1f, t / fadeDur);
                modalLayer.alpha = k;
                modeBoard.localScale = Vector3.LerpUnclamped(from, to, k);
                yield return null;
            }
            modeBoard.localScale = to;
        }
        else
        {
            float t = 0f;
            while (t < fadeDur)
            {
                t += Time.unscaledDeltaTime;
                modalLayer.alpha = Mathf.SmoothStep(0f, 1f, t / fadeDur);
                yield return null;
            }
        }
        modalLayer.alpha = 1f;
    }

    private IEnumerator FadeOutModal()
    {
        modalLayer.interactable = false;
        modalLayer.blocksRaycasts = false;

        float t = 0f;
        while (t < fadeDur)
        {
            t += Time.unscaledDeltaTime;
            modalLayer.alpha = Mathf.SmoothStep(1f, 0f, t / fadeDur);
            yield return null;
        }
        modalLayer.alpha = 0f;
        if (scrollRect) scrollRect.enabled = true;
        isOpen = false;
    }

    private IEnumerator ShowCup(Image cup)
    {
        var cg = cup.GetComponent<CanvasGroup>();
        if (!cg) cg = cup.gameObject.AddComponent<CanvasGroup>();

        cup.gameObject.SetActive(true);
        cg.alpha = 0f;
        cup.rectTransform.localScale = Vector3.one * 0.96f;

        const float d = 0.25f;
        float t = 0f;
        while (t < d)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / d);
            cg.alpha = k;
            cup.rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.96f, Vector3.one, k);
            yield return null;
        }
        cg.alpha = 1f;
        cup.rectTransform.localScale = Vector3.one;

        // --- 이 부분이 핵심입니다 ---
        // 컵 애니메이션이 끝난 후 3초를 기다립니다.
        yield return new WaitForSeconds(3f);

        // 3초가 지난 후 모달을 닫습니다.
        CloseModes();
    }

}
