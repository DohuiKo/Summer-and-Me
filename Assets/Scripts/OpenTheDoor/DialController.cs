using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DialController : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    private RectTransform needleRect;

    [Header("연동 이미지")]
    public Image backgroundBedImage;
    public Image phoneBlackScreenImage; 

    [Header("회전 설정")]
    [Tooltip("바늘이 회전할 수 있는 최대 누적 각도")]
    public float maxRotationAngle = 720f;
    [Tooltip("드래그 움직임에 대한 바늘 회전 속도")]
    public float dragSensitivity = 1.0f; 

    private float currentAngle = 0f;
    private Vector2 startDragPosition;

    private const float PHASE_1_END = 360f;
    private const float PHASE_2_END = 720f;

    // 🎧 사운드 관련
    [Header("사운드 설정")]
    private AudioSource dialAudioSource;
    private bool isDragging = false;
    private bool hasReachedEnd = false;

    void Start()
    {
        needleRect = GetComponent<RectTransform>();

        // 초기 투명도 설정
        SetAlpha(backgroundBedImage, 1f);
        SetAlpha(phoneBlackScreenImage, 0f);

        // 🎧 사운드 소스 초기화
        dialAudioSource = gameObject.AddComponent<AudioSource>();
        dialAudioSource.loop = true;
        dialAudioSource.playOnAwake = false;

        if (AudioManager.Instance != null && AudioManager.Instance.soundDB != null)
        {
            dialAudioSource.clip = AudioManager.Instance.soundDB.dialSFX;
            dialAudioSource.volume = AudioManager.Instance.sfxVolume;
        }
        else
        {
            Debug.LogWarning("⚠️ AudioManager 또는 dialSFX가 연결되지 않음");
        }
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color color = img.color;
        color.a = Mathf.Clamp(alpha, 0f, 1f);
        img.color = color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (hasReachedEnd) return; // 이미 끝났으면 반응 안 함
        isDragging = true;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            needleRect.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out startDragPosition);

        // 🔊 드래그 시작 시 사운드 재생 시작
        if (dialAudioSource != null && dialAudioSource.clip != null)
            dialAudioSource.Play();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (hasReachedEnd) return;
        if (!isDragging) return;

        Vector2 currentDragPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            needleRect.parent.GetComponent<RectTransform>(),
            eventData.position,
            eventData.pressEventCamera,
            out currentDragPosition);

        Vector2 center = needleRect.anchoredPosition;
        float startAngle = Mathf.Atan2(startDragPosition.y - center.y, startDragPosition.x - center.x) * Mathf.Rad2Deg;
        float currentAngleRad = Mathf.Atan2(currentDragPosition.y - center.y, currentDragPosition.x - center.x) * Mathf.Rad2Deg;
        float angleDifference = currentAngleRad - startAngle;

        if (angleDifference > 180) angleDifference -= 360;
        if (angleDifference < -180) angleDifference += 360;

        float adjustedAngleChange = angleDifference * dragSensitivity;
        currentAngle = Mathf.Clamp(currentAngle - adjustedAngleChange, 0f, maxRotationAngle);
        needleRect.localRotation = Quaternion.Euler(0, 0, -currentAngle);

        UpdateAlphaStates();

        if (currentAngle >= maxRotationAngle)
        {
            hasReachedEnd = true;
            isDragging = false;

            if (dialAudioSource.isPlaying)
                dialAudioSource.Stop();

            Debug.Log("✅ 다이얼 조작 완료! 사운드 중지 및 검은 화면 완료");
        }

        startDragPosition = currentDragPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 🔇 드래그 종료 시 사운드 멈춤
        if (dialAudioSource != null && dialAudioSource.isPlaying)
            dialAudioSource.Stop();

        isDragging = false;
    }

    private void UpdateAlphaStates()
    {
        // 1단계: 침대 페이드아웃
        float bedAlpha;
        if (currentAngle <= PHASE_1_END)
        {
            float progress = currentAngle / PHASE_1_END;
            bedAlpha = 1f - progress;
        }
        else
        {
            bedAlpha = 0f;
        }
        SetAlpha(backgroundBedImage, bedAlpha);

        // 2단계: 검은 화면 페이드인
        float blackScreenAlpha = 0f;
        if (currentAngle >= PHASE_1_END && currentAngle <= PHASE_2_END)
        {
            float phaseDuration = PHASE_2_END - PHASE_1_END;
            float phaseProgress = (currentAngle - PHASE_1_END) / phaseDuration;
            blackScreenAlpha = phaseProgress;
        }
        else if (currentAngle > PHASE_2_END)
        {
            blackScreenAlpha = 1f;
        }
        SetAlpha(phoneBlackScreenImage, blackScreenAlpha);
    }
}
