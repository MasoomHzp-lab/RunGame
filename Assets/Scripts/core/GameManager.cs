using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GroundLooper groundLooper;
    [SerializeField] private RunProcessor runProcessor;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ProgressManager progressManager;
    [SerializeField] private FeedbackManager feedbackManager;

    public TherapySessionState CurrentState { get; private set; } = TherapySessionState.Idle;

    public event Action<TherapySessionState> OnStateChanged;

    private void Start()
    {
        InitializeSession();
    }

    public void InitializeSession()
    {
        Time.timeScale = 1f;
        CurrentState = TherapySessionState.Idle;

        if (runProcessor != null)
            runProcessor.StopAll();

        if (groundLooper != null)
            groundLooper.SetRunning(false);

        if (scoreManager != null)
            scoreManager.ResetScore();

        if (progressManager != null)
            progressManager.ResetProgress();

        if (feedbackManager != null)
            feedbackManager.ClearMessage();

        OnStateChanged?.Invoke(CurrentState);
    }

    public void StartSession()
    {
        if (CurrentState == TherapySessionState.Running)
            return;

        if (CurrentState == TherapySessionState.Finished ||
            CurrentState == TherapySessionState.EmergencyStop)
        {
            InitializeSession();
        }

        Time.timeScale = 1f;
        SetState(TherapySessionState.Running);

        if (groundLooper != null)
            groundLooper.SetRunning(true);
    }

    public void PauseSession()
    {
        if (CurrentState != TherapySessionState.Running)
            return;

        Time.timeScale = 0f;
        SetState(TherapySessionState.Paused);
    }

    public void ResumeSession()
    {
        if (CurrentState != TherapySessionState.Paused)
            return;

        Time.timeScale = 1f;
        SetState(TherapySessionState.Running);
    }

    public void StopSession()
    {
        Time.timeScale = 1f;

        if (runProcessor != null)
            runProcessor.StopAll();

        if (groundLooper != null)
            groundLooper.SetRunning(false);

        SetState(TherapySessionState.Finished);
    }

    public void ResetSession()
    {
        InitializeSession();
    }

    public void EmergencyStop()
    {
        Time.timeScale = 1f;

        if (runProcessor != null)
            runProcessor.StopAll();

        if (groundLooper != null)
            groundLooper.SetRunning(false);

        SetState(TherapySessionState.EmergencyStop);
    }

    private void SetState(TherapySessionState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }
}