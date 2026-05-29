using UnityEngine;
using UnityEngine.UI;

public class AudioToggleButton : MonoBehaviour
{
    [SerializeField] private Image icon; // 👈 فقط آیکن داخل Button
    [SerializeField] private Sprite unmuteSprite;
    [SerializeField] private Sprite muteSprite;
    [SerializeField] private AudioSource musicSource;

    private bool isMuted = false;

    private void Awake()
    {
        if (icon == null)
            icon = GetComponent<Image>();

        RefreshIcon();
    }

    public void ToggleAudio()
    {
        isMuted = !isMuted;

        if (musicSource != null)
            musicSource.mute = isMuted;

        RefreshIcon();
    }

    private void RefreshIcon()
    {
        icon.sprite = isMuted ? muteSprite : unmuteSprite;
    }
}