using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 클릭 시: (3n번째마다) 여름 얼굴 Flash → 다음 상태로 전환.
/// 마지막 상태 도달 시: 1초간 장면 유지 후 깨진 거울(brokenMirror) 등장 + nextDownArrow 버튼 활성화.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class MirrorChanger : MonoBehaviour
{
    [Header("Target (필수)")]
    public Image targetImage;
    public RectTransform targetRect;

    [Header("Sprite & Size Settings")]
    public List<SpriteState> spriteStates = new List<SpriteState>();

    [Header("Final State Action (마스터 스위치)")]
    [Tooltip("마지막 상태 도달 시 깨진 거울 FX 오브젝트를 활성화할지 여부")]
    public bool enableActionOnLastState = false;

    [Tooltip("마지막 상태 도달 후 활성화할 깨진 거울 FX 오브젝트")]
    public GameObject brokenMirror;

    [Tooltip("깨진 거울이 표시된 직후 활성화할 다음 버튼 오브젝트")]
    public GameObject nextDownArrow;

    [Tooltip("마지막 상태를 화면에 그대로 보여줄 시간(초)")]
    public float delayBeforeShowOnLastState = 1.0f;

    [Tooltip("마지막 상태 이후 입력 차단 여부")]
    public bool blockInputOnLastState = true;

    [Header("Input")]
    public Button clickSource;

    [Header("Wave/Blur Effect (권장)")]
    public Image fullScreenEffectImage;
    public Material waveBlurMaterial;

    [Header("Effect Tuning")]
    [Min(0.05f)] public float effectDuration = 0.6f;
    public float maxDistortStrength = 0.25f;
    public float maxBlurAmount = 0.6f;
    public float waveSpeed = 3.0f;

    [Header("Fallback (머티리얼 없을 때)")]
    public float punchScale = 1.06f;
    [Range(0f, 1f)] public float flashMaxAlpha = 0.25f;
    public AnimationCurve effectCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("SFX (옵션)")]
    public AudioSource sfxOnChange;

    [Header("Yeoreum Flash (여름 얼굴 번쩍)")]
    public bool enableYeoreumFlash = true;
    public Sprite yeoreumFlashSprite;
    public float yeoreumFlashDuration = 0.3f;
    public int flashEveryN = 3;

    private int currentStateIndex = 0;
    private int stateChangeCount = 0;
    private bool reachedLastOnce = false;

    private Coroutine runningEffect;
    private Material runtimeMatInstance;
    private CanvasGroup effectCanvasGroup;
    private float matTime;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<Image>();
        if (!targetRect)  targetRect  = targetImage ? targetImage.rectTransform : GetComponent<RectTransform>();

        if (fullScreenEffectImage)
        {
            effectCanvasGroup = fullScreenEffectImage.GetComponent<CanvasGroup>();
            if (!effectCanvasGroup) effectCanvasGroup = fullScreenEffectImage.gameObject.AddComponent<CanvasGroup>();
            effectCanvasGroup.alpha = 0f;

            if (waveBlurMaterial)
            {
                runtimeMatInstance = new Material(waveBlurMaterial);
                fullScreenEffectImage.material = runtimeMatInstance;
                SetMaterialValues(0f, 0f);
            }
        }

        // 시작 시 깨진 거울 및 화살표 비활성화
        if (brokenMirror) brokenMirror.SetActive(false);
        if (nextDownArrow) nextDownArrow.SetActive(false);
    }

    void Start()
    {
        if (spriteStates != null && spriteStates.Count > 0)
            ApplyStateInstant(0);

        if (clickSource)
        {
            clickSource.onClick.RemoveAllListeners();
            clickSource.onClick.AddListener(NextState);
        }
    }

    public void NextState()
    {
        if (reachedLastOnce && blockInputOnLastState) return;

        if (spriteStates == null || spriteStates.Count == 0)
        {
            Debug.LogWarning("[MirrorChanger] SpriteStates가 비어 있습니다.");
            return;
        }

        if (currentStateIndex >= spriteStates.Count - 1)
        {
            Debug.Log("[MirrorChanger] 마지막 스프라이트 상태입니다.");
            return;
        }

        int nextIndex = currentStateIndex + 1;
        if (runningEffect != null) StopCoroutine(runningEffect);
        runningEffect = StartCoroutine(Co_FlashThenSwap(nextIndex));
    }

    private IEnumerator Co_FlashThenSwap(int nextIndex)
    {
        stateChangeCount++;

        bool shouldFlash = enableYeoreumFlash &&
                           yeoreumFlashSprite != null &&
                           flashEveryN > 0 &&
                           (stateChangeCount % flashEveryN == 0);

        // 화면 효과 시작
        Coroutine fx = StartCoroutine(Co_WaveBlurEffect());
        if (sfxOnChange) sfxOnChange.Play();

        // 1) 여름 얼굴 Flash (3n번째만)
        if (shouldFlash && targetImage)
        {
            Sprite original = targetImage.sprite;
            targetImage.sprite = yeoreumFlashSprite;

            float t = 0f;
            while (t < yeoreumFlashDuration)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        // 2) 다음 상태 적용
        ApplyStateInstant(nextIndex);
        currentStateIndex = nextIndex;

        // 3) 마지막 상태 처리
        bool isLast = (currentStateIndex == spriteStates.Count - 1);
        if (isLast)
        {
            reachedLastOnce = true;
            StartCoroutine(Co_ShowLastActionAfterDelay());
        }

        yield return fx;
        runningEffect = null;
    }

    private IEnumerator Co_ShowLastActionAfterDelay()
    {
        // 마지막 장면 유지
        float t = 0f;
        while (t < delayBeforeShowOnLastState)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        // 깨진 거울 등장
        if (brokenMirror) brokenMirror.SetActive(true);

        // 동시에 다음 페이지 버튼 활성화
        if (nextDownArrow) nextDownArrow.SetActive(true);
    }

    private void ApplyStateInstant(int index)
    {
        if (index < 0 || index >= spriteStates.Count) return;
        var state = spriteStates[index];
        if (targetImage) targetImage.sprite = state.sprite;
        if (state.overrideSize && targetRect) targetRect.sizeDelta = state.size;
    }

    private IEnumerator Co_WaveBlurEffect()
    {
        float t = 0f;
        matTime = 0f;
        Vector3 baseScale = targetRect ? targetRect.localScale : Vector3.one;

        while (t < effectDuration)
        {
            t += Time.unscaledDeltaTime;
            float norm = Mathf.Clamp01(t / effectDuration);
            float k = effectCurve != null ? effectCurve.Evaluate(norm) : norm;

            if (runtimeMatInstance && fullScreenEffectImage)
            {
                matTime += Time.unscaledDeltaTime * waveSpeed;
                float distort = maxDistortStrength * k * Mathf.Sin(matTime);
                float blur = maxBlurAmount * k;
                SetMaterialValues(distort, blur);

                if (effectCanvasGroup)
                    effectCanvasGroup.alpha = flashMaxAlpha * (1f - Mathf.Abs(0.5f - norm) * 2f);
            }
            else
            {
                if (targetRect)
                {
                    float s = Mathf.Lerp(1f, punchScale, k);
                    targetRect.localScale = baseScale * s;
                }
                if (effectCanvasGroup)
                    effectCanvasGroup.alpha = Mathf.Lerp(0f, flashMaxAlpha, k);
            }
            yield return null;
        }

        if (runtimeMatInstance && fullScreenEffectImage)
        {
            SetMaterialValues(0f, 0f);
            if (effectCanvasGroup) effectCanvasGroup.alpha = 0f;
        }
        else
        {
            if (targetRect) targetRect.localScale = baseScale;
            if (effectCanvasGroup) effectCanvasGroup.alpha = 0f;
        }
    }

    private void SetMaterialValues(float distortStrength, float blurAmount)
    {
        if (!runtimeMatInstance) return;
        if (runtimeMatInstance.HasProperty("_DistortStrength"))
            runtimeMatInstance.SetFloat("_DistortStrength", distortStrength);
        if (runtimeMatInstance.HasProperty("_BlurAmount"))
            runtimeMatInstance.SetFloat("_BlurAmount", blurAmount);
        if (runtimeMatInstance.HasProperty("_TimeFactor"))
            runtimeMatInstance.SetFloat("_TimeFactor", matTime);
    }

    [ContextMenu("Next State (Test)")]
    private void _Context_Next() => NextState();
}
