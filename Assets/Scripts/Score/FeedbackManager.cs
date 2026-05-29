using TMPro;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip failClip;
    [SerializeField] private GameObject successFxPrefab;
    [SerializeField] private GameObject failFxPrefab;
    [SerializeField] private Transform fxAnchor;
    [SerializeField] private TMP_Text messageText;

    private void Reset()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayStepFeedback(StepResult result)
    {
        if (result == null)
            return;

        if (result.success)
        {
            if (audioSource != null && successClip != null)
                audioSource.PlayOneShot(successClip);

            if (successFxPrefab != null && fxAnchor != null)
                Instantiate(successFxPrefab, fxAnchor.position, Quaternion.identity);
        }
        else
        {
            if (audioSource != null && failClip != null)
                audioSource.PlayOneShot(failClip);

            if (failFxPrefab != null && fxAnchor != null)
                Instantiate(failFxPrefab, fxAnchor.position, Quaternion.identity);
        }

        ShowMessage(result.message, result.success, false);
    }

    public void ShowMessage(string message)
    {
        SetMessage(message);
    }

    public void ShowMessage(string message, bool success, bool sticky)
    {
        SetMessage(message);
    }

    private void SetMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }

    public void ClearMessage()
    {
        SetMessage(string.Empty);
    }
}