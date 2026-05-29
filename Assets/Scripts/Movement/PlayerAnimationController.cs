using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMotor motor;

    [Header("Animation Smoothing")]
    [SerializeField] private float speedDamp = 4.5f;
    [SerializeField] private float walkPlaybackSpeed = 1.0f;

    [Header("Foot Phase")]
    [Tooltip("State name inside PlayerController.controller. In your controller it is lowercase: walking")]
    [SerializeField] private string walkingStateName = "walking";

    [Tooltip("Normalized start time for Right foot. Use 0 if the current walking clip starts with right foot.")]
    [SerializeField] [Range(0f, 1f)] private float rightFootStartPhase = 0.0f;

    [Tooltip("Normalized start time for Left foot. Usually half a gait cycle after right foot.")]
    [SerializeField] [Range(0f, 1f)] private float leftFootStartPhase = 0.5f;

    [Tooltip("Animator Speed value forced when a step starts. Must stay below running threshold.")]
    [SerializeField] [Range(0.05f, 0.58f)] private float forcedWalkSpeedValue = 0.35f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private float currentSpeedValue;

    private void Reset()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (motor == null)
            motor = GetComponentInParent<PlayerMotor>();
    }

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (motor == null)
            motor = GetComponent<PlayerMotor>();

        if (motor == null)
            motor = GetComponentInParent<PlayerMotor>();

        if (animator == null)
        {
            Debug.LogError("PlayerAnimationController: Animator not found.", this);
            enabled = false;
            return;
        }

        animator.applyRootMotion = false;
    }

    private void Start()
    {
        animator.Rebind();
        animator.Update(0f);
        animator.SetFloat(SpeedHash, 0f);
        animator.speed = walkPlaybackSpeed;
    }

    private void Update()
    {
        float targetSpeed = (motor != null) ? motor.NormalizedLocomotion : 0f;

        currentSpeedValue = Mathf.MoveTowards(
            currentSpeedValue,
            targetSpeed,
            speedDamp * Time.deltaTime
        );

        animator.SetFloat(SpeedHash, currentSpeedValue);
        animator.speed = walkPlaybackSpeed;
    }

    public void TriggerStep(FootSide footSide)
    {
        if (animator == null)
            return;

        // Safe restore:
        // The Animator Controller does NOT contain LeftStep/RightStep parameters.
        // So we must not call SetTrigger("LeftStep") / SetTrigger("RightStep").
        // Instead we force the walking state to start from a different phase.
        float phase = footSide == FootSide.Left ? leftFootStartPhase : rightFootStartPhase;

        currentSpeedValue = forcedWalkSpeedValue;
        animator.SetFloat(SpeedHash, forcedWalkSpeedValue);
        animator.speed = walkPlaybackSpeed;

        if (!string.IsNullOrWhiteSpace(walkingStateName))
        {
            animator.Play(walkingStateName, 0, phase);
            animator.Update(0f);
        }
    }

    // Compatibility overload for PlayerController versions that call:
    // animationController.TriggerStep(footSide, durationSeconds);
    // Duration is controlled by PlayerMotor.MoveForDuration(durationSeconds),
    // so animation only needs the foot side.
    public void TriggerStep(FootSide footSide, float durationSeconds)
    {
        TriggerStep(footSide);
    }

    public void TriggerJump()
    {
        if (animator == null)
            return;

        // The controller has a Jump trigger, but no transition to the jump state in the current file.
        // Keeping this call is harmless; add a transition in Animator later if jump is required.
        animator.SetTrigger(JumpHash);
    }

    public void ForceIdle()
    {
        currentSpeedValue = 0f;

        if (animator != null)
        {
            animator.SetFloat(SpeedHash, 0f);
            animator.speed = walkPlaybackSpeed;
        }
    }

    public void RefreshAnimator()
    {
        animator = GetComponentInChildren<Animator>(true);

        if (animator == null)
            return;

        animator.applyRootMotion = false;
        animator.Rebind();
        animator.Update(0f);
        animator.SetFloat(SpeedHash, 0f);
    }

    public void SetAnimator(Animator newAnimator)
    {
        animator = newAnimator;

        if (animator == null)
            return;

        animator.applyRootMotion = false;
        animator.Rebind();
        animator.Update(0f);
        animator.SetFloat(SpeedHash, currentSpeedValue);
    }
}
