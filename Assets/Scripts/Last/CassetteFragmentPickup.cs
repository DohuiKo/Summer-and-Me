using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CassetteFragmentFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1.2f;  // 서서히 사라지는 시간

    [Header("Audio Settings")]
    public AudioSource pickupSound;    // 기존 AudioSource (선택)
    public bool useSoundManager = true; // ✅ Chap5SoundManager 통해 재생할지 여부

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

        // ✅ Chap5SoundManager를 통한 사운드 재생 (우선순위 높음)
        if (useSoundManager && Chap5SoundManager.Instance != null)
        {
            AudioClip clip = Chap5SoundManager.Instance.soundDB.getTapePieceSFX; // 🔹 SoundDatabase 연결 필수
            if (clip != null)
            {
                Chap5SoundManager.Instance.PlaySFX(clip);
                Debug.Log("🎧 GetTapePieceSFX 재생 via Chap5SoundManager");
            }
            else
            {
                Debug.LogWarning("[CassetteFragmentFade] SoundDB.getTapePieceSFX가 비어있습니다!");
            }
        }
        // ✅ 기존 AudioSource 방식 (백업)
        else if (pickupSound != null)
        {
            pickupSound.Play();
        }

        // ✅ 페이드아웃 시작
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
