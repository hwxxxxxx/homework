using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Transform cameraRoot;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float normalRotationSmoothTime = 0.08f;
    [SerializeField] private float aimRotationSmoothTime = 0.14f;
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private float deceleration = 28f;
    [SerializeField] private float airControlMultiplier = 0.4f;

    [Header("Jump & Gravity")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpHeight = 1.3f;
    [SerializeField] private float groundedStickVelocity = -2f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    public GameInput GameInput => gameInput;
    public Transform CameraRoot => cameraRoot;
    public Vector3 HorizontalVelocity => horizontalVelocity;
    public bool IsGrounded => isGrounded;
    public bool IsAiming => playerCombat != null && playerCombat.IsAiming;
    public float MaxGroundSpeed => Mathf.Max(moveSpeed, runSpeed);

    private CharacterController characterController;
    private float verticalVelocity;
    private float rotationSmoothVelocity;
    private Vector3 horizontalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;
    private bool wasGrounded;
    private bool isRunningAudioActive;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (gameInput == null || cameraRoot == null)
        {
            Debug.LogError("PlayerController references are not fully assigned.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        UpdateGroundedState();
        HandleMovement();
        HandleGravityAndJump();
        ApplyCharacterMove();
    }

    private void HandleMovement()
    {
        bool isAiming = playerCombat != null && playerCombat.IsAiming;
        GetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight);

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        RotateCharacterByState(isAiming, cameraForward);

        Vector2 moveInput = gameInput.GetMoveInput();
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 targetHorizontalVelocity = Vector3.zero;
        if (inputDirection.magnitude >= 0.1f)
        {
            Vector3 moveDirection = (cameraRight * inputDirection.x + cameraForward * inputDirection.z).normalized;
            targetHorizontalVelocity = moveDirection * GetTargetMoveSpeed();

            Vector3 facingDirection = isAiming ? cameraForward : moveDirection;
            if (!isAiming)
            {
                RotateTowards(facingDirection, normalRotationSmoothTime);
            }
        }

        float controlMultiplier = isGrounded ? 1f : airControlMultiplier;
        float currentAcceleration = targetHorizontalVelocity.sqrMagnitude > horizontalVelocity.sqrMagnitude
            ? acceleration
            : deceleration;
        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity,
            targetHorizontalVelocity,
            currentAcceleration * controlMultiplier * Time.deltaTime
        );

        UpdateRunAudioState(moveInput);
    }

    private void RotateCharacterByState(bool isAiming, Vector3 cameraForward)
    {
        if (!isAiming)
        {
            return;
        }

        RotateTowards(cameraForward, aimRotationSmoothTime);
    }

    private void RotateTowards(Vector3 direction, float smoothTime)
    {
        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float smoothAngle = Mathf.SmoothDampAngle(
            transform.eulerAngles.y,
            targetAngle,
            ref rotationSmoothVelocity,
            smoothTime
        );

        transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);
    }

    private void HandleGravityAndJump()
    {
        if (gameInput.IsJumpPressed())
        {
            jumpBufferTimer = jumpBufferTime;
        }
        else
        {
            jumpBufferTimer -= Time.deltaTime;
        }

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            if (verticalVelocity < 0f)
            {
                verticalVelocity = groundedStickVelocity;
            }
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            isGrounded = false;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    private void ApplyCharacterMove()
    {
        Vector3 movement = horizontalVelocity;
        movement.y = verticalVelocity;
        characterController.Move(movement * Time.deltaTime);
    }

    private float GetTargetMoveSpeed()
    {
        return gameInput.IsRunPressed() ? runSpeed : moveSpeed;
    }

    private void UpdateGroundedState()
    {
        wasGrounded = isGrounded;
        isGrounded = characterController.isGrounded;

        if (!wasGrounded && isGrounded && verticalVelocity < groundedStickVelocity)
        {
            verticalVelocity = groundedStickVelocity;
        }
    }

    private void GetCameraBasis(out Vector3 forward, out Vector3 right)
    {
        forward = cameraRoot.forward;
        right = cameraRoot.right;
    }

    private void UpdateRunAudioState(Vector2 moveInput)
    {
        bool isRunningNow = isGrounded && moveInput.magnitude >= 0.1f;
        if (isRunningNow == isRunningAudioActive)
        {
            return;
        }

        isRunningAudioActive = isRunningNow;
        EventBus.Publish(new PlayerRunStateChangedEvent(gameObject, transform.position, isRunningAudioActive));
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        bool controllerWasEnabled = characterController.enabled;
        if (controllerWasEnabled)
        {
            characterController.enabled = false;
        }

        transform.SetPositionAndRotation(position, rotation);

        if (controllerWasEnabled)
        {
            characterController.enabled = true;
        }

        horizontalVelocity = Vector3.zero;
        verticalVelocity = groundedStickVelocity;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;
        isGrounded = false;
        wasGrounded = false;
        isRunningAudioActive = false;
        EventBus.Publish(new PlayerRunStateChangedEvent(gameObject, transform.position, false));
    }

    private void OnDisable()
    {
        if (!isRunningAudioActive)
        {
            return;
        }

        isRunningAudioActive = false;
        EventBus.Publish(new PlayerRunStateChangedEvent(gameObject, transform.position, false));
    }
}
