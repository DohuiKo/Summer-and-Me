using UnityEngine;
using System.Collections.Generic; // List<T>를 사용하기 위해 추가

/// <summary>
/// 범용적인 모달(팝업) 창 관리를 위한 스크립트.
/// 여러 씬과 다양한 UI 요소에서 재사용할 수 있도록 설계되었습니다.
/// </summary>
public class ModalManager : MonoBehaviour
{
    // Inspector에서 연결할 모달 패널 루트 오브젝트
    [Tooltip("화면에 표시하거나 숨길 모달 패널의 루트 GameObject를 연결하세요.")]
    public GameObject modalPanel;

    // 모달이 열릴 때 숨기거나, 닫힐 때 다시 표시할 오브젝트들의 리스트 (배열 대신 리스트 사용을 권장)
    [Tooltip("모달이 열릴 때 비활성화하고, 닫힐 때 활성화할 GameObject들을 연결하세요.")]
    public List<GameObject> objectsToToggle;

    /// <summary>
    /// 모달 패널을 활성화하고, 함께 숨겨야 할 오브젝트들을 비활성화합니다.
    /// 이 함수는 UI 버튼의 OnClick 이벤트에 연결하여 사용합니다.
    /// </summary>
    public void OpenModal()
    {
        if (modalPanel != null)
        {
            // 모달 패널 활성화
            modalPanel.SetActive(true);
            Debug.Log($"ModalManager: '{modalPanel.name}' 모달창 열림.");

            // 함께 숨길 오브젝트들 비활성화
            ToggleObjects(false);
        }
        else
        {
            Debug.LogError("ModalManager: modalPanel 오브젝트가 할당되지 않았습니다!");
        }
    }

    /// <summary>
    /// 모달 패널을 비활성화하고, 숨겨져 있던 오브젝트들을 다시 활성화합니다.
    /// 이 함수는 모달 내 '닫기' 버튼의 OnClick 이벤트에 연결하여 사용합니다.
    /// </summary>
    public void CloseModal()
    {
        if (modalPanel != null)
        {
            // 모달 패널 비활성화
            modalPanel.SetActive(false);
            Debug.Log($"ModalManager: '{modalPanel.name}' 모달창 닫힘.");

            // 숨겼던 오브젝트들 다시 활성화
            ToggleObjects(true);
        }
        // else는 OpenModal에서 처리했으므로 생략 가능
    }

    /// <summary>
    /// objectsToToggle 리스트의 모든 오브젝트의 활성화 상태를 변경합니다.
    /// </summary>
    /// <param name="state">설정할 활성화 상태 (true: 활성화, false: 비활성화)</param>
    private void ToggleObjects(bool state)
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(state);
            }
            else
            {
                // 리스트에 null 값이 있을 경우를 대비
                Debug.LogWarning("ModalManager: objectsToToggle 리스트에 할당되지 않은(null) 요소가 있습니다.");
            }
        }
    }
}