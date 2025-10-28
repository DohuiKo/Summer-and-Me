using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CassetteFragmentFade : MonoBehaviour
{
    public float fadeDuration = 1.2f;  // 서서히 사라지는 시간
    public AudioSource pickupSound;    // 선택: 획득 사운드 연결 가능

    private Button button;
    private Image image;
    private bool isPickedUp = false;

    void Awake()
    {
        button = GetComponent<Button>();
        image = GetComponent<Image>();

        if (button != null)
            button.onClick.AddListener(OnPickup);
    }

    void OnPickup()
    {
        if (isPickedUp) return; // 중복 방지
        isPickedUp = true;

        if (pickupSound != null)
            pickupSound.Play();

        StartCoroutine(FadeOutAndHide());
    }

    IEnumerator FadeOutAndHide()
    {
        float t = 0f;
        Color original = image.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            image.color = new Color(original.r, original.g, original.b, alpha);
            yield return null;
        }

        gameObject.SetActive(false);
        Debug.Log("🎞️ 카세트 테이프 조각을 획득했습니다.");
    }
}
