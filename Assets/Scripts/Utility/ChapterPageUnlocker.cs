using UnityEngine;
using System.Collections; 

public class ChapterPageUnlocker : MonoBehaviour
{
    // 1. 인스펙터에 할당할 스크롤락 해제 버튼 (NextDownArrow)
    public GameObject unlockButton; 

    // 2. 응시(대기) 시간 설정
    public float gazeDuration = 3.0f; 

    void OnEnable()
    {
        if (unlockButton == null)
        {
            Debug.LogError("Unlock Button이 할당되지 않아 ChapterPageUnlocker가 작동할 수 없습니다.");
            return;
        }

        // 페이지가 나타날 때 (OnEnable) 버튼을 비활성화하고 타이머 시작
        unlockButton.SetActive(false);
        StartCoroutine(GazeTimerAndActivate());
    }

    void OnDisable()
    {
        // 페이지가 비활성화될 때 타이머 중지 (다른 페이지로 전환 시)
        StopAllCoroutines();
    }

    IEnumerator GazeTimerAndActivate()
    {
        Debug.Log($"Chapter 페이지 응시 타이머 시작: {gazeDuration}초 후 버튼 활성화.");
        
        // 3초간 대기 (사용자가 화면을 응시하는 시간)
        yield return new WaitForSeconds(gazeDuration);

        // 딜레이 후 버튼 활성화
        if (unlockButton != null)
        {
            unlockButton.SetActive(true);
            Debug.Log("스크롤락 해제 버튼 활성화 완료.");
        }
    }
}