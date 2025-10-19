using UnityEngine;
using UnityEngine.Video;

public class PlayVideoAfterModal : MonoBehaviour
{
    [Header("참조 연결")]
    public MimiModal miMaiModal;        // ← 타입을 MimiModal로!
    public VideoPlayer videoPlayer;

    void Start()
    {
        if (miMaiModal != null && videoPlayer != null)
        {
            miMaiModal.OnSequenceEnd += OnModalSequenceEnd;
        }
        else
        {
            Debug.LogWarning("[PlayVideoAfterModal] 참조가 비었습니다. miMaiModal / videoPlayer 확인!");
        }
    }

    private void OnModalSequenceEnd()
    {
        // 모달 닫기(원하면 유지해도 됨)
        if (miMaiModal != null) miMaiModal.gameObject.SetActive(false);

        // 비디오 바로 재생
        if (videoPlayer != null)
        {
            videoPlayer.Play();
            Debug.Log("🎬 Video started after MimiModal finished.");
        }
    }

    void OnDestroy()
    {
        if (miMaiModal != null)
            miMaiModal.OnSequenceEnd -= OnModalSequenceEnd;
    }
}
