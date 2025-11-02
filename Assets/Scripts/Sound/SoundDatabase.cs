using UnityEngine; 

[CreateAssetMenu(fileName = "SoundDatabase", menuName = "Game/Sound Database")]
public class SoundDatabase : ScriptableObject
{
    [Header("=== 공용 효과음 ===")]
    public AudioClip objClickSFX;           // 물체 클릭 효과음
    public AudioClip getTapePieceSFX;       // 테이프 조각 획득 사운드
    
    [Header("=== 공용 마이마이 효과음 ===")]
    public AudioClip mymyWindingSFX;        // 마이마이 테이프 도는 소리
    public AudioClip mymyOpenSFX;           // 마이마이 문 여는 소리
    
    [Header("=== 배경음악 ===")]
    public AudioClip mainBGM;               // 메인 배경음
    public AudioClip prologBGM;             // 프롤로그 배경음
    public AudioClip chap1BGM;              // 챕터1 배경음
    public AudioClip chap2BGM;              // 챕터2 배경음
    public AudioClip chap3BGM;              // 챕터3 배경음
    public AudioClip chap4BGM;              // 챕터4 배경음
    public AudioClip chap5BGM;              // 챕터5 배경음
    public AudioClip chap6BGM;              // 챕터6 배경음
    public AudioClip brokenTheTuneBGM;      // 음악겜 배경음
    
    [Header("=== Prolog 효과음 ===")]
    public AudioClip trainSFX;              // 기차 효과음
    public AudioClip coffeeSFX;             // 커피 내리는 효과음
    
    [Header("=== Chapter 1 효과음 ===")]
    public AudioClip doorOpenSFX;           // 문 여는 효과음
    public AudioClip baddingSFX;            // 이불 효과음
    public AudioClip waterSquirtSFX;        // 물 뿌리는 효과음
    public AudioClip dialSFX;               // 다이얼 효과음

    [Header("=== Chapter 2 효과음 ===")]
    public AudioClip boxOpenSFX;            // 박스 여는 효과음
    public AudioClip diaryCloseSFX;         // 다이어리 닫히는 효과음
    public AudioClip pencilWriteSFX;        // 글씨 쓰는 효과음
    public AudioClip tapeDeckSFX;           // 테이프 도는 효과음
    public AudioClip tapePlaySFX;           // 테이프 재생(도는) 소리
    public AudioClip tapeZiziziSFX;         // 테이프 지지직거리는 소리
    
    [Header("=== Chapter 3 효과음 ===")]
    public AudioClip typingSFX;             // 타이핑 효과음
    
    [Header("=== Chapter 4 효과음 ===")]
    public AudioClip foldLaundrySFX;        // 옷 접는 효과음
    public AudioClip mirrorBrokenSFX;       // 거울 깨지는 효과음
    public AudioClip cassetteGoingInSFX;    // 마이마이에 테이프 끼는 소리
    public AudioClip mymyDoorCloseSFX;      // 마이마이 닫는 소리
    
    [Header("=== Chapter 5 효과음 ===")]
    public AudioClip continueTypingSFX;     // 타이핑 연쇄적으로 치는 소리
    public AudioClip recordingSFX;          // 녹음기 소리
    public AudioClip sobbingGaeulSFX;       // 가을 우는 소리
}
