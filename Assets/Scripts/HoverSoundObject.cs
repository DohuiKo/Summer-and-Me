using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HoverSoundObject : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;     
    public AudioClip hoverClip;
    [Range(0f,1f)] public float volume = 1f;

    private bool hoveringPlayed = false;  // 같은 hover 구간 중복 방지
    private bool clicked = false;         // 클릭 여부

    void Awake()
    {
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
    }

    void OnMouseEnter()
    {
        if (clicked) return;  // 클릭 후에는 무조건 차단

        if (!hoveringPlayed && hoverClip && audioSource)
        {
            audioSource.PlayOneShot(hoverClip, volume);
            hoveringPlayed = true;
        }
    }

    void OnMouseExit()
    {
        if (clicked) return;  // 클릭 후엔 리셋 불필요
        hoveringPlayed = false;
    }

    void OnMouseDown()
    {
        // 이 오브젝트를 한 번이라도 클릭하면 영구적으로 hover 사운드 금지
        clicked = true;
    }
}
