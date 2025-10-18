using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PhoneInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("흔들림 효과 설정")]
    [Tooltip("얼마나 오래 흔들릴지 (초)")]
    public float shakeDuration = 0.5f;
    [Tooltip("얼마나 세게 흔들릴지")]
    public float shakeMagnitude = 4f;

    [Header("입력 방지 설정")]
    [Tooltip("화면 전체를 덮는 투명한 UI의 Canvas Group을 연결하세요.")]
    public CanvasGroup inputBlocker;

    private Vector3 originalPosition;
    private Coroutine runningShake;

    void Awake()
    {
        // 스크립트가 시작될 때 휴대폰의 원래 위치를 기억합니다.
        originalPosition = GetComponent<RectTransform>().anchoredPosition;
    }

    // 이 오브젝트가 클릭되었을 때 자동으로 실행되는 함수입니다.
    public void OnPointerClick(PointerEventData eventData)
    {
        // 이미 흔들리는 중이면 또 실행하지 않습니다.
        if (runningShake == null)
        {
            runningShake = StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        // 1. 입력 방해판을 활성화해서 다른 곳이 클릭되는 것을 막습니다.
        if (inputBlocker != null)
        {
            inputBlocker.blocksRaycasts = true;
        }

        float elapsedTime = 0f;

        // 2. 정해진 시간 동안 휴대폰 위치를 무작위로 바꿉니다.
        while (elapsedTime < shakeDuration)
        {
            float xOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;
            float yOffset = Random.Range(-0.5f, 0.5f) * shakeMagnitude;

            GetComponent<RectTransform>().anchoredPosition = originalPosition + new Vector3(xOffset, yOffset, 0);

            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 기다립니다.
        }

        // 3. 흔들림이 끝나면 휴대폰을 원래 위치로 되돌립니다.
        GetComponent<RectTransform>().anchoredPosition = originalPosition;

        // 4. 입력 방해판을 비활성화해서 다시 다른 곳을 클릭할 수 있게 합니다.
        if (inputBlocker != null)
        {
            inputBlocker.blocksRaycasts = false;
        }

        // 실행 중인 코루틴 정보를 초기화합니다.
        runningShake = null;
    }
}