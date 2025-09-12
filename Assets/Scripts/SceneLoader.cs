/* using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadProlog()
    {
        SceneManager.LoadScene("0_prolog");
    }
} */

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Defaults (optional)")]
    [SerializeField] private string prologSceneName = "0_prolog"; // 기존 프롤로그
    [SerializeField] private string homeSceneName   = "main"; // 홈(메인) 씬 이름
    [SerializeField] private LoadSceneMode loadMode = LoadSceneMode.Single;

    // ── 버튼에 바로 연결할 프리셋 ──
    public void LoadProlog() => LoadByName(prologSceneName);
    public void LoadHome()   => LoadByName(homeSceneName);

    // ── 범용: 버튼 OnClick에 파라미터로 씬 이름/인덱스를 넘겨 호출 ──
    public void LoadByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName) && Application.CanStreamedLevelBeLoaded(sceneName))
            SceneManager.LoadScene(sceneName, loadMode);
        else
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' 가 Build Settings에 없습니다.");
    }

    public void LoadByIndex(int buildIndex)
    {
        if (buildIndex >= 0 && buildIndex < SceneManager.sceneCountInBuildSettings)
            SceneManager.LoadScene(buildIndex, loadMode);
        else
            Debug.LogError($"[SceneLoader] 인덱스 {buildIndex} 가 범위를 벗어났습니다.");
    }
}
