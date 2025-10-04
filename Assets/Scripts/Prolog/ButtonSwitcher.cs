using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonSwitcher : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject btnHot;   // BtnHot 오브젝트
    public GameObject btnIced;  // BtnIced 오브젝트

    [Header("Transition")]
    public float duration = 0.35f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void SwitchToIced()
    {
        StartCoroutine(Switch(btnHot, btnIced));
    }

    public void SwitchToHot()
    {
        StartCoroutine(Switch(btnIced, btnHot));
    }

    private IEnumerator Switch(GameObject fromObj, GameObject toObj)
    {
        // CanvasGroup 준비
        var fromCg = fromObj.GetComponent<CanvasGroup>();
        if (!fromCg) fromCg = fromObj.AddComponent<CanvasGroup>();

        var toCg = toObj.GetComponent<CanvasGroup>();
        if (!toCg) toCg = toObj.AddComponent<CanvasGroup>();

        // 시작 세팅
        toObj.SetActive(true);
        toCg.alpha = 0f;
        toCg.interactable = false;
        toCg.blocksRaycasts = false;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = curve.Evaluate(t / duration);

            fromCg.alpha = 1f - k;
            toCg.alpha = k;

            yield return null;
        }

        // 최종 상태 고정
        fromObj.SetActive(false);
        fromCg.alpha = 1f;

        toCg.alpha = 1f;
        toCg.interactable = true;
        toCg.blocksRaycasts = true;
    }
}
