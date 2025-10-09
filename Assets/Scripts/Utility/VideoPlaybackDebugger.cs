using UnityEngine;
using UnityEngine.Video;
using System.Collections;

public class VideoPlaybackDebugger : MonoBehaviour
{
    // 인스펙터에서 Video Player를 직접 연결해주세요.
    public VideoPlayer videoPlayer;

    // 테스트용 버튼에서 이 함수를 호출할 겁니다.
    public void AttemptToPlay()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("디버그 실패: VideoPlayer가 스크립트에 연결되지 않았습니다!");
            return;
        }

        Debug.Log("디버그: 재생을 시도합니다...");

        if (videoPlayer.clip == null)
        {
            Debug.LogError("디버그 실패: VideoPlayer에 비디오 클립(Video Clip)이 없습니다!");
            return;
        }

        // 비디오 재생 시도
        videoPlayer.Play();

        // 잠시 후 실제로 재생이 시작되었는지 확인
        StartCoroutine(CheckPlaybackState());
    }

    private IEnumerator CheckPlaybackState()
    {
        // 0.1초만 기다립니다.
        yield return new WaitForSeconds(0.1f);

        if (videoPlayer.isPlaying)
        {
            Debug.Log("<color=green>디버그 성공: 비디오가 정상적으로 재생을 시작했습니다!</color>");
        }
        else
        {
            Debug.LogError("디버그 실패: Play()를 호출했지만 비디오가 재생되지 않습니다. 비디오 파일 형식(코덱) 문제일 가능성이 높습니다.");
        }
    }
}