using System;
using System.Collections.Generic;
using UnityEngine;

public class RunProcessor : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GroundLooper groundLooper;
    [SerializeField] private CollectibleSpawner collectibleSpawner;

    [Header("Collectible Spawn Timing")]
    [SerializeField] private int stepsPerCollectible = 2;
    private int collectibleStepCounter;

    [Header("Managers")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ProgressManager progressManager;
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private StepAlternationManager alternationManager;

    private readonly Queue<StepCommand> commandQueue = new Queue<StepCommand>();

    private bool isProcessing;
    private float activeTimer;
    private StepCommand activeCommand;

    public event Action<StepCommand> OnStepStarted;
    public event Action<StepResult> OnStepCompleted;

    public int QueuedCount => commandQueue.Count;
    public bool IsProcessing => isProcessing;

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnStateChanged -= HandleStateChanged;
    }

    public void EnqueueStep(StepCommand command)
    {
        if (command == null)
            return;

        commandQueue.Enqueue(command);
        TryStartNext();
    }

    public void StopAll()
    {
        commandQueue.Clear();

        isProcessing = false;
        activeTimer = 0f;
        activeCommand = null;

        collectibleStepCounter = 0;

        if (playerController != null)
            playerController.StopNow();

        if (groundLooper != null)
            groundLooper.SetPlayerSpeed(0f);
    }

    private void Update()
    {
        if (gameManager == null ||
            gameManager.CurrentState != TherapySessionState.Running)
            return;

        if (!isProcessing || activeCommand == null)
        {
            TryStartNext();
            return;
        }

        activeTimer -= Time.deltaTime;

        if (activeTimer <= 0f)
            CompleteActiveStep();
    }

    private void TryStartNext()
    {
        if (gameManager == null ||
            gameManager.CurrentState != TherapySessionState.Running)
            return;

        if (isProcessing || commandQueue.Count == 0)
            return;

        activeCommand = commandQueue.Dequeue();

        activeTimer = Mathf.Max(
            0.01f,
            activeCommand.durationSeconds);

        isProcessing = true;

        collectibleStepCounter++;

        if (collectibleStepCounter >= stepsPerCollectible)
        {
            collectibleStepCounter = 0;
            collectibleSpawner?.SpawnOne();
        }

        bool celebrationJump =
            ShouldTriggerCelebrationJump(activeCommand);

        if (playerController != null)
        {
            playerController.MoveForStep(
                activeCommand.footSide,
                activeCommand.durationSeconds,
                celebrationJump);
        }

        if (groundLooper != null)
        {
            float moveSpeed =
                playerController != null
                    ? playerController.GetMoveSpeed()
                    : 0f;

            groundLooper.SetPlayerSpeed(moveSpeed);
        }

        OnStepStarted?.Invoke(activeCommand);
    }

    private void CompleteActiveStep()
    {
        if (activeCommand == null)
        {
            isProcessing = false;
            activeTimer = 0f;
            return;
        }

        StepResult result = BuildResult(activeCommand);

        scoreManager?.AddScore(result.scoreDelta);

        progressManager?.AddProgress(result.progressDelta);

        feedbackManager?.PlayStepFeedback(result);

        alternationManager?.ConfirmStep(activeCommand.footSide);

        OnStepCompleted?.Invoke(result);

        activeCommand = null;

        isProcessing = false;
        activeTimer = 0f;

        TryStartNext();
    }

    private StepResult BuildResult(StepCommand command)
{
    bool success =
        command.footPower >= command.powerThreshold;

    return new StepResult
    {
        footSide = command.footSide,

        footPower = command.footPower,

        threshold = command.powerThreshold,

        success = success,

        // NO SCORE FOR STEPS
        scoreDelta = 0,

        progressDelta = success ? 0.02f : 0f,

        triggeredCelebrationJump = false,

        message = success
            ? "Good Step!"
            : "Try Again!"
    };
}
    private bool ShouldTriggerCelebrationJump(StepCommand command)
    {
        // Disabled for now
        return false;
    }

    private void HandleStateChanged(TherapySessionState state)
    {
        if (state == TherapySessionState.Idle ||
            state == TherapySessionState.Finished ||
            state == TherapySessionState.EmergencyStop)
        {
            StopAll();
        }

        if (state == TherapySessionState.Running)
        {
            TryStartNext();
        }
    }
}