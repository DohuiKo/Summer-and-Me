using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UIBob : MonoBehaviour
{
    public float amplitude = 10f;     // 위아래 이동 픽셀
    public float frequency = 2f;      // 초당 왕복 횟수(Hz)
    public bool playOnEnable = true;  // 활성화되면 자동 시작
    public bool useUnscaledTime = true;

    RectTransform rt;
    Vector2 basePos;
    float t0;

    void Awake()
    {
        rt = transform as RectTransform;
        basePos = rt.anchoredPosition;
    }

    void OnEnable()
    {
        basePos = rt.anchoredPosition;
        t0 = useUnscaledTime ? Time.unscaledTime : Time.time;
        if (!playOnEnable) enabled = false;  // 수동 시작 모드
    }

    void Update()
    {
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) - t0;
        float y = Mathf.Sin(t * Mathf.PI * 2f * frequency) * amplitude;
        rt.anchoredPosition = basePos + Vector2.up * y;
    }

    // 외부에서 제어용
    public void StartBob() { basePos = rt.anchoredPosition; t0 = useUnscaledTime ? Time.unscaledTime : Time.time; enabled = true; }
    public void StopBob()  { enabled = false; rt.anchoredPosition = basePos; }
}
