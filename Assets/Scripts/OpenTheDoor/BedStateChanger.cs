using UnityEngine;
using UnityEngine.UI;

public class BedStateChanger : MonoBehaviour
{
    [Header("침대 상태 스프라이트")]
    public Sprite[] bedStates; // 0~3까지 순서대로 배치

    [Header("숨겨진 오브젝트 (마지막 상태에서 표시)")]
    public GameObject hiddenObject;

    private int currentStateIndex = 0;
    private Image bedImage;

    void Start()
    {
        bedImage = GetComponent<Image>();

        if (bedStates.Length == 0)
        {
            Debug.LogError("BedStateChanger: bedStates 배열이 비어 있습니다!");
            return;
        }

        // 초기 상태 설정
        bedImage.sprite = bedStates[0];

        if (hiddenObject != null)
            hiddenObject.SetActive(false);
    }

    // ✅ 침대 상태 변경 (버튼 클릭 시 호출)
    public void ChangeState()
    {
        if (currentStateIndex >= bedStates.Length - 1)
        {
            Debug.Log("침대 탐색 완료! 마지막 상태에 도달했습니다.");
            return;
        }

        currentStateIndex++;
        bedImage.sprite = bedStates[currentStateIndex];
        Debug.Log($"🛏️ 침대 상태 변경: {bedStates[currentStateIndex].name}");

        // ✅ 사운드 재생 (상태 변경마다)
        PlayBeddingSound();

        // 마지막 상태 시 숨겨진 오브젝트 활성화
        if (currentStateIndex == bedStates.Length - 1 && hiddenObject != null)
        {
            hiddenObject.SetActive(true);
            Debug.Log("📱 숨겨진 오브젝트(휴대폰) 발견!");
        }
    }

    private void PlayBeddingSound()
    {

        // Fallback: AudioManager 직접 호출
        if (AudioManager.Instance != null && AudioManager.Instance.soundDB != null)
        {
            var clip = AudioManager.Instance.soundDB.baddingSFX;
            if (clip != null)
            {
                AudioManager.Instance.PlaySFX(clip);
                return;
            }
        }

        Debug.LogWarning("⚠️ BedStateChanger: 이불 사운드를 재생할 AudioManager나 SoundDB를 찾을 수 없습니다.");
    }
}
