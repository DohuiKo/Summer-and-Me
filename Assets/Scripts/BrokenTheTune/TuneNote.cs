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
    private readonly Color clickColor = new Color(0.3f, 0.75f, 0.5f, 1f);
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

        // ▼▼▼ 이 부분이 요청하신 대로 수정되었습니다 ▼▼▼
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
        }
        else
        {
            // GameManager에서 올바른 순서 여부 판단
            bool correct = TuneGameManager.Instance.TryProcessNote(this);

            // 맞았을 때만 사라짐
            if (correct)
                StartCoroutine(ClickPulse(true));   // 사라지는 연출 포함
            else
                StartCoroutine(ClickPulse(false));  // 색상만 변화 후 복귀
        }
    }

    IEnumerator ClickPulse(bool shouldFadeOut)
    {
        if (image == null || cg == null) yield break;

        // 색상 변화
        float t = 0;
        Color startColor = image.color;
        while (t < 0.4f)
        {
            t += Time.deltaTime;
            image.color = Color.Lerp(startColor, clickColor, Mathf.SmoothStep(0, 1, t / 0.4f));
            yield return null;
        }

        // 둥~ 커졌다가 작아지는 펄스 효과
        Vector3 baseScale = transform.localScale;
        float pulseTime = 0.45f;
        float elapsed = 0;
        while (elapsed < pulseTime)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(elapsed / pulseTime * Mathf.PI) * 0.2f;
            transform.localScale = baseScale * scale;
            yield return null;
        }
        transform.localScale = baseScale;

        // 올바른 순서일 때만 fade-out 후 삭제
        if (shouldFadeOut)
            yield return StartCoroutine(FadeOutAndDestroy());
        else
        {
            // 틀렸을 경우 원래 색상 복귀
            float r = 0;
            while (r < 0.4f)
            {
                r += Time.deltaTime;
                image.color = Color.Lerp(clickColor, normalColor, r / 0.4f);
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