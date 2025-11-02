using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class ButtonVideoPlayer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float debounceSeconds = 0.2f; // 연속 클릭 방지
    private float _lastClickTime;
    private bool hasPlayedOnce = false; // 🎬 처음 재생 여부 추적

    void Awake()
    {
        if (!videoPlayer) videoPlayer = GetComponent<VideoPlayer>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.unscaledTime - _lastClickTime < debounceSeconds) return;
        _lastClickTime = Time.unscaledTime;

        TogglePlayPause();
    }

    public void TogglePlayPause()
    {
        if (!videoPlayer)
        {
            Debug.LogWarning("⚠️ VideoPlayer not assigned!");
            return;
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("⏸ Video paused");
        }
        else
        {
            // 🎬 비디오 재생
            videoPlayer.Play();
            Debug.Log("▶️ Video playing");

            // ✅ 첫 재생 시에만 DiaryClose 사운드 출력
            if (!hasPlayedOnce)
            {
                hasPlayedOnce = true;

                if (Chap2SoundManager.Instance != null)
                {
                    Chap2SoundManager.Instance.PlayDiaryClose();
                    Debug.Log("📖 DiaryClose 사운드 재생 (첫 영상 재생 시)");
                }
                else
                {
                    Debug.LogWarning("⚠️ Chap2SoundManager.Instance를 찾을 수 없습니다!");
                }
            }
        }
    }
}
