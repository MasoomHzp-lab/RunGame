using System.Collections;
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

    private Coroutine currentEffectRoutine;
    private bool isShowingImportantMessage = false;

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
            if (currentEffectRoutine != null)
                StopCoroutine(currentEffectRoutine);
                
            currentEffectRoutine = StartCoroutine(TargetReachedEffectRoutine());
        }
    }

    private IEnumerator TargetReachedEffectRoutine()
    {
        isShowingImportantMessage = true;
        messageText.text = "Hoooray! The target is reached!";
        messageText.color = new Color(1f, 0.84f, 0f); // Gold color
        
        Vector3 originalScale = Vector3.one;
        
        // Pulse effect
        for (int i = 0; i < 4; i++)
        {
            float elapsed = 0f;
            float duration = 0.4f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(1f, 1.6f, Mathf.Sin(t * Mathf.PI));
                messageText.transform.localScale = originalScale * scale;
                yield return null;
            }
        }
        
        messageText.transform.localScale = originalScale;
        
        // Wait a bit
        yield return new WaitForSeconds(2f);
        
        // Fade out effect
        float fadeElapsed = 0f;
        float fadeDuration = 1f;
        Color startColor = messageText.color;
        
        while (fadeElapsed < fadeDuration)
        {
            fadeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, fadeElapsed / fadeDuration);
            messageText.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        
        // Clear message if it hasn't been changed by something else
        if (messageText.text == "مأموریت انجام شد!")
        {
            messageText.text = string.Empty;
        }
        
        isShowingImportantMessage = false;
        currentEffectRoutine = null;
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
        if (isShowingImportantMessage) return;

        if (messageText != null)
        {
            if (currentEffectRoutine != null)
            {
                StopCoroutine(currentEffectRoutine);
                currentEffectRoutine = null;
            }
            
            messageText.text = message;
            messageText.color = Color.white; // Reset to default color
            messageText.transform.localScale = Vector3.one; // Reset scale
        }
    }

    public void ClearMessage()
    {
        isShowingImportantMessage = false;
        if (currentEffectRoutine != null)
        {
            StopCoroutine(currentEffectRoutine);
            currentEffectRoutine = null;
        }
        if (messageText != null)
        {
            messageText.text = string.Empty;
        }
    }
}