using UnityEngine;
using UnityEngine.UI;
using System.Collections; // 코루틴(IEnumerator)을 사용하기 위해 필요합니다.

public class PhotoStateChanger : MonoBehaviour
{
    // 💡 Inspector에서 연결할 사진 상태 Sprite 배열
    public Sprite[] photoStates;

    // 💡 마지막 상태에 나타날 오브젝트 (선택 사항)
    public GameObject objectToShow;

    // 💡 페이드 효과에 걸리는 시간 (초 단위, 인스펙터에서 조절 가능)
    public float fadeDuration = 2f;

    private int currentStateIndex = 0;
    private Image photoImage;
    private Coroutine runningFadeCoroutine = null; // 현재 실행 중인 코루틴을 저장하기 위한 변수

    void Start()
    {
        photoImage = GetComponent<Image>();
        if (photoStates.Length > 0)
        {
            photoImage.sprite = photoStates[0];
        }
        if (objectToShow != null)
        {
            objectToShow.SetActive(false);
        }
    }

    // 💡 버튼의 OnClick() 이벤트에서 호출할 함수
    public void ChangePhotoState()
    {
        // 마지막 상태이거나, 이미 페이드 효과가 진행 중이면 아무것도 하지 않음
        if (currentStateIndex >= photoStates.Length - 1 || runningFadeCoroutine != null)
        {
            return;
        }

        currentStateIndex++;
        
        // 코루틴을 시작시키고, 실행 중인 코루틴 정보를 변수에 저장
        runningFadeCoroutine = StartCoroutine(FadeAndChangeSprite());
    }

    // 💡 서서히 이미지를 바꾸는 코루틴 함수
    private IEnumerator FadeAndChangeSprite()
    {
        // --- 1. 페이드 아웃 (서서히 투명해지기) ---
        float elapsedTime = 0f;
        Color originalColor = photoImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration); // 알파값을 1에서 0으로
            photoImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            yield return null; // 다음 프레임까지 대기
        }
        // 확실하게 투명하게 만듦
        photoImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        // --- 2. 스프라이트 교체 ---
        photoImage.sprite = photoStates[currentStateIndex];
        Debug.Log($"사진 상태 변경: 상태 {currentStateIndex + 1} ({photoStates[currentStateIndex].name})");

        // --- 3. 페이드 인 (서서히 나타나기) ---
        elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float newAlpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeDuration); // 알파값을 0에서 1로
            photoImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, newAlpha);
            yield return null; // 다음 프레임까지 대기
        }
        // 확실하게 불투명하게 만듦
        photoImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

        // --- 4. 최종 상태 처리 ---
        if (currentStateIndex == photoStates.Length - 1)
        {
            if (objectToShow != null)
            {
                objectToShow.SetActive(true);
                Debug.Log("최종 단서 발견: 숨겨진 오브젝트를 찾았습니다!");
            }
        }

        // 코루틴이 끝났으므로 변수를 null로 초기화
        runningFadeCoroutine = null;
    }
}