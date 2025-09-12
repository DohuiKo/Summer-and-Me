using UnityEngine;

public class DelayedBGM : MonoBehaviour
{
    public AudioSource bgmAudioSource; // BGM 오디오 소스
    public float delay = 5f; // 지연 시간 (초 단위)

    void Start()
    {
        // 코루틴 실행
        StartCoroutine(PlayBGMAfterDelay());
    }

    private System.Collections.IEnumerator PlayBGMAfterDelay()
    {
        yield return new WaitForSeconds(delay);
        bgmAudioSource.Play();
    }
}
