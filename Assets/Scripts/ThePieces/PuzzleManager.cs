using UnityEngine;
using System.Collections;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager instance;

    [Header("퍼즐 기본 설정")]
    public int totalPieces = 4;

    [Header("UI 연결")]
    public CanvasGroup puzzlePiecesCanvasGroup;
    public CanvasGroup completedImageCanvasGroup;
    public GameObject nextPageArrow;

    [Header("텍스트 연출")]
    public FloatText[] floatingTexts; // 💬 떠다니는 텍스트들 (자동 인식 지원)
    public float textFadeDelay = 0.3f;

    private int piecesInPlace = 0;
    private bool firstPieceTriggered = false;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    [System.Obsolete]
    void Start()
    {
        // ✅ 자동으로 FloatText 찾아 등록 (Inspector 비어있을 경우)
        if (floatingTexts == null || floatingTexts.Length == 0)
        {
            floatingTexts = FindObjectsOfType<FloatText>(true); // 비활성 포함 검색
            Debug.Log($"🧩 자동으로 {floatingTexts.Length}개의 FloatText를 찾았습니다.");
        }

        // ✅ 시작 시 모든 텍스트 비활성화
        foreach (var t in floatingTexts)
        {
            if (t != null)
                t.gameObject.SetActive(false);
        }

        // ✅ 완성 이미지 숨기기
        if (completedImageCanvasGroup != null)
        {
            completedImageCanvasGroup.alpha = 0;
            completedImageCanvasGroup.gameObject.SetActive(false);
        }

        // ✅ 다음 화살표 숨기기
        if (nextPageArrow != null)
            nextPageArrow.SetActive(false);
    }

    // 🧩 퍼즐 조각이 맞춰질 때마다 호출됨
    public void PiecePlaced()
    {
        piecesInPlace++;
        Debug.Log($"🧩 현재 조각 수: {piecesInPlace}/{totalPieces}");

        // ✅ 첫 조각 맞췄을 때 한 번만 실행
        if (!firstPieceTriggered && piecesInPlace == 1)
        {
            firstPieceTriggered = true;

            // ✏️ 사운드 재생
            if (Chap2SoundManager.Instance != null)
            {
                Chap2SoundManager.Instance.PlayPencilWrite();
                Debug.Log("✏️ 펜 소리 재생됨");
            }

            // 💬 텍스트 활성화 및 부유 연출 시작
            StartCoroutine(ActivateFloatingTexts());
        }

        // ✅ 모든 조각 맞췄을 때
        if (piecesInPlace >= totalPieces)
            StartCoroutine(PuzzleCompleteSequence());
    }

    // 💬 텍스트 순차 활성화 + 페이드인
    private IEnumerator ActivateFloatingTexts()
    {
        if (floatingTexts == null || floatingTexts.Length == 0)
        {
            Debug.LogWarning("⚠️ 활성화할 FloatText가 없습니다. 연결 또는 자동 인식 확인 필요.");
            yield break;
        }

        foreach (var t in floatingTexts)
        {
            if (t == null) continue;

            // 부모 CanvasGroup 영향 방지용 처리
            CanvasGroup parentCg = t.GetComponentInParent<CanvasGroup>();
            if (parentCg != null) parentCg.ignoreParentGroups = true;

            t.gameObject.SetActive(true);
            Debug.Log($"🌬️ {t.gameObject.name} 활성화됨");

            yield return new WaitForSeconds(textFadeDelay);
            t.Activate(); // FloatText 내부에서 페이드인 + 부유 시작
        }
    }

    // 🧩 퍼즐 완성 시 페이드 전환 연출
    private IEnumerator PuzzleCompleteSequence()
    {
        Debug.Log("🎉 퍼즐 완성! 연출 시작");

        if (puzzlePiecesCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(puzzlePiecesCanvasGroup, 1f, 0f));
            puzzlePiecesCanvasGroup.gameObject.SetActive(false);
        }

        if (completedImageCanvasGroup != null)
        {
            completedImageCanvasGroup.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(completedImageCanvasGroup, 0f, 1f));
        }

        if (nextPageArrow != null)
            nextPageArrow.SetActive(true);
    }

    // 🎚️ 페이드 함수
    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float startAlpha, float endAlpha)
    {
        if (cg == null) yield break;

        float elapsedTime = 0f;
        float fadeDuration = 1f;

        while (elapsedTime < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cg.alpha = endAlpha;
    }
}
