using UnityEngine;
using TMPro; // TextMeshPro 관련 클래스를 사용하기 위해 필요
using System.Collections;

public class WobblyText : MonoBehaviour
{
    // 흔들림의 강도 (값이 클수록 더 많이 흔들림)
    public float wobbleMagnitude = 0.5f; 

    // 흔들림의 속도 (값이 클수록 더 빠르게 흔들림)
    public float wobbleSpeed = 10f; 

    // 흔들림의 불규칙성 (값이 클수록 더 불규칙하게 흔들림)
    public float randomMagnitude = 0.1f; 

    private TMP_Text textComponent; // TextMeshProUGUI 컴포넌트 참조

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        if (textComponent == null)
        {
            Debug.LogError("WobblyText 스크립트는 TextMeshProUGUI 컴포넌트가 필요합니다!");
            enabled = false; // 컴포넌트가 없으면 스크립트 비활성화
            return;
        }

        // 텍스트를 구성하는 정점 정보를 동적으로 수정할 수 있도록 설정
        textComponent.ForceMeshUpdate(); 
    }

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 코루틴 시작
        StartCoroutine(AnimateVertices());
    }

    void OnDisable()
    {
        // 오브젝트가 비활성화될 때 코루틴 정지
        StopAllCoroutines();
    }

    IEnumerator AnimateVertices()
    {
        // 매 프레임마다 정점 정보를 업데이트
        while (true)
        {
            textComponent.ForceMeshUpdate(); // 최신 텍스트 메쉬 정보로 업데이트
            TMP_TextInfo textInfo = textComponent.textInfo; // 텍스트 정보 가져오기

            // 모든 글자에 대해 반복
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                // 글자가 유효한지 확인 (스페이스나 개행 문자 등은 제외)
                if (!textInfo.characterInfo[i].isVisible)
                    continue;

                // 각 글자의 정점(Vertex) 인덱스 가져오기 (각 글자는 4개의 정점으로 구성된 사각형)
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                // 현재 글자의 4개 정점 위치를 가져옴
                Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;
                
                // 흔들림 효과 계산
                // 현재 시간과 글자 인덱스를 기반으로 불규칙한 흔들림 값을 만듭니다.
                float offset = (Time.time * wobbleSpeed) + (i * randomMagnitude); 
                
                // Sin 함수를 사용하여 시간에 따라 흔들리는 값 계산
                Vector3 wobbleOffset = new Vector3(
                    Mathf.Sin(offset) * wobbleMagnitude,
                    Mathf.Cos(offset * 0.8f) * wobbleMagnitude, // X, Y축 흔들림에 약간의 위상차를 줘서 더 자연스럽게
                    0
                );

                // 4개의 정점 각각에 흔들림 오프셋을 적용
                sourceVertices[vertexIndex + 0] += wobbleOffset; // 좌하단
                sourceVertices[vertexIndex + 1] += wobbleOffset; // 좌상단
                sourceVertices[vertexIndex + 2] += wobbleOffset; // 우상단
                sourceVertices[vertexIndex + 3] += wobbleOffset; // 우하단
            }

            // 수정된 정점 정보를 메쉬에 적용하여 텍스트를 업데이트
            textComponent.UpdateVertexData();
            yield return null; // 다음 프레임까지 대기
        }
    }
}