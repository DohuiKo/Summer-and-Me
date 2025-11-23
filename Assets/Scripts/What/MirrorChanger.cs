using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class MirrorChanger : MonoBehaviour
{
    [Header("Target")]
    public Image targetImage;
    public RectTransform targetRect;

    [Header("Sprite States")]
    public List<SpriteState> spriteStates = new List<SpriteState>();

    [Header("Final State Action")]
    public GameObject brokenMirror;
    public GameObject nextDownArrow;
    public float delayBeforeShowOnLastState = 1.0f;
    public bool blockInputOnLastState = true;

    [Header("Input")]
    public Button clickSource;

    [Header("SFX")]
    public AudioSource sfxOnChange;
    public AudioSource sfxOnBroken;

    [Header("Wobble Effect (우글거림)")]
    public float wobbleDuration = 0.55f;
    public float wobbleScaleStrength = 0.012f;
    public float wobbleRotateStrength = 0.4f;
    public AnimationCurve wobbleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Yeoreum Flash (랜덤 플래시)")]
    public bool enableYeoreumFlash = true;
    public List<Sprite> yeoreumFlashList = new List<Sprite>();    
    public float yeoreumFlashDuration = 0.25f;
    public int flashEveryN = 3;

    private int currentStateIndex = 0;
    private int stateChangeCount = 0;

    private bool reachedLastOnce = false;
    private bool hasPlayedBrokenSFX = false;

    private Vector3 baseScale;
    private Quaternion baseRot;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<Image>();
        if (!targetRect) targetRect = targetImage.rectTransform;

        baseScale = targetRect.localScale;
        baseRot = targetRect.localRotation;

        if (brokenMirror) brokenMirror.SetActive(false);
        if (nextDownArrow) nextDownArrow.SetActive(false);
    }

    void Start()
    {
        if (spriteStates.Count > 0)
            ApplyStateInstant(0);

        clickSource.onClick.RemoveAllListeners();
        clickSource.onClick.AddListener(NextState);
    }

    public void NextState()
    {
        if (reachedLastOnce && blockInputOnLastState) return;
        if (currentStateIndex >= spriteStates.Count - 1) return;

        // ⭐ 클릭 순간 즉시 우글거림 시작
        StartCoroutine(Co_WobbleEffect());

        // 나머지는 기존 흐름대로 작동
        StartCoroutine(Co_ChangeState(currentStateIndex + 1));
    }

    private IEnumerator Co_ChangeState(int nextIndex)
    {
        stateChangeCount++;

        if (sfxOnChange) sfxOnChange.Play();

        // ⭐ 랜덤 Flash (딜레이 있음 → 하지만 Wobble은 이미 시작됨)
        if (enableYeoreumFlash &&
            yeoreumFlashList.Count > 0 &&
            flashEveryN > 0 &&
            (stateChangeCount % flashEveryN == 0))
        {
            Sprite original = targetImage.sprite;

            Sprite flashSprite = yeoreumFlashList[Random.Range(0, yeoreumFlashList.Count)];
            targetImage.sprite = flashSprite;

            yield return new WaitForSeconds(yeoreumFlashDuration);

            targetImage.sprite = original;
        }

        // 상태 전환
        ApplyStateInstant(nextIndex);
        currentStateIndex = nextIndex;

        // 마지막 상태 처리
        if (currentStateIndex == spriteStates.Count - 1)
        {
            reachedLastOnce = true;
            yield return StartCoroutine(Co_ShowLastAction());
        }
    }

    private IEnumerator Co_ShowLastAction()
    {
        yield return new WaitForSeconds(delayBeforeShowOnLastState);

        if (!hasPlayedBrokenSFX && sfxOnBroken)
        {
            sfxOnBroken.Play();
            hasPlayedBrokenSFX = true;
        }

        if (brokenMirror) brokenMirror.SetActive(true);
        if (nextDownArrow) nextDownArrow.SetActive(true);
    }

    private void ApplyStateInstant(int index)
    {
        if (index < 0 || index >= spriteStates.Count) return;

        var state = spriteStates[index];
        targetImage.sprite = state.sprite;

        if (state.overrideSize)
            targetRect.sizeDelta = state.size;
    }

    private IEnumerator Co_WobbleEffect()
    {
        float t = 0f;

        while (t < wobbleDuration)
        {
            t += Time.deltaTime;
            float k = wobbleCurve.Evaluate(t / wobbleDuration);

            float sx = 1 + Mathf.Sin(t * 6f) * wobbleScaleStrength * k;
            float sy = 1 + Mathf.Cos(t * 7f) * wobbleScaleStrength * k;
            targetRect.localScale = new Vector3(sx, sy, 1);

            float rot = Mathf.Sin(t * 4f) * wobbleRotateStrength * k;
            targetRect.localRotation = Quaternion.Euler(0, 0, rot);

            yield return null;
        }

        targetRect.localScale = baseScale;
        targetRect.localRotation = baseRot;
    }
}
