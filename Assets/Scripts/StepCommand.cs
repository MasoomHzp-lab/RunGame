[System.Serializable]
public class StepCommand
{
    public int timeMs;
    public float durationSeconds;
    public short footPower;
    public short powerThreshold;
    public FootSide footSide;
    public bool meetsThreshold;

    public StepCommand(int timeMs, short footPower, short powerThreshold, FootSide footSide)
    {
        this.timeMs = timeMs;
        this.durationSeconds = timeMs / 1000f;
        this.footPower = footPower;
        this.powerThreshold = powerThreshold;
        this.footSide = footSide;
        this.meetsThreshold = footPower >= powerThreshold;
    }
}