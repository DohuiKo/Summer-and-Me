using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TuneNote : MonoBehaviour
{
    public int noteIndex = -1;
    public bool isObstacle = false;

    private Button button;
    private Image image;
    private TextMeshProUGUI text;
    private CanvasGroup cg;

    private readonly Color normalColor = new Color(0.55f, 0.55f, 0.55f, 0.8f);
    private readonly Color correctColor = new Color(0.3f, 0.75f, 0.5f, 1f);
    private readonly Color wrongColor = new Color(0.8f, 0.3f, 0.3f, 1f);
    private readonly Color obstacleColor = new Color(0.8f, 0.3f, 0.3f, 0.9f);

    public void Initialize(int index, bool obstacle)
    {
        noteIndex = index;
        isObstacle = obstacle;

        image = GetComponentInChildren<Image>(true);
        text = GetComponentInChildren<TextMeshProUGUI>(true);
        button = GetComponent<Button>();
        cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();

        if (image == null || text == null)
        {
            Debug.LogError($"[TuneNote] Missing Image/Text on {name}");
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        image.color = normalColor;
        text.text = isObstacle ? "-" : (index >= 0 ? index.ToString() : "♪");

        cg.alpha = 0f;
        if (isActiveAndEnabled)
            StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float t = 0;
        while (t < 1f)
        {
            if (!isActiveAndEnabled) yield break;
            t += Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(0, 1, t / 1f);
            yield return null;
        }
        cg.alpha = 1;
    }

    public void OnClick()
    {
        if (!isActiveAndEnabled) return;

        if (isObstacle)
        {
            TuneGameManager.Instance.OnObstacleClicked();
            StartCoroutine(FlashError());
            return;
        }

        // 🎯 GameManager에게 판정 요청
        bool isCorrect = TuneGameManager.Instance.HandleNoteClickAndReturnResult(this);

        // ✅ 정답이면 Fade-out / ❌ 오답이면 색상 복귀
        StartCoroutine(ClickPulse(isCorrect));
    }

    IEnumerator ClickPulse(bool shouldFadeOut)
    {
        if (image == null || cg == null) yield break;

        float t = 0;
        Color startColor = image.color;
        Color targetColor = shouldFadeOut ? correctColor : wrongColor;

        // 색상 변화
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            image.color = Color.Lerp(startColor, targetColor, t / 0.3f);
            yield return null;
        }

        // 펄스 효과
        Vector3 baseScale = transform.localScale;
        float pulseTime = 0.4f;
        float elapsed = 0;
        while (elapsed < pulseTime)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / pulseTime * Mathf.PI) * 0.2f;
            transform.localScale = baseScale * scale;
            yield return null;
        }
        transform.localScale = baseScale;

        // ✅ 정답이면 사라짐
        if (shouldFadeOut)
        {
            yield return StartCoroutine(FadeOutAndDestroy());
        }
        else
        {
            // ❌ 오답이면 원래 색 복귀
            float r = 0;
            while (r < 0.3f)
            {
                r += Time.deltaTime;
                image.color = Color.Lerp(wrongColor, normalColor, r / 0.3f);
                yield return null;
            }
            image.color = normalColor;
        }
    }

    IEnumerator FadeOutAndDestroy()
    {
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.SmoothStep(1, 0, t / 1f);
            yield return null;
        }

        if (this != null && gameObject != null)
            Destroy(gameObject);
    }

    IEnumerator FlashError()
    {
        if (image == null) yield break;
        Color original = image.color;

        for (int i = 0; i < 3; i++)
        {
            image.color = obstacleColor;
            transform.localScale = Vector3.one * 1.1f;
            yield return new WaitForSeconds(0.1f);
            image.color = original;
            transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
