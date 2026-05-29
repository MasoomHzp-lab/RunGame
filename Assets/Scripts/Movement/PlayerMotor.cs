using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;

    [Header("Movement")]
    [Tooltip("Stable forward speed used for every accepted step. Keep this below running thresholds.")]
    [SerializeField] private float walkSpeed = 1.6f;

    [Header("Animation Output")]
    [Tooltip("Animator Speed value while walking. Must stay below the controller's running threshold 0.72.")]
    [SerializeField] [Range(0.05f, 0.58f)] private float walkingAnimatorSpeed = 0.35f;

    [Header("Smoothing")]
    [SerializeField] private float acceleration = 3.0f;
    [SerializeField] private float deceleration = 3.5f;

    private float moveTimer;
    private bool isMoving;
    private float currentForwardSpeed;
    private float targetForwardSpeed;
    private FootSide lastFootSide = FootSide.None;

    public bool IsMoving => isMoving;
    public float ForwardSpeed => currentForwardSpeed;

    public float NormalizedLocomotion
    {
        get
        {
            if (!isMoving || currentForwardSpeed <= 0.01f)
                return 0f;

            // Keep the Animator in the walking range only.
            // In PlayerController.controller, Running starts around Speed >= 0.72.
            // Returning a fixed walking value prevents the unwanted Running state.
            return walkingAnimatorSpeed;
        }
    }

    private void Reset()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = GetComponentInChildren<CharacterController>();
    }

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (controller == null)
            controller = GetComponentInChildren<CharacterController>();

        if (controller == null)
        {
            Debug.LogError("PlayerMotor: CharacterController not found.", this);
            enabled = false;
            return;
        }

        currentForwardSpeed = 0f;
        targetForwardSpeed = 0f;
    }

    public void RegisterStepPulse(FootSide footSide)
    {
        lastFootSide = footSide;

        // Safe restore behavior:
        // every step uses the same stable walk speed.
        // No fast-walk/run escalation based on repeated clicks.
        targetForwardSpeed = walkSpeed;
    }

    public void MoveForDuration(float seconds)
    {
        moveTimer += Mathf.Max(0f, seconds);
        isMoving = moveTimer > 0f;

        if (isMoving && targetForwardSpeed <= 0f)
            targetForwardSpeed = walkSpeed;
    }

    public void StopImmediately()
    {
        moveTimer = 0f;
        isMoving = false;
        lastFootSide = FootSide.None;
        targetForwardSpeed = 0f;
        currentForwardSpeed = 0f;
    }

    public void PlayCelebrationJump()
    {
        // Kept for API compatibility. Jump animation is controlled by PlayerAnimationController.
    }

    private void Update()
    {
        if (controller == null)
            return;

        if (isMoving)
        {
            currentForwardSpeed = Mathf.MoveTowards(
                currentForwardSpeed,
                targetForwardSpeed,
                acceleration * Time.deltaTime
            );

            Vector3 motion = transform.forward * currentForwardSpeed * Time.deltaTime;
            controller.Move(motion);

            moveTimer -= Time.deltaTime;
            if (moveTimer <= 0f)
            {
                moveTimer = 0f;
                isMoving = false;
                lastFootSide = FootSide.None;
                targetForwardSpeed = 0f;
            }
        }
        else
        {
            currentForwardSpeed = Mathf.MoveTowards(
                currentForwardSpeed,
                0f,
                deceleration * Time.deltaTime
            );
        }
    }
}
