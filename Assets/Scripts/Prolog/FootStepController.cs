using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FootstepController : MonoBehaviour
{
    [Header("Steps")]
    [SerializeField] private int targetSteps = 20;   // 목표 발자국 수
    private int currentStep = 0;

    [Header("UI")]
    public GameObject nextButton; // Next 버튼 오브젝트 할당

    private bool isWalking = true;

    void Start()
    {
        if (nextButton != null)
            nextButton.SetActive(false); // 시작할 때 꺼두기
    }

    void Update()
    {
        if (!isWalking) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            currentStep++;

            // 발자국 찍는 기존 로직 호출
            PlaceFootprint();

            // 다 찍었을 경우
            if (currentStep >= targetSteps)
            {
                isWalking = false; // 더 이상 못 걷게
                if (nextButton != null)
                    nextButton.SetActive(true); // 버튼 켜기
            }
        }
    }

    void PlaceFootprint()
    {
        // 기존 발자국 생성 / 애니메이션 로직
    }

    // 버튼 클릭 시 씬 전환
    public void OnClickNext()
    {
        SceneManager.LoadScene("SummerRoom");
        // 또는 SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
