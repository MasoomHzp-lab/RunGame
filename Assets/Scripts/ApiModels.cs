using System;

[Serializable]
public class ApiResponse
{
    public bool success;
    public string message;
    public TherapySnapshot data;
}

[Serializable]
public class TherapySnapshot
{
    public string state;
    public int score;
    public float progress;
    public float speed;
    public string activeSide;
    public string lastResult;
    public bool isRunning;
    public int queuedCommands;
}
