using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource bgmPlayer;
    private AudioSource sfxPlayer;

    [SerializeField] public SoundDatabase soundDB;

    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeAudio()
    {
        bgmPlayer = gameObject.AddComponent<AudioSource>();
        sfxPlayer = gameObject.AddComponent<AudioSource>();

        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        sfxPlayer.volume = sfxVolume;
    }

    // ======================================================
    // 🎵 BGM 제어
    // ======================================================

    public void PlayBGM(AudioClip bgm)
    {
        if (bgm == null)
        {
            Debug.LogWarning("[AudioManager] 재생할 BGM이 null입니다!");
            return;
        }

        // 같은 곡이면 다시 시작하지 않음
        if (bgmPlayer.clip == bgm && bgmPlayer.isPlaying)
            return;

        bgmPlayer.clip = bgm;
        bgmPlayer.Play();
    }

    public void StopBGM() => bgmPlayer.Stop();
    public void PauseBGM() => bgmPlayer.Pause();
    public void ResumeBGM() => bgmPlayer.UnPause();

    public void FadeOutBGM(float duration = 1f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    public void FadeInBGM(AudioClip bgm, float duration = 1f)
    {
        StartCoroutine(FadeInCoroutine(bgm, duration));
    }

    /// ✅ 현재 재생 중인 BGM이 같은지 확인
    public bool IsBGMPlaying(AudioClip clip)
    {
        if (bgmPlayer == null || bgmPlayer.clip == null) return false;
        return bgmPlayer.isPlaying && bgmPlayer.clip == clip;
    }

    // ======================================================
    // 🎚️ 효과음
    // ======================================================

    public void PlaySFX(AudioClip sfx)
    {
        if (sfx == null)
        {
            Debug.LogWarning("[AudioManager] 재생할 SFX가 null입니다!");
            return;
        }

        sfxPlayer.PlayOneShot(sfx);
    }

    // ======================================================
    // 🎶 간편 호출용 (BGM & SFX)
    // ======================================================

    // 공용 SFX
    public void PlayObjClick() => PlaySFX(soundDB.objClickSFX);
    public void PlayGetTapePiece() => PlaySFX(soundDB.getTapePieceSFX);
    public void PlayMymyWinding() => PlaySFX(soundDB.mymyWindingSFX);
    public void PlayMymyOpen() => PlaySFX(soundDB.mymyOpenSFX);

    // BGM
    public void PlayMainBGM() => PlayBGM(soundDB.mainBGM);
    public void PlayPrologBGM() => PlayBGM(soundDB.prologBGM);
    public void PlayChap1BGM() => PlayBGM(soundDB.chap1BGM);
    public void PlayChap2BGM() => PlayBGM(soundDB.chap2BGM);
    public void PlayChap3BGM() => PlayBGM(soundDB.chap3BGM);
    public void PlayChap4BGM() => PlayBGM(soundDB.chap4BGM);
    public void PlayChap5BGM() => PlayBGM(soundDB.chap5BGM);
    public void PlayChap6BGM() => PlayBGM(soundDB.chap6BGM);

    // ✅ 새로 추가된 부분
    public void PlayBrokenTheTuneBGM() => PlayBGM(soundDB.brokenTheTuneBGM);

    // 챕터 4 SFX
    public void PlayAlarmPipipipi() => PlaySFX(soundDB.alarmPipipipiSFX);
    public void PlayFoldLaundry() => PlaySFX(soundDB.foldLaundrySFX);
    public void PlayMirrorBroken() => PlaySFX(soundDB.mirrorBrokenSFX);
    public void PlayCassetteGoingIn() => PlaySFX(soundDB.cassetteGoingInSFX);
    public void PlayMymyDoorClose() => PlaySFX(soundDB.mymyDoorCloseSFX);

    // 챕터 5 SFX
    public void PlayContinueTyping() => PlaySFX(soundDB.continueTypingSFX);
    public void PlayRecording() => PlaySFX(soundDB.recordingSFX);
    public void PlaySobbingGaeul() => PlaySFX(soundDB.sobbingGaeulSFX);

    // ======================================================
    // 🔊 볼륨 제어
    // ======================================================

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmPlayer.volume = bgmVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxPlayer.volume = sfxVolume;
    }

    // ======================================================
    // 🎬 코루틴 (페이드 효과)
    // ======================================================

    IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = bgmPlayer.volume;

        while (bgmPlayer.volume > 0)
        {
            bgmPlayer.volume -= startVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgmPlayer.Stop();
        bgmPlayer.volume = startVolume;
    }

    IEnumerator FadeInCoroutine(AudioClip bgm, float duration)
    {
        bgmPlayer.clip = bgm;
        bgmPlayer.volume = 0f;
        bgmPlayer.Play();

        float targetVolume = bgmVolume;

        while (bgmPlayer.volume < targetVolume)
        {
            bgmPlayer.volume += targetVolume * Time.deltaTime / duration;
            yield return null;
        }

        bgmPlayer.volume = targetVolume;
    }

    // === 🎧 이어듣기용 기능 추가 ===
    public AudioClip CurrentBGM => bgmPlayer.clip;

    public float GetCurrentBGMTime()
    {
        return bgmPlayer != null && bgmPlayer.isPlaying ? bgmPlayer.time : 0f;
    }

    public void PlayBGM(AudioClip bgm, float startTime)
    {
        if (bgm == null) return;
        bgmPlayer.clip = bgm;
        bgmPlayer.time = startTime;
        bgmPlayer.Play();
    }

    public AudioSource GetSFXPlayer() => sfxPlayer;

    // ======================================================
    // 🛑 모든 효과음(SFX) 중단 함수 추가 (Chap4SoundManager용)
    // ======================================================
    public void StopAllSFX()
    {
        if (sfxPlayer == null) return;

        sfxPlayer.Stop(); // 루프 중인 SFX 즉시 중단
        Debug.Log("[AudioManager] 모든 SFX 정지 완료");
    }
}
