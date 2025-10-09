using UnityEngine;
using System.Collections; // 코루틴을 위해 추가

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance;

    [Header("기본 설정")]
    [Tooltip("퍼즐을 완성하는 데 필요한 총 조각의 개수입니다.")]
    public int totalPieces = 4;
    
    [Header("완성 연출 설정")]
    [Tooltip("모든 퍼즐 조각들을 담고 있는 부모 오브젝트의 Canvas Group입니다.")]
    public CanvasGroup puzzlePiecesCanvasGroup;
    
    [Tooltip("퍼즐 완성 후 나타날 이미지의 Canvas Group입니다.")]
    public CanvasGroup completedImageCanvasGroup;
    
    [Tooltip("모든 연출이 끝난 후 나타날 '다음 페이지' 화살표입니다.")]
    public GameObject nextPageArrow;

    [Tooltip("페이드 효과에 걸리는 시간입니다.")]
    public float fadeDuration = 1.0f;

    private int piecesInPlace = 0;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 시작할 때 완성 이미지와 화살표는 보이지 않도록 설정
        if (completedImageCanvasGroup != null)
        {
            completedImageCanvasGroup.gameObject.SetActive(false);
            completedImageCanvasGroup.alpha = 0;
        }
        if (nextPageArrow != null)
        {
            nextPageArrow.SetActive(false);
        }
    }

    // 조각이 맞춰질 때마다 호출
    public void PiecePlaced()
    {
        piecesInPlace++;
        if (piecesInPlace >= totalPieces)
        {
            // 바로 버튼을 보여주는 대신, 완성 연출 코루틴을 시작
            StartCoroutine(PuzzleCompleteSequence());
        }
    }

    // 완성 연출을 순서대로 진행하는 메인 코루틴
    private IEnumerator PuzzleCompleteSequence()
    {
        Debug.Log("퍼즐 완성! 연출을 시작합니다.");
        
        // 1. 퍼즐 조각들을 페이드 아웃
        yield return StartCoroutine(FadeCanvasGroup(puzzlePiecesCanvasGroup, 1f, 0f));
        puzzlePiecesCanvasGroup.gameObject.SetActive(false);

        // 2. 완성 이미지를 활성화하고 페이드 인
        completedImageCanvasGroup.gameObject.SetActive(true);
        yield return StartCoroutine(FadeCanvasGroup(completedImageCanvasGroup, 0f, 1f));

        // 3. 모든 연출이 끝나면 다음 페이지 화살표를 활성화
        if (nextPageArrow != null)
        {
            nextPageArrow.SetActive(true);
        }
    }

    // Canvas Group의 Alpha 값을 조절하여 페이드 효과를 내는 코루틴
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        cg.alpha = endAlpha;
    }
}