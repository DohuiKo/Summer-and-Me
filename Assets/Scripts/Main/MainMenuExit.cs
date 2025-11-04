using UnityEngine;

public class MainMenuExit : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
        Debug.Log("[MainMenuExit] 💻 에디터 모드 - 종료 버튼 클릭됨");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("[MainMenuExit] 🚪 빌드 종료됨");
        Application.Quit();
#endif
    }
}
