using UnityEngine;
using System.Runtime.InteropServices;
using System;

public class TherapyCommandAPI : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

    const int SW_HIDE = 0;
    const int SW_SHOW = 5;

    private static IntPtr _mainWindowHandle = IntPtr.Zero;

    private void Awake()
    {
        Application.runInBackground = true;
#if UNITY_STANDALONE_WIN
        if (_mainWindowHandle == IntPtr.Zero)
        {
            _mainWindowHandle = GetActiveWindow();
            if (_mainWindowHandle == IntPtr.Zero)
            {
                _mainWindowHandle = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
            }
            if (_mainWindowHandle == IntPtr.Zero)
            {
                _mainWindowHandle = FindWindow(null, Application.productName);
            }
        }
#endif
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    [Header("Step Timing")]
    [SerializeField] private bool inputTimeRepresentsFullGaitCycle = true;
    [SerializeField] [Range(0.1f, 1f)] private float singleStepDurationMultiplier = 0.5f;

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RunProcessor runProcessor;
    [SerializeField] private StepAlternationManager alternationManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ProgressManager progressManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private FeedbackManager feedbackManager;

    private static bool _autoStartNextScene = false;

    private void Start()
    {
        if (_autoStartNextScene)
        {
            _autoStartNextScene = false;
            Invoke(nameof(StartSession), 0.5f); // Wait a bit to ensure GameManager initialized
        }
    }

    public void StartSession() => gameManager?.StartSession();
    public void PauseSession() => gameManager?.PauseSession();
    public void ResumeSession() => gameManager?.ResumeSession();

    public void StopSession()
    {
        runProcessor?.StopAll();
        alternationManager?.ResetSequence();
        gameManager?.StopSession();
    }

    public void StopRun()
    {
        runProcessor?.StopAll();
        alternationManager?.ResetSequence();
    }

    public void ResetSession()
    {
        runProcessor?.StopAll();
        alternationManager?.ResetSequence();
        scoreManager?.ResetScore();
        progressManager?.ResetProgress();
        uiManager?.ResetUI();
        feedbackManager?.ClearMessage();
        gameManager?.ResetSession();
    }

    public void EmergencyStop()
    {
        runProcessor?.StopAll();
        alternationManager?.ResetSequence();
        gameManager?.EmergencyStop();
    }

    public bool CanAcceptRun(out string reason)
    {
        if (gameManager == null)
        {
            reason = "game-manager-not-set";
            return false;
        }

        if (runProcessor == null)
        {
            reason = "run-processor-not-set";
            return false;
        }

        if (gameManager.CurrentState != TherapySessionState.Running)
        {
            reason = "session-not-running";
            return false;
        }

        reason = null;
        return true;
    }

    public bool Run(int timeMs, short footPower, short threshold, short footSide, out string message)
    {
        FootSide side = ParseFootSide(footSide);
        if (side == FootSide.None)
        {
            message = "invalid-foot-side";
            return false;
        }

        return RunInternal(timeMs, footPower, threshold, side, out message);
    }

    public bool Run(int timeMs, short footPower, short threshold, out string message)
{
    message = "foot-side-required";
    return false;
}

    public void Run(int timeMs, short footPower, short threshold, short footSide)
        => Run(timeMs, footPower, threshold, footSide, out _);

    public void Run(int timeMs, short footPower, short threshold)
        => Run(timeMs, footPower, threshold, out _);

    public void SleepMode()
    {
        // Hide window
#if UNITY_STANDALONE_WIN
        if (_mainWindowHandle != IntPtr.Zero) ShowWindow(_mainWindowHandle, SW_HIDE);
#endif

        // Render قطع
        // GPU کم‌مصرف
        Application.targetFrameRate = 5;

        // صدا قطع
        AudioListener.pause = true;
        AudioListener.volume = 0f;

        // Physics خواب
        Physics.simulationMode = SimulationMode.Script;
        Physics2D.simulationMode = SimulationMode2D.Script;

        // Animator قطع - Find all animators and disable them
        Animator[] animators = FindObjectsByType<Animator>(FindObjectsSortMode.None);
        foreach (var anim in animators)
        {
            anim.enabled = false;
        }

        // دوربین غیرفعال
        if (Camera.main != null)
        {
            Camera.main.enabled = false;
        }
    }

    public void StartFreshGame()
    {
        // Show window
#if UNITY_STANDALONE_WIN
        if (_mainWindowHandle != IntPtr.Zero) ShowWindow(_mainWindowHandle, SW_SHOW);
#endif

        _autoStartNextScene = true;
        Invoke(nameof(ReloadScene), 0.1f);
    }

    private void ReloadScene()
    {
        // بازی کامل ریست میشه
        // Scene از اول لود میشه
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );

        // تنظیمات به حالت اولیه برمی‌گردند
        Application.targetFrameRate = -1; // Default

        // صدا برمی‌گرده
        AudioListener.pause = false;
        AudioListener.volume = 1f;

        // Physics فعال
        Physics.simulationMode = SimulationMode.FixedUpdate;
        Physics2D.simulationMode = SimulationMode2D.FixedUpdate;
    }

    public TherapyStateSnapshot GetStateSnapshot()
    {
        return new TherapyStateSnapshot
        {
            ok = true,
            state = gameManager != null ? gameManager.CurrentState.ToString() : "Unknown",
            score = scoreManager != null ? scoreManager.CurrentScore : 0,
            progress = progressManager != null ? progressManager.CurrentProgress : 0f,
            queuedCommands = runProcessor != null ? runProcessor.QueuedCount : 0,
            isProcessing = runProcessor != null && runProcessor.IsProcessing,
            nextExpectedFoot = alternationManager != null ? alternationManager.GetNextExpectedFoot().ToString() : "Left"
        };
    }

    private FootSide ParseFootSide(short footSide)
{
    if (footSide == 1)
        return FootSide.Right;

    if (footSide == 2)
        return FootSide.Left;

    return FootSide.None;
}

    private bool RunInternal(int timeMs, short footPower, short threshold, FootSide side, out string message)
    {
        if (timeMs <= 0)
        {
            message = "invalid-time";
            return false;
        }

        if (threshold < 0)
        {
            message = "invalid-threshold";
            return false;
        }

        if (footPower < 0)
        {
            message = "invalid-foot-power";
            return false;
        }

        if (!CanAcceptRun(out string reason))
        {
            message = reason;
            return false;
        }

        int effectiveTimeMs = GetSingleStepDurationMs(timeMs);

        Debug.Log($"[TherapyCommandAPI] Run accepted: rawSide mapped to {side}, inputTimeMs={timeMs}, effectiveStepMs={effectiveTimeMs}, power={footPower}, threshold={threshold}");

        StepCommand command = new StepCommand(effectiveTimeMs, footPower, threshold, side);
        runProcessor.EnqueueStep(command);
        uiManager?.UpdateIncomingStep(side, footPower, threshold);

        message = "run-enqueued";
        return true;
    }

    private int GetSingleStepDurationMs(int requestedTimeMs)
    {
        if (!inputTimeRepresentsFullGaitCycle)
            return requestedTimeMs;

        float scaled = requestedTimeMs * Mathf.Max(0.01f, singleStepDurationMultiplier);
        return Mathf.Max(1, Mathf.RoundToInt(scaled));
    }
}

[System.Serializable]
public class TherapyStateSnapshot
{
    public bool ok;
    public string state;
    public int score;
    public float progress;
    public int queuedCommands;
    public bool isProcessing;
    public string nextExpectedFoot;
}