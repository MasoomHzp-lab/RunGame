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
    [SerializeField] private ScoreManager scoreManager;

    private void Reset()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>(true);
    }

    private void OnEnable()
    {
        if (scoreManager != null)
            scoreManager.OnTargetReached += HandleTargetReached;
    }

    private void OnDisable()
    {
        if (scoreManager != null)
            scoreManager.OnTargetReached -= HandleTargetReached;
    }

    private void HandleTargetReached()
    {
        if (audioSource != null && successClip != null)
        {
            // Play sound slightly louder or multiple times for effect
            audioSource.PlayOneShot(successClip, 1.5f);
        }

        if (successFxPrefab != null && fxAnchor != null)
        {
            // Spawn multiple particles to create a big effect
            Instantiate(successFxPrefab, fxAnchor.position + Vector3.up * 1f, Quaternion.identity);
            Instantiate(successFxPrefab, fxAnchor.position + Vector3.left * 1f, Quaternion.identity);
            Instantiate(successFxPrefab, fxAnchor.position + Vector3.right * 1f, Quaternion.identity);
        }

        if (messageText != null)
        {
            messageText.text = "🎯 TARGET REACHED! 🎯";
            messageText.color = Color.yellow;
            // Optionally reset color later or keep it
        }
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
        {
            messageText.text = message;
            messageText.color = Color.white; // Reset to default color
        }
    }

    public void ClearMessage()
    {
        SetMessage(string.Empty);
    }
}