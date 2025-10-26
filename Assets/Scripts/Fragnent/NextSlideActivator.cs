using UnityEngine;

public class NextSlideActivator : MonoBehaviour
{
    [Header("활성화할 오브젝트")]
    public GameObject nextSlideArrow;

    [Header("대기 시간 (초)")]
    public float delay = 3f;

    void OnEnable()
    {
        // 페이지가 활성화될 때 코루틴 시작
        if (nextSlideArrow != null)
        {
            nextSlideArrow.SetActive(false);
            StartCoroutine(ActivateAfterDelay());
        }
    }

    System.Collections.IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        nextSlideArrow.SetActive(true);
    }
}
