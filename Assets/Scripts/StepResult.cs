using System;

[Serializable]
public class StepResult
{
    public FootSide footSide;
    public short footPower;
    public short threshold;
    public bool success;
    public int scoreDelta;
    public float progressDelta;
    public bool triggeredCelebrationJump;
    public string message;
}