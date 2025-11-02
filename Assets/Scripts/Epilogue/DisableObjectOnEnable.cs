using UnityEngine;

public class DisableObjectOnEnable : MonoBehaviour
{
    // 💾 인스펙터에서 끌어다 놓을 '비활성화할 대상'
    public GameObject objectToDisable;

    // 💾 이 스크립트가 붙은 오브젝트(Yreum-Close)가 켜지는 순간 자동 호출됩니다.
    void OnEnable()
    {
        if (objectToDisable != null)
        {
            // '비활성화할 대상'을 끕니다.
            objectToDisable.SetActive(false);
        }
    }
}