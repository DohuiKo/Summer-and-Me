using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.
using System.Collections.Generic;

public class PasswordPanel : MonoBehaviour
{
    // 정답 문자열 (비교 시 소문자로 변환하므로 소문자로 지정)
    private const string CorrectAnswer = "summer";

    // Inspector 창에서 연결할 UI 요소들
    [Header("UI Elements")]
    public List<TextMeshProUGUI> characterDisplaySlots; // 입력된 글자가 표시될 6개의 TextMeshPro UI
    public GameObject diaryFileObject; // 정답을 맞혔을 때 활성화될 Diary.txt 오브젝트

    // 현재까지 입력된 문자열을 저장하는 변수
    private string currentInput = "";

    // 이 Panel이 활성화될 때마다 입력을 초기화합니다.
    private void OnEnable()
    {
        currentInput = "";
        UpdateDisplay();
    }

    // 매 프레임마다 키보드 입력을 감지합니다.
    void Update()
    {
        // 키보드 입력을 처리합니다.
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        // 알파벳 또는 숫자 입력 감지
        foreach (char c in Input.inputString)
        {
            if (char.IsLetterOrDigit(c) && currentInput.Length < 6)
            {
                currentInput += c;
            }
        }

        // 백스페이스 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Backspace) && currentInput.Length > 0)
        {
            currentInput = currentInput.Substring(0, currentInput.Length - 1);
        }

        // 엔터 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Return) && currentInput.Length > 0)
        {
            CheckPassword();
        }

        // UI 디스플레이를 업데이트합니다.
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < characterDisplaySlots.Count; i++)
        {
            if (i < currentInput.Length)
            {
                // 입력된 문자가 있으면 해당 슬롯에 표시
                characterDisplaySlots[i].text = currentInput[i].ToString();
            }
            else
            {
                // 입력된 문자가 없으면 빈칸으로 표시
                characterDisplaySlots[i].text = "";
            }
        }
    }

    private void CheckPassword()
    {
        // 사용자의 입력을 소문자로 변환하여 정답과 비교 (대소문자 구분 없음)
        if (currentInput.ToLower() == CorrectAnswer)
        {
            Debug.Log("비밀번호 정답!");

            // Diary.txt 파일 오브젝트를 활성화
            if (diaryFileObject != null)
            {
                diaryFileObject.SetActive(true);
            }
            
            // 비밀번호 입력창 자신은 비활성화
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("비밀번호 오류! 입력값: " + currentInput);
            // (선택) 틀렸을 때 흔들림 효과나 사운드를 추가할 수 있습니다.
            currentInput = ""; // 입력 초기화
            UpdateDisplay();
        }
    }
}