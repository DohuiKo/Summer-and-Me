// WordGameManager.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;          // LayoutRebuilder
using TMPro;                   // TextMeshProUGUI

[System.Serializable]
public struct SentenceData
{
    [TextArea]
    public string sentence;
    public string[] words;
}

public class WordGameManager : MonoBehaviour
{
    public static WordGameManager instance;

    [Header("데이터")]
    public List<SentenceData> sentences;

    [Header("프리팹 & 컨테이너")]
    public GameObject wordPrefab;
    public RectTransform wordContainer;   // 상단 랜덤 배치 영역
    public RectTransform targetContainer; // 하단 목표 영역

    [Header("UI")]
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI messageText;
    public GameObject nextSceneButton;    // 모든 게임 클리어 시 활성화될 버튼

    [Header("배치 옵션")]
    public bool preventOverlap = true;
    public float padding = 8f;
    public int maxPlaceTriesPerWord = 80;

    private int currentPhase = 0;
    private int wrongAttempts = 0;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        if (nextSceneButton != null)
            nextSceneButton.SetActive(false);

        InitializePhase();
    }

    void InitializePhase()
    {
        // 기존 단어 삭제
        foreach (Transform child in wordContainer) Destroy(child.gameObject);
        foreach (Transform child in targetContainer) Destroy(child.gameObject);

        messageText.text = "한여름의 일기장";
        wrongAttempts = 0;
        phaseText.text = $"페이지: {currentPhase + 1} / {sentences.Count}";

        // 단어 섞기
        List<string> shuffledWords = sentences[currentPhase].words.OrderBy(x => Random.value).ToList();
        var placedRects = new List<Rect>();

        foreach (string word in shuffledWords)
        {
            GameObject wordObj = Instantiate(wordPrefab, wordContainer, false);
            var label = wordObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label) label.text = word;

            var wordRect = wordObj.GetComponent<RectTransform>();
            wordRect.anchorMin = wordRect.anchorMax = new Vector2(0.5f, 0.5f);
            wordRect.pivot = new Vector2(0.5f, 0.5f);

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(wordRect);

            Vector2 chosenPos = Vector2.zero;
            Bounds2D bounds = GetClampBounds(wordContainer, wordRect, padding);

            for (int tries = 0; tries < (preventOverlap ? maxPlaceTriesPerWord : 1); tries++)
            {
                float rx = Random.Range(bounds.min.x, bounds.max.x);
                float ry = Random.Range(bounds.min.y, bounds.max.y);
                chosenPos = new Vector2(rx, ry);

                if (!preventOverlap)
                {
                    break;
                }

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
                    placedRects.Add(candidate);
                    break;
                }
            }
            
            chosenPos = ClampAnchoredPosition(chosenPos, wordContainer, wordRect, padding);
            wordRect.anchoredPosition = chosenPos;
        }
    }

    // "저장하기" 버튼
    public void CheckAnswer()
    {
        if (targetContainer.childCount == 0)
        {
            messageText.text = "한여름의 일기장";
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

    void GoToNextPhase()
    {
        currentPhase++;
        if (currentPhase < sentences.Count)
        {
            InitializePhase();
        }
        else
        {
            messageText.text = "이제 끝일까?";
            if (nextSceneButton != null)
                nextSceneButton.SetActive(true);
        }
    }

    // 단어 클릭 시 이동 (WordDragger에서 호출)
    public void MoveWordOnClick(GameObject wordObject)
    {
        var rt = wordObject.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        if (wordObject.transform.parent == (Transform)wordContainer)
        {
            wordObject.transform.SetParent(targetContainer, worldPositionStays: false);
            rt.anchoredPosition = ClampAnchoredPosition(rt.anchoredPosition, targetContainer, rt, padding);
        }
        else
        {
            wordObject.transform.SetParent(wordContainer, worldPositionStays: false);
            rt.anchoredPosition = ClampAnchoredPosition(rt.anchoredPosition, wordContainer, rt, padding);
        }
    }
    
    // ====== 유틸 ======
    private struct Bounds2D { public Vector2 min, max; public Bounds2D(Vector2 mn, Vector2 mx){ min=mn; max=mx; } }

    private Bounds2D GetClampBounds(RectTransform container, RectTransform child, float pad)
    {
        Vector2 cSize = container.rect.size;
        Vector2 sSize = child.rect.size;
        float halfW = cSize.x * 0.5f, halfH = cSize.y * 0.5f;
        float halfWordW = sSize.x * 0.5f, halfWordH = sSize.y * 0.5f;
        float minX = -halfW + halfWordW + pad, maxX = halfW - halfWordW - pad;
        float minY = -halfH + halfWordH + pad, maxY = halfH - halfWordH - pad;
        return new Bounds2D(new Vector2(minX, minY), new Vector2(maxX, maxY));
    }

    private Vector2 ClampAnchoredPosition(Vector2 pos, RectTransform container, RectTransform child, float pad)
    {
        var b = GetClampBounds(container, child, pad);
        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);
        return pos;
    }

    private Rect GetRectAroundCenter(Vector2 center, Vector2 size, float pad)
    {
        float w = size.x + pad * 2f, h = size.y + pad * 2f;
        return new Rect(center.x - w * 0.5f, center.y - h * 0.5f, w, h);
    }
}