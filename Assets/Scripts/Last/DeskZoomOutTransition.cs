using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DeskZoomOutTransition : MonoBehaviour
{
    [Header("Image References")]
    public Image closeUpImage;
    public Image fullDeskImage;
    public Image emptyDeskImage;

    [Header("Zoom Settings")]
    public float zoomOutDuration = 3f;
    public float fadeDuration = 2f;
    public float delayBetween = 1f;

    [Header("Cassette Fragment")]
    public GameObject cassetteFragmentButton; // ✅ 조각 버튼
    public GameObject nextSceneButton;        // ✅ 다음 씬 버튼 (새로 추가)

    void Start()
    {
        // 초기화
        closeUpImage.gameObject.SetActive(true);
        fullDeskImage.gameObject.SetActive(false);
        emptyDeskImage.gameObject.SetActive(false);

        if (cassetteFragmentButton != null)
            cassetteFragmentButton.SetActive(false); // 처음엔 꺼두기
        if (nextSceneButton != null)
            nextSceneButton.SetActive(false); // 다음씬 버튼도 꺼두기

        StartCoroutine(PlayTransition());
    }

    IEnumerator PlayTransition()
    {
        yield return new WaitForSeconds(1f);

        // 확대컷 → 전체컷
        yield return StartCoroutine(FadeSwitch(closeUpImage, fullDeskImage));
        yield return new WaitForSeconds(delayBetween);

        // 전체컷 → 빈 책상
        yield return StartCoroutine(FadeSwitch(fullDeskImage, emptyDeskImage));

        // ✅ 모든 컷이 끝난 후 조각 버튼 활성화
        if (cassetteFragmentButton != null)
        {
            yield return new WaitForSeconds(0.5f);
            cassetteFragmentButton.SetActive(true);

            // 버튼 클릭 이벤트 등록
            Button btn = cassetteFragmentButton.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(OnFragmentPickup);
        }
    }

    IEnumerator FadeSwitch(Image from, Image to)
    {
        to.gameObject.SetActive(true);
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = t / fadeDuration;

            SetAlpha(from, 1f - alpha);
            SetAlpha(to, alpha);

            yield return null;
        }

        from.gameObject.SetActive(false);
        SetAlpha(to, 1f);
    }

    void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        var c = img.color;
        c.a = alpha;
        img.color = c;
    }

    // ✅ 조각 클릭 시 페이드아웃 후 버튼 제거 + 다음씬버튼 활성화
    // ✅ 조각 클릭 시 페이드아웃 후 버튼 제거 + 다음씬버튼 활성화
    void OnFragmentPickup()
    {
        if (cassetteFragmentButton != null && gameObject.activeInHierarchy)
            StartCoroutine(FadeOutFragment());
    }

    IEnumerator FadeOutFragment()
    {
        if (cassetteFragmentButton == null) yield break;

        Image fragmentImage = cassetteFragmentButton.GetComponent<Image>();
        Button fragmentButton = cassetteFragmentButton.GetComponent<Button>();

        if (fragmentButton != null)
            fragmentButton.interactable = false;

        float fadeTime = 1.2f;
        float t = 0f;

        // ✅ 페이드아웃 중에 사라지지 않도록 보호
        while (t < fadeTime)
        {
            if (fragmentImage == null) yield break;
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);
            SetAlpha(fragmentImage, alpha);
            yield return null;
        }

        // ✅ 완전 비활성화
        if (cassetteFragmentButton != null)
            cassetteFragmentButton.SetActive(false);

        // ✅ 페이드 후 0.5초 뒤 nextSceneButton 활성화
        yield return new WaitForSeconds(0.5f);

        if (nextSceneButton != null)
        {
            nextSceneButton.SetActive(true);
            Debug.Log("✅ 조각 획득 완료 → NextSceneButton 활성화됨");
        }
        else
        {
            Debug.LogWarning("⚠️ nextSceneButton이 Inspector에 연결되지 않았습니다!");
        }
    }

}
