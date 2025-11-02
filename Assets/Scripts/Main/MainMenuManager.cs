using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요

public class MainMenuManager : MonoBehaviour
{
    // --- 인스펙터에서 연결할 변수들 ---

    // 1. "Start" 버튼 클릭 시 이동할 게임 씬의 이름
    public string gameSceneName = "GameScene"; // "GameScene"은 예시입니다. 실제 씬 이름으로 변경하세요.

    // 2. "My Box" 클릭 시 켤 팝업 UI (Panel)
    public GameObject myBoxPopup;

    // 3. "Settings" 클릭 시 켤 팝업 UI (Panel)
    public GameObject settingsPopup;

    // 4. "Exit" 클릭 시 켤 확인 팝업 UI (Panel)
    public GameObject exitConfirmPopup;


    // --- 버튼 클릭 이벤트에 연결할 함수들 ---

    // "Start" 버튼에 연결
    public void OnStartButton()
    {
        // "gameSceneName" 변수에 설정된 이름의 씬을 불러옵니다.
        SceneManager.LoadScene(gameSceneName);
    }

    // "My Box" 버튼에 연결
    public void OnMyBoxButton()
    {
        // "개발 중입니다" 팝업을 활성화합니다.
        if (myBoxPopup != null)
        {
            myBoxPopup.SetActive(true);
        }
    }

    // "Settings" 버튼에 연결
    public void OnSettingsButton()
    {
        // 설정 팝업을 활성화합니다.
        // (지금은 My Box와 동일하게 "개발 중" 팝업을 연결해도 됩니다)
        if (settingsPopup != null)
        {
            settingsPopup.SetActive(true);
        }
    }

    // "Exit" 버튼에 연결
    public void OnExitButton()
    {
        // 종료 확인 팝업을 활성화합니다.
        if (exitConfirmPopup != null)
        {
            exitConfirmPopup.SetActive(true);
        }
    }
}