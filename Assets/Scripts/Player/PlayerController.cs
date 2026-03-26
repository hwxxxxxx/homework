using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float rotationSmoothTime = 0.1f;

    [Header("Jump & Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.2f;

    private CharacterController characterController;
    private float verticalVelocity;
    private float rotationSmoothVelocity;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleGravityAndJump();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = gameInput.GetMoveInput();
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        if (inputDirection.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg
                                + Camera.main.transform.eulerAngles.y;

            float smoothAngle = Mathf.SmoothDampAngle(
                transform.eulerAngles.y,
                targetAngle,
                ref rotationSmoothVelocity,
                rotationSmoothTime
            );

            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            float currentSpeed = gameInput.IsRunPressed() ? runSpeed : moveSpeed;
            characterController.Move(moveDirection.normalized * currentSpeed * Time.deltaTime);
        }
    }

    private void HandleGravityAndJump()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (gameInput.IsJumpPressed() && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        characterController.Move(new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
    }
}