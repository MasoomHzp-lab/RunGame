using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    [Header("Coin Counter")]
    [SerializeField] private ScoreManager scoreManager;

    [Header("Pickup Audio")]
    [SerializeField] private AudioClip pickupClip;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 1f;

    [Header("Pickup Feedback")]
    [SerializeField] private GameObject pickupFxPrefab;
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private string pickupMessage = "Great! Keep going!";

    private bool collected;

    private void Awake()
    {
        if (scoreManager == null)
            scoreManager = FindObjectOfType<ScoreManager>(true);

        if (feedbackManager == null)
            feedbackManager = FindObjectOfType<FeedbackManager>(true);
    }

    private void OnEnable()
    {
        collected = false;
    }

    public void Activate()
    {
        collected = false;
        gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (collected) return;

        if (other.CompareTag("Player") || other.GetComponentInParent<PlayerController>() != null)
            Collect();
    }

    private void Collect()
    {
        collected = true;

        if (scoreManager != null)
            scoreManager.AddScore(1);

        if (pickupClip != null)
            AudioSource.PlayClipAtPoint(pickupClip, transform.position, pickupVolume);

        if (pickupFxPrefab != null)
            Instantiate(pickupFxPrefab, transform.position, Quaternion.identity);

        if (feedbackManager != null)
            feedbackManager.ShowMessage(pickupMessage);

        gameObject.SetActive(false);
    }
}