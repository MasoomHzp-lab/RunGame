using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMotor motor;
    [SerializeField] private PlayerAnimationController animationController;

    private void Reset()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (animationController == null)
            animationController = GetComponent<PlayerAnimationController>();

        if (animationController == null)
            animationController = GetComponentInChildren<PlayerAnimationController>(true);
    }

    private void Awake()
    {
        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (animationController == null)
            animationController = GetComponent<PlayerAnimationController>();

        if (animationController == null)
            animationController = GetComponentInChildren<PlayerAnimationController>(true);

        if (motor == null)
            Debug.LogError("PlayerController: PlayerMotor not found.", this);

        if (animationController == null)
            Debug.LogWarning("PlayerController: PlayerAnimationController not found.", this);
    }

    public void MoveForStep(FootSide footSide, float durationSeconds, bool celebrationJump)
    {
        if (motor == null)
            return;

        motor.RegisterStepPulse(footSide);
        motor.MoveForDuration(durationSeconds);

        if (animationController != null)
            animationController.TriggerStep(footSide, durationSeconds);

        if (celebrationJump)
        {
            motor.PlayCelebrationJump();

            if (animationController != null)
                animationController.TriggerJump();
        }
    }

    public void StopNow()
    {
        motor?.StopImmediately();
        animationController?.ForceIdle();
    }

    public void StopImmediately()
    {
        StopNow();
    }

    public bool IsMoving()
    {
        return motor != null && motor.IsMoving;
    }

    public float GetMoveSpeed()
    {
        return motor != null ? motor.ForwardSpeed : 0f;
    }
}