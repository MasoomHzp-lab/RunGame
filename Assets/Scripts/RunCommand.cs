using UnityEngine;

public class RunCommand
{
    public float durationSeconds;
    public float power;
    public float threshold;
    public TargetSide side;
    public float stepInterval;

    public float DurationSeconds => durationSeconds;

    // Kept for backward compatibility with any existing code that still reads duration.
    public float duration => durationSeconds;

    public RunCommand(float durationMs, float power, float threshold, TargetSide side, float stepIntervalSeconds = 0.2f)
    {
        durationSeconds = Mathf.Max(0f, durationMs / 1000f);
        this.power = Mathf.Max(0f, power);
        this.threshold = Mathf.Max(0f, threshold);
        this.side = side;
        stepInterval = Mathf.Max(0.01f, stepIntervalSeconds);
    }
}
