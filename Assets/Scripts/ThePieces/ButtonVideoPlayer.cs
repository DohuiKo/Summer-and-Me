using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

public class ButtonVideoPlayer : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private float debounceSeconds = 0.2f; // 연속 클릭 방지
    private float _lastClickTime;

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
            Debug.LogWarning("VideoPlayer not assigned!");
            return;
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            Debug.Log("⏸ Video paused");
        }
        else
        {
            videoPlayer.Play();
            Debug.Log("▶️ Video playing");
        }
    }
}
