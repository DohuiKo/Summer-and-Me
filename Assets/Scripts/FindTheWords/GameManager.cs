// GameManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;          // LayoutRebuilder
using TMPro;                  // TextMeshProUGUI

[System.Serializable]
public struct SentenceData
{
    [TextArea]
    public string sentence;
    public string[] words;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("데이터")]
    public List<SentenceData> sentences;

    [Header("프리팹 & 컨테이너")]
    public GameObject wordPrefab;
    public RectTransform wordContainer;    // 상단 랜덤 배치 영역 (회색 박스)
    public RectTransform targetContainer;  // 하단 목표 영역 (회색 박스)

    [Header("UI")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI messageText;

    [Header("배치 옵션")]
    public bool preventOverlap = true;     // 서로 겹치지 않게
    public float padding = 8f;             // 컨테이너/단어 간 여유
    public int maxPlaceTriesPerWord = 80;  // 겹침 회피 재시도 수

    private int currentPhase = 0;
    private int wrongAttempts = 0;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        InitializePhase();
    }

    void InitializePhase()
    {
        // 기존 단어 삭제
        foreach (Transform child in wordContainer) Destroy(child.gameObject);
        foreach (Transform child in targetContainer) Destroy(child.gameObject);

        messageText.text = "";
        wrongAttempts = 0;
        phaseText.text = $"페이지: {currentPhase + 1} / {sentences.Count}";

        // 단어 섞기
        List<string> shuffledWords = sentences[currentPhase].words.OrderBy(x => Random.value).ToList();

        // 이미 배치된 단어들의 사각형(로컬 좌표 기준)을 보관
        var placedRects = new List<Rect>();

        foreach (string word in shuffledWords)
        {
            GameObject wordObj = Instantiate(wordPrefab, wordContainer, false);

            var label = wordObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = word;

            var wordRect = wordObj.GetComponent<RectTransform>();

            // 앵커/피벗 중앙 → 로컬(anchoredPosition) 계산이 간단해짐
            wordRect.anchorMin = wordRect.anchorMax = new Vector2(0.5f, 0.5f);
            wordRect.pivot = new Vector2(0.5f, 0.5f);

            // 텍스트 길이에 따른 사이즈 반영
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(wordRect);

            // 경계 안에서 랜덤 후보 뽑기
            Vector2 chosenPos = Vector2.zero;
            bool placed = false;

            // 컨테이너 경계(현재 단어 크기 + padding 고려) 계산
            Bounds2D bounds = GetClampBounds(wordContainer, wordRect, padding);

            for (int tries = 0; tries < (preventOverlap ? maxPlaceTriesPerWord : 1); tries++)
            {
                float rx = Random.Range(bounds.min.x, bounds.max.x);
                float ry = Random.Range(bounds.min.y, bounds.max.y);
                chosenPos = new Vector2(rx, ry);

                if (!preventOverlap)
                {
                    placed = true;
                    break;
                }

                // 후보 사각형
                Rect candidate = GetRectAroundCenter(chosenPos, wordRect.rect.size, padding);

                bool overlaps = false;
                for (int i = 0; i < placedRects.Count; i++)
                {
                    if (candidate.Overlaps(placedRects[i], true))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    placed = true;
                    placedRects.Add(candidate);
                    break;
                }
            }

            // 실패해도 마지막 후보를 경계 내로 보정해서 사용
            chosenPos = ClampAnchoredPosition(chosenPos, wordContainer, wordRect, padding);

            wordRect.anchoredPosition = chosenPos;
        }
    }

    // "저장하기" 버튼
    public void CheckAnswer()
    {
        if (targetContainer.childCount == 0)
        {
            messageText.text = "여름이가 남긴 일기장을";
            return;
        }

        string userAnswer = "";
        for (int i = 0; i < targetContainer.childCount; i++)
        {
            userAnswer += targetContainer.GetChild(i).GetComponentInChildren<TextMeshProUGUI>().text;
            if (i < targetContainer.childCount - 1) userAnswer += " ";
        }

        string correctAnswer = string.Join(" ", sentences[currentPhase].words);

        if (userAnswer == correctAnswer)
        {
            messageText.text = "올바른 문장이다.";
            Invoke(nameof(GoToNextPhase), 0.5f);
        }
        else
        {
            wrongAttempts++;
            messageText.text = $"틀렸다. 다시해보자. (시도: {wrongAttempts}/3)";

            if (wrongAttempts >= 3)
            {
                messageText.text = "읽을 수가 없다. 다음으로 넘어가자.";
                Invoke(nameof(GoToNextPhase), 1.5f);
            }
        }
    }

    // "다시 시작" 버튼
    public void ResetCurrent()
    {
        InitializePhase();
    }

    // 다음 단계
    void GoToNextPhase()
    {
        currentPhase++;
        if (currentPhase < sentences.Count)
        {
            InitializePhase();
        }
        else
        {
            messageText.text = "이제 끝이다.";
        }
    }

    // 단어 클릭 시 이동 (WordDragger에서 호출)
    public void MoveWordOnClick(GameObject wordObject)
    {
        var rt = wordObject.GetComponent<RectTransform>();

        // 앵커/피벗 중앙 고정
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        if (wordObject.transform.parent == (Transform)wordContainer)
        {
            wordObject.transform.SetParent(targetContainer, worldPositionStays: false);
            // 부모가 바뀌었으니, 현재 위치를 새 부모 경계 안으로 한번 더 보정
            rt.anchoredPosition = ClampAnchoredPosition(rt.anchoredPosition, targetContainer, rt, padding);
        }
        else
        {
            wordObject.transform.SetParent(wordContainer, worldPositionStays: false);
            rt.anchoredPosition = ClampAnchoredPosition(rt.anchoredPosition, wordContainer, rt, padding);
        }
    }

    // ====== 유틸 ======

    // 경계 정보를 반환(로컬 anchored 좌표 기준)
    private struct Bounds2D { public Vector2 min, max; public Bounds2D(Vector2 mn, Vector2 mx){ min=mn; max=mx; } }

    private Bounds2D GetClampBounds(RectTransform container, RectTransform child, float pad)
    {
        // 컨테이너/자식 크기
        Vector2 cSize = container.rect.size;
        Vector2 sSize = child.rect.size;

        float halfW = cSize.x * 0.5f;
        float halfH = cSize.y * 0.5f;

        float halfWordW = sSize.x * 0.5f;
        float halfWordH = sSize.y * 0.5f;

        float minX = -halfW + halfWordW + pad;
        float maxX =  halfW - halfWordW - pad;
        float minY = -halfH + halfWordH + pad;
        float maxY =  halfH - halfWordH - pad;

        return new Bounds2D(new Vector2(minX, minY), new Vector2(maxX, maxY));
    }

    // 주어진 위치를 컨테이너 경계 안으로 클램프
    private Vector2 ClampAnchoredPosition(Vector2 pos, RectTransform container, RectTransform child, float pad)
    {
        var b = GetClampBounds(container, child, pad);
        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);
        return pos;
    }

    // 겹침 체크용 사각형(패딩 고려)
    private Rect GetRectAroundCenter(Vector2 center, Vector2 size, float pad)
    {
        float w = size.x + pad * 2f;
        float h = size.y + pad * 2f;
        return new Rect(center.x - w * 0.5f, center.y - h * 0.5f, w, h);
    }
}
