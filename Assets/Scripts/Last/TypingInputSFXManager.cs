using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_InputField))]
public class TypingInputSFXManager : MonoBehaviour
{
    private TMP_InputField inputField;
    private float lastPlayTime = 0f;
    private float playInterval = 0.12f;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        inputField.onValueChanged.AddListener(OnUserInput);
    }

    void OnDestroy()
    {
        inputField.onValueChanged.RemoveListener(OnUserInput);
    }

    private void OnUserInput(string text)
    {
        if (Chap5SoundManager.Instance == null) return;

        if (Time.time - lastPlayTime > playInterval)
        {
            Chap5SoundManager.Instance.PlayTypingSFX();
            lastPlayTime = Time.time;
        }
    }
}
