using UnityEngine;

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Game/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    [Header("=== 공용 효과음 ===")]
    public AudioClip objClickSFX;
    public AudioClip getTapePieceSFX;

    [Header("=== 공용 마이마이 효과음 ===")]
    public AudioClip mymyWindingSFX;
    public AudioClip mymyOpenSFX;

    [Header("=== 배경음악 ===")]
    public AudioClip mainBGM;
    public AudioClip prologBGM;
    public AudioClip chap1BGM;
    public AudioClip chap2BGM;
    public AudioClip chap3BGM;
    public AudioClip chap4BGM;
    public AudioClip chap5BGM;
    public AudioClip chap6BGM;
    public AudioClip brokenTheTuneBGM;

    [Header("=== Prolog 효과음 ===")]
    public AudioClip trainSFX;
    public AudioClip coffeeSFX;

    [Header("=== Chapter 1 효과음 ===")]
    public AudioClip doorOpenSFX;
    public AudioClip baddingSFX;
    public AudioClip waterSquirtSFX;
    public AudioClip dialSFX;

    [Header("=== Chapter 2 효과음 ===")]
    public AudioClip boxOpenSFX;
    public AudioClip diaryCloseSFX;
    public AudioClip pencilWriteSFX;
    public AudioClip tapeDeckSFX;
    public AudioClip tapePlaySFX;
    public AudioClip tapeZiziziSFX;

    [Header("=== Chapter 3 효과음 ===")]
    public AudioClip typingSFX;

    [Header("=== Chapter 4 효과음 ===")]
    public AudioClip alarmPipipipiSFX;
    public AudioClip foldLaundrySFX;
    public AudioClip mirrorBrokenSFX;
    public AudioClip cassetteGoingInSFX;
    public AudioClip mymyDoorCloseSFX;

    [Header("=== Chapter 4 실패 효과음 ===")]
    public AudioClip laundryFailSFX;   // ★ 여기 추가!

    [Header("=== Chapter 5 효과음 ===")]
    public AudioClip continueTypingSFX;
    public AudioClip recordingSFX;
    public AudioClip sobbingGaeulSFX;
}
