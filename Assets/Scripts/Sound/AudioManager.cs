using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static AudioManager Instance { get; private set; }
    
    // 오디오 플레이어들
    private AudioSource bgmPlayer;
    private AudioSource sfxPlayer;
    
    // 사운드 데이터베이스
    [SerializeField] public SoundDatabase soundDB;
    
    // 볼륨 설정
    [Header("볼륨 설정")]
    [Range(0f, 1f)] public float bgmVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 0.7f;
    
    void Awake()
    {
        // 싱글톤 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환해도 안 사라짐
            InitializeAudio();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeAudio()
    {
        // AudioSource 2개 생성 (BGM용, SFX용)
        bgmPlayer = gameObject.AddComponent<AudioSource>();
        sfxPlayer = gameObject.AddComponent<AudioSource>();
        
        // BGM은 반복 재생
        bgmPlayer.loop = true;
        bgmPlayer.volume = bgmVolume;
        sfxPlayer.volume = sfxVolume;
    }
    
    #region BGM 제어
    
    /// <summary>
    /// BGM 재생
    /// </summary>
    public void PlayBGM(AudioClip bgm)
    {
        if (bgm == null)
        {
            Debug.LogWarning("재생할 BGM이 null입니다!");
            return;
        }
        
        // 이미 같은 음악이 재생 중이면 무시
        if (bgmPlayer.clip == bgm && bgmPlayer.isPlaying)
            return;
        
        bgmPlayer.clip = bgm;
        bgmPlayer.Play();
    }
    
    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        bgmPlayer.Stop();
    }
    
    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        bgmPlayer.Pause();
    }
    
    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        bgmPlayer.UnPause();
    }
    
    /// <summary>
    /// BGM 페이드 아웃 후 정지
    /// </summary>
    public void FadeOutBGM(float duration = 1f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }
    
    /// <summary>
    /// BGM 페이드 인하며 재생
    /// </summary>
    public void FadeInBGM(AudioClip bgm, float duration = 1f)
    {
        StartCoroutine(FadeInCoroutine(bgm, duration));
    }
    
    #endregion
    
    #region 효과음 제어
    
    /// <summary>
    /// 효과음 재생
    /// </summary>
    public void PlaySFX(AudioClip sfx)
    {
        if (sfx == null)
        {
            Debug.LogWarning("재생할 효과음이 null입니다!");
            return;
        }
        
        sfxPlayer.PlayOneShot(sfx);
    }
    
    #endregion
    
    #region 간편 재생 메서드들 (자주 쓰는 소리)
    
    // === 공용 효과음 ===
    public void PlayObjClick() => PlaySFX(soundDB.objClickSFX);
    public void PlayGetTapePiece() => PlaySFX(soundDB.getTapePieceSFX);
    public void PlayMymyWinding() => PlaySFX(soundDB.mymyWindingSFX);
    public void PlayMymyOpen() => PlaySFX(soundDB.mymyOpenSFX);
    
    // === 배경음악 ===
    public void PlayMainBGM() => PlayBGM(soundDB.mainBGM);
    public void PlayPrologBGM() => PlayBGM(soundDB.prologBGM);
    public void PlayChap1BGM() => PlayBGM(soundDB.chap1BGM);
    public void PlayChap2BGM() => PlayBGM(soundDB.chap2BGM);
    public void PlayChap3BGM() => PlayBGM(soundDB.chap3BGM);
    public void PlayChap4BGM() => PlayBGM(soundDB.chap4BGM);
    public void PlayChap5BGM() => PlayBGM(soundDB.chap5BGM);
    public void PlayChap6BGM() => PlayBGM(soundDB.chap6BGM);
    
    // === Prolog 효과음 ===
    public void PlayTrain() => PlaySFX(soundDB.trainSFX);
    public void PlayCoffee() => PlaySFX(soundDB.coffeeSFX);
    
    // === Chapter 1 효과음 ===
    public void PlayDoorOpen() => PlaySFX(soundDB.doorOpenSFX);
    public void PlayBadding() => PlaySFX(soundDB.baddingSFX);
    public void PlayWaterSquirt() => PlaySFX(soundDB.waterSquirtSFX);
    
    // === Chapter 2 효과음 ===
    public void PlayBoxOpen() => PlaySFX(soundDB.boxOpenSFX);
    public void PlayDiaryClose() => PlaySFX(soundDB.diaryCloseSFX);
    public void PlayPencilWrite() => PlaySFX(soundDB.pencilWriteSFX);
    public void PlayTapeDeck() => PlaySFX(soundDB.tapeDeckSFX);
    
    // === Chapter 3 효과음 ===
    public void PlayTyping() => PlaySFX(soundDB.typingSFX);
    
    // === Chapter 4 효과음 ===
    public void PlayFoldLaundry() => PlaySFX(soundDB.foldLaundrySFX);
    public void PlayMirrorBroken() => PlaySFX(soundDB.mirrorBrokenSFX);
    public void PlayCassetteGoingIn() => PlaySFX(soundDB.cassetteGoingInSFX);
    public void PlayMymyDoorClose() => PlaySFX(soundDB.mymyDoorCloseSFX);
    
    // === Chapter 5 효과음 ===
    public void PlayContinueTyping() => PlaySFX(soundDB.continueTypingSFX);
    public void PlayRecording() => PlaySFX(soundDB.recordingSFX);
    public void PlaySobbingGaeul() => PlaySFX(soundDB.sobbingGaeulSFX);
    
    #endregion
    
    #region 볼륨 제어
    
    /// <summary>
    /// BGM 볼륨 설정
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmPlayer.volume = bgmVolume;
    }
    
    /// <summary>
    /// 효과음 볼륨 설정
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        sfxPlayer.volume = sfxVolume;
    }
    
    #endregion
    
    #region 코루틴 (페이드 효과)
    
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
    
    #endregion
}
