using UnityEngine;
using UnityEngine.UI;

public class BedStateChanger : MonoBehaviour
{
    [Header("침대 상태 스프라이트")]
    public Sprite[] bedStates; // 0~3까지 순서대로 배치

    [Header("숨겨진 오브젝트 (마지막 상태에서 표시)")]
    public GameObject hiddenObject;

    [Header("추가 설정")]
    [Tooltip("히든 폰이 나온 뒤에는 다이얼 퍼즐이 끝나야만 침대 클릭을 다시 허용할지 여부")]
    public bool requireDialBeforeNext = true;

    private int currentStateIndex = 0;
    private Image bedImage;

    // 히든 폰이 이미 나왔는지 / 다이얼 퍼즐이 끝났는지 여부
    private bool hiddenShown = false;
    private bool dialCompleted = false;

    void Start()
    {
        bedImage = GetComponent<Image>();

        if (bedStates == null || bedStates.Length == 0)
        {
            Debug.LogError("BedStateChanger: bedStates 배열이 비어 있습니다!");
            return;
        }

        // 초기 상태 설정
        currentStateIndex = 0;
        bedImage.sprite = bedStates[0];

        if (hiddenObject != null)
            hiddenObject.SetActive(false);
    }

    // ✅ 침대 상태 변경 (버튼 클릭 시 호출)
    public void ChangeState()
    {
        // ▽ 히든 폰이 이미 보이는 상태 + 다이얼이 아직 안 끝났으면 클릭 무시
        if (requireDialBeforeNext && hiddenShown && !dialCompleted)
        {
            Debug.Log("[BedStateChanger] Dial not completed yet. Ignore bed click.");
            return;
        }

        if (bedStates == null || bedStates.Length == 0 || bedImage == null)
            return;

        // 더 이상 진행할 상태가 없으면 종료
        if (currentStateIndex >= bedStates.Length - 1)
        {
            Debug.Log("침대 탐색 완료! 마지막 상태에 도달했습니다.");
            return;
        }

        currentStateIndex++;
        bedImage.sprite = bedStates[currentStateIndex];
        Debug.Log($"🛏️ 침대 상태 변경: {bedStates[currentStateIndex].name}");

        // ✅ 상태 변경마다 이불 사운드 재생
        PlayBeddingSound();

        // 마지막 상태 시 숨겨진 오브젝트 활성화
        if (currentStateIndex == bedStates.Length - 1 && hiddenObject != null)
        {
            hiddenObject.SetActive(true);
            hiddenShown = true;   // ▶ 폰이 한 번 등장했다고 표시
            Debug.Log("📱 숨겨진 오브젝트(휴대폰) 발견!");
        }
    }

    /// <summary>
    /// 다이얼 퍼즐이 완료됐을 때 외부(다이얼 스크립트)에서 호출해 줄 함수
    /// </summary>
    public void OnDialCompleted()
    {
        dialCompleted = true;
        Debug.Log("[BedStateChanger] Dial completed. Bed click unlocked.");
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
