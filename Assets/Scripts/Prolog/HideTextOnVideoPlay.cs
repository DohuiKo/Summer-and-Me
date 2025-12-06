using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Video;

public class HideTextOnVideoPlay : MonoBehaviour
{
    [Header("참조")]
    public VideoPlayer videoPlayer; // 해당 페이지 영상
    public Text uiText;             // 일반 Text 쓸 때 (없으면 비워둠)
    public TMP_Text tmpText;        // TMP 텍스트 쓸 때 (지금 이거 쓰겠지?)

    private bool hidden = false;

    void Update()
    {
        if (hidden) return;

        // 비디오가 실제 재생을 시작하면
        if (videoPlayer != null && videoPlayer.isPlaying)
        {
            if (uiText != null) uiText.enabled = false;
            if (tmpText != null) tmpText.enabled = false;

            hidden = true; // 한 번만 처리
        }
    }
}
