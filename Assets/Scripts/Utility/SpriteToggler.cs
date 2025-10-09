using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class SpriteState
{
    public Sprite sprite;
    
    [Tooltip("체크하면 아래에 설정된 Size 값으로 크기를 변경합니다.")]
    public bool overrideSize = false;
    
    public Vector2 size = new Vector2(100, 100);
}

public class SpriteToggler : MonoBehaviour
{
    [Header("Sprite & Size Settings")]
    [Tooltip("여기에 스프라이트와 원하는 사이즈를 설정하세요.")]
    public List<SpriteState> spriteStates;

    // --- [수정된 부분 1: 마스터 스위치 추가] ---
    [Header("Final State Action")]
    [Tooltip("체크하면 마지막 상태가 되었을 때 아래 오브젝트를 활성화합니다.")]
    public bool enableActionOnLastState = false; // 이 체크박스가 마스터 스위치입니다.

    [Tooltip("마지막 상태가 되었을 때 활성화할 게임 오브젝트를 연결하세요.")]
    public GameObject objectToShowOnLastState;
    // ------------------------------------------

    private Image imageComponent;
    private RectTransform rectTransform;
    private int currentStateIndex = 0;

    void Awake()
    {
        imageComponent = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        if (spriteStates != null && spriteStates.Count > 0)
        {
            ApplyState(0);
        }

        // 마스터 스위치가 켜져 있을 때만 시작 시 오브젝트를 비활성화합니다.
        if (enableActionOnLastState && objectToShowOnLastState != null)
        {
            objectToShowOnLastState.SetActive(false);
        }
    }

    public void ToggleSpriteAndSize()
    {
        if (spriteStates == null || spriteStates.Count == 0)
        {
            Debug.LogWarning("Sprite States 리스트가 비어있습니다!");
            return;
        }

        if (currentStateIndex >= spriteStates.Count - 1)
        {
            Debug.Log("마지막 스프라이트입니다.");
            return;
        }

        currentStateIndex++;
        ApplyState(currentStateIndex);
    }

    private void ApplyState(int index)
    {
        if (index < 0 || index >= spriteStates.Count) return;

        SpriteState state = spriteStates[index];
        
        if (imageComponent != null)
        {
            imageComponent.sprite = state.sprite;
        }

        if (state.overrideSize && rectTransform != null)
        {
            rectTransform.sizeDelta = state.size;
        }

        // --- [수정된 부분 2: 마스터 스위치 확인 로직 추가] ---
        // 마스터 스위치가 켜져 있을 때만 마지막 상태 활성화 로직을 실행합니다.
        if (enableActionOnLastState && objectToShowOnLastState != null)
        {
            bool isLastState = (index == spriteStates.Count - 1);
            objectToShowOnLastState.SetActive(isLastState);
        }
        // ----------------------------------------------------
    }
}