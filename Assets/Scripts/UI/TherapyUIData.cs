[System.Serializable]
public class TherapyUIData
{
    public TherapySessionState state;
    public int score;
    public float progress;
    public FootSide activeFoot;
    public float leftPercent;
    public float rightPercent;
    public string lastMessage;
    public bool lastStepSuccess;
}