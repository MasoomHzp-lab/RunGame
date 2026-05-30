using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("System References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RunProcessor runProcessor;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ProgressManager progressManager;

    [Header("Texts")]
    [SerializeField] private TMP_Text stateText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text activeFootText;

    [Header("Progress")]
    [SerializeField] private Slider progressSlider;

    [Header("Foot UI")]
    [SerializeField] private FootTelemetryUI footTelemetryUI;

    private void Awake()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
        }
    }

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnStateChanged += HandleStateChanged;

        if (scoreManager != null)
            scoreManager.OnScoreChanged += RefreshScore;

        if (progressManager != null)
            progressManager.OnProgressChanged += RefreshProgress;

        if (runProcessor != null)
            runProcessor.OnStepCompleted += HandleStepCompleted;
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnStateChanged -= HandleStateChanged;

        if (scoreManager != null)
            scoreManager.OnScoreChanged -= RefreshScore;

        if (progressManager != null)
            progressManager.OnProgressChanged -= RefreshProgress;

        if (runProcessor != null)
            runProcessor.OnStepCompleted -= HandleStepCompleted;
    }

    private void Start()
    {
        ResetUI();

        if (gameManager != null)
            HandleStateChanged(gameManager.CurrentState);

        if (scoreManager != null)
            RefreshScore(scoreManager.CurrentScore);

        if (progressManager != null)
            RefreshProgress(progressManager.CurrentProgress);
    }

    public void UpdateIncomingStep(FootSide side, short power, short threshold)
    {
        float percent = Mathf.Clamp01((float)power / 100f);

        if (side == FootSide.Left)
            footTelemetryUI?.SetLeftPercent(percent);
        else
            footTelemetryUI?.SetRightPercent(percent);

        footTelemetryUI?.SetActiveFoot(side);
        if (activeFootText != null)
            activeFootText.text = side == FootSide.Left ? "Left Foot" : "Right Foot";
    }

    public void HandleStepCompleted(StepResult result)
    {
        if (result == null) return;

        if (messageText != null)
            messageText.text = result.message;
    }

    public void RefreshScore(int score)
    {
        if (scoreText != null)
            scoreText.text = score.ToString();
    }

    public void RefreshProgress(float progress)
    {
        if (progressSlider != null)
            progressSlider.value = progress;
    }

    public void HandleStateChanged(TherapySessionState state)
    {
        if (stateText != null)
            stateText.text = state.ToString();

        if (state == TherapySessionState.Idle ||
            state == TherapySessionState.Finished ||
            state == TherapySessionState.EmergencyStop)
        {
            footTelemetryUI?.ResetHighlights();

            if (activeFootText != null)
                activeFootText.text = string.Empty;
        }
    }

    public void ResetUI()
    {
        if (progressSlider != null)
            progressSlider.value = 0f;

        if (scoreText != null)
            scoreText.text = "0";

        if (messageText != null)
            messageText.text = string.Empty;

        if (activeFootText != null)
            activeFootText.text = string.Empty;

        if (footTelemetryUI != null)
        {
            footTelemetryUI.SetLeftPercent(0f);
            footTelemetryUI.SetRightPercent(0f);
            footTelemetryUI.ResetHighlights();
        }
    }
}