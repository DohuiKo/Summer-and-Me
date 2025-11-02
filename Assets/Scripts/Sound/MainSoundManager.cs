using UnityEngine;

public class MainSoundManager : MonoBehaviour
{
    public static MainSoundManager Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 메인 화면 시작 시 자동으로 BGM 재생
        if (AudioManager.Instance != null)
        {
            PlayMainBGM();
        }
    }
    
    // 🔥 씬을 벗어날 때 BGM 중단!
    void OnDestroy()
    {
        // MainSoundManager가 파괴될 때 = 다른 씬으로 이동할 때
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopBGM();
            Debug.Log("메인 BGM 중단!");
        }
    }
    
    #region BGM
    
    public void PlayMainBGM()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("❌ AudioManager가 없습니다!");
            return;
        }
        
        AudioManager.Instance.PlayMainBGM();
        Debug.Log("메인 BGM 재생!");
    }
    
    #endregion
    
    #region 공용 효과음
    
    public void PlayObjClick()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayObjClick();
    }
    
    public void PlayGetTapePiece()
    {
        if (AudioManager.Instance == null) return;
        AudioManager.Instance.PlayGetTapePiece();
    }
    
    #endregion
}