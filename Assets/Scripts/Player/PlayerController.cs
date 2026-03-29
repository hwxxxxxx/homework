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

    [Header("Debug")]
    [SerializeField] private bool enableMovementDebugLog;
    [SerializeField] private float debugLogInterval = 0.25f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float rotationSmoothVelocity;
    private Vector3 horizontalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private bool isGrounded;
    private bool wasGrounded;
    private float nextDebugLogTime;
    private Vector3 lastCameraForward;
    private Vector2 lastMoveInput;
    private bool usedCameraRootBasis;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
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
        if (gameInput == null)
        {
            return;
        }

        bool isAiming = playerCombat != null && playerCombat.IsAiming;
        if (!TryGetCameraBasis(out Vector3 cameraForward, out Vector3 cameraRight))
        {
            return;
        }
        lastCameraForward = cameraForward;

        cameraForward.y = 0f;
        cameraRight.y = 0f;
        cameraForward.Normalize();
        cameraRight.Normalize();

        RotateCharacterByState(isAiming, cameraForward);

        Vector2 moveInput = gameInput.GetMoveInput();
        lastMoveInput = moveInput;
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

        TryDebugLog(isAiming);
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
        if (gameInput != null && gameInput.IsJumpPressed())
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
        return gameInput != null && gameInput.IsRunPressed() ? runSpeed : moveSpeed;
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

    private bool TryGetCameraBasis(out Vector3 forward, out Vector3 right)
    {
        if (cameraRoot != null)
        {
            usedCameraRootBasis = true;
            forward = cameraRoot.forward;
            right = cameraRoot.right;
            return true;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            usedCameraRootBasis = false;
            forward = mainCamera.transform.forward;
            right = mainCamera.transform.right;
            return true;
        }

        usedCameraRootBasis = false;
        forward = Vector3.forward;
        right = Vector3.right;
        return false;
    }

    private void TryDebugLog(bool isAiming)
    {
        if (!enableMovementDebugLog || Time.time < nextDebugLogTime)
        {
            return;
        }

        nextDebugLogTime = Time.time + Mathf.Max(0.05f, debugLogInterval);

        float cameraForwardYaw = Mathf.Atan2(lastCameraForward.x, lastCameraForward.z) * Mathf.Rad2Deg;
        float playerYaw = transform.eulerAngles.y;
        float yawDelta = Mathf.DeltaAngle(playerYaw, cameraForwardYaw);

        Debug.Log(
            $"[PlayerMoveDebug] basis={(usedCameraRootBasis ? "CameraRoot" : "MainCamera")} aiming={isAiming} " +
            $"moveInput={lastMoveInput:F2} horizVel={horizontalVelocity.magnitude:F2} vertVel={verticalVelocity:F2} " +
            $"grounded={isGrounded} playerYaw={playerYaw:F1} camYaw={cameraForwardYaw:F1} yawDelta={yawDelta:F1}"
        );
    }
}
