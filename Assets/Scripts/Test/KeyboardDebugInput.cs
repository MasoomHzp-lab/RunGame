using UnityEngine;

public class KeyboardDebugInput : MonoBehaviour
{
    [SerializeField] private TherapyCommandAPI therapyApi;

    [Tooltip("Full right+left gait cycle time in milliseconds.")]
    [SerializeField] private int gaitCycleDurationMs = 3000;

    [SerializeField] private short testPower = 70;
    [SerializeField] private short testThreshold = 50;

    private void Reset()
    {
        if (therapyApi == null)
            therapyApi = FindFirstObjectByType<TherapyCommandAPI>();
    }

    private void Awake()
    {
        if (therapyApi == null)
            therapyApi = FindFirstObjectByType<TherapyCommandAPI>();
    }

    private void Update()
    {
        if (therapyApi == null) return;

        if (Input.GetKeyDown(KeyCode.S)) therapyApi.StartSession();
        if (Input.GetKeyDown(KeyCode.P)) therapyApi.PauseSession();
        if (Input.GetKeyDown(KeyCode.R)) therapyApi.ResumeSession();
        if (Input.GetKeyDown(KeyCode.X)) therapyApi.StopSession();
        if (Input.GetKeyDown(KeyCode.Backspace)) therapyApi.ResetSession();

        if (Input.GetKeyDown(KeyCode.Q))
            therapyApi.Run(gaitCycleDurationMs, testPower, testThreshold, (short)FootSide.Left);

        if (Input.GetKeyDown(KeyCode.E))
            therapyApi.Run(gaitCycleDurationMs, testPower, testThreshold, (short)FootSide.Right);

        if (Input.GetKeyDown(KeyCode.T))
            therapyApi.Run(gaitCycleDurationMs, testPower, testThreshold);

        if (Input.GetKeyDown(KeyCode.Space))
            therapyApi.StopRun();
    }
}