using UnityEngine;

public class StepAlternationManager : MonoBehaviour
{
    [SerializeField] private FootSide nextExpectedFoot = FootSide.Left;

    private void Awake()
    {
        if (nextExpectedFoot == FootSide.None)
            ResetSequence();
    }

    public FootSide GetNextExpectedFoot()
    {
        if (nextExpectedFoot == FootSide.None)
            ResetSequence();

        return nextExpectedFoot;
    }

    public void ConfirmStep(FootSide performedFoot)
    {
        if (performedFoot == FootSide.Left)
            nextExpectedFoot = FootSide.Right;
        else if (performedFoot == FootSide.Right)
            nextExpectedFoot = FootSide.Left;
        else
            ResetSequence();
    }

    public void ResetSequence()
    {
        nextExpectedFoot = FootSide.Left;
    }
} 