using UnityEngine;
using UnityEngine.SceneManagement;

public class Chap1SoundManager : MonoBehaviour
{
    public static Chap1SoundManager Instance;

    [Range(0f, 1f)] public float bgmVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    private AudioSource bgmSource;
    private AudioSource sfxSource;

    void Awake()
    {
        // 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 오디오 소스 준비
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = bgmVolume;
        sfxSource.volume = sfxVolume;

        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void Start()
    {
        PlayChap1BGM();
    }

    void OnDestroy()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        StopBGM();
    }

    // ✅ BGM 자동 재생
    private void PlayChap1BGM()
    {
        if (AudioManager.Instance == null || AudioManager.Instance.soundDB == null)
        {
            Debug.LogWarning("⚠️ AudioManager 또는 SoundDB를 찾을 수 없습니다!");
            return;
        }

        var bgmClip = AudioManager.Instance.soundDB.chap1BGM;
        if (bgmClip == null)
        {
            Debug.LogWarning("⚠️ Chap1 BGM이 SoundDB에 등록되어 있지 않습니다!");
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.Play();
        Debug.Log("🎵 Chap1 BGM 자동 재생 시작 (Start)");
    }

    // ✅ 효과음 공용 재생 함수
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null)
        {
            Debug.LogWarning("⚠️ 재생할 클립이 없습니다!");
            return;
        }
        sfxSource.PlayOneShot(clip);
    }

    // ✅ 개별 사운드 단축 메서드
    public void PlayDoorOpen()
    {
        PlaySFX(AudioManager.Instance?.soundDB?.doorOpenSFX);
        Debug.Log("🚪 문 여는 소리 재생");
    }

    public void PlayWater()
    {
        PlaySFX(AudioManager.Instance?.soundDB?.waterSquirtSFX);
        Debug.Log("💧 스프레이 소리 재생");
    }

    public void PlayDial()
    {
        PlaySFX(AudioManager.Instance?.soundDB?.dialSFX);
        Debug.Log("🎛️ 다이얼 돌리는 소리 재생");
    }

    public void PlayBadding()
    {
        PlaySFX(AudioManager.Instance?.soundDB?.baddingSFX);
        Debug.Log("🛏️ 이불 사운드 재생");
    }

    // ✅ BGM 중단
    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
        {
            bgmSource.Stop();
            Debug.Log("🎵 Chap1 BGM 자동 정지 (씬 이동)");
        }
    }
}
