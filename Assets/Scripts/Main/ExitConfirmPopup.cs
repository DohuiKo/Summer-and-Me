using UnityEngine;

// UNITY_EDITOR 전처리기 지시문을 사용하기 위해 필요
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExitConfirmPopup : MonoBehaviour
{
    // "Yes" 버튼에 연결
    public void OnYesButton()
    {
        Debug.Log("게임을 종료합니다.");

        // 유니티 에디터에서 테스트 중일 경우
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        // 실제 빌드된 게임(PC, 모바일 등)에서 실행 중일 경우
#else
        Application.Quit();
#endif
    }

    // "No" 버튼에 연결
    public void OnNoButton()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트(종료 확인 팝업)를 닫습니다.
        gameObject.SetActive(false);
    }
}