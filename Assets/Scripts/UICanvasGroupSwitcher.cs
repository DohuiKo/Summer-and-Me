using UnityEngine;
using System.Collections;

public class UICanvasGroupSwitcher : MonoBehaviour
{
    [Header("Transition")]
    public float duration = 0.35f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0,0,1,1);

    bool playing;

    public void Switch(CanvasGroup from, CanvasGroup to)
    {
        if (!from || !to || playing) return;
        StartCoroutine(CoSwitch(from, to));
    }

    IEnumerator CoSwitch(CanvasGroup from, CanvasGroup to)
    {
        playing = true;

        to.gameObject.SetActive(true);
        to.blocksRaycasts = true;

        float t = 0f;
        float aFrom0 = from.alpha;
        to.alpha = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float e = curve.Evaluate(Mathf.Clamp01(t / duration));
            from.alpha = Mathf.Lerp(aFrom0, 0f, e);
            to.alpha   = Mathf.Lerp(0f, 1f, e);
            yield return null;
        }

        from.alpha = 0f;
        from.blocksRaycasts = false;
        from.gameObject.SetActive(false);

        to.alpha = 1f;
        to.blocksRaycasts = true;

        playing = false;
    }
}
