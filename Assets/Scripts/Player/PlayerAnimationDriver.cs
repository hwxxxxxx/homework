using UnityEngine;

[DisallowMultipleComponent]
public class PlayerAnimationDriver : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Animator targetAnimator;

    [Header("Animator Params")]
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string groundedParam = "IsGrounded";
    [SerializeField] private string reloadingParam = "IsReloading";
    [SerializeField] private float dampTime = 0.1f;

    private int moveXHash;
    private int moveYHash;
    private int speedHash;
    private int groundedHash;
    private int reloadingHash;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
        }

        if (targetAnimator == null)
        {
            targetAnimator = GetComponentInChildren<Animator>(true);
        }

        if (playerController == null || targetAnimator == null)
        {
            Debug.LogError(
                "PlayerAnimationDriver missing required references: " +
                $"playerController={(playerController != null)}, " +
                $"targetAnimator={(targetAnimator != null)}",
                this
            );
            enabled = false;
            return;
        }
        
        moveXHash = Animator.StringToHash(moveXParam);
        moveYHash = Animator.StringToHash(moveYParam);
        speedHash = Animator.StringToHash(speedParam);
        groundedHash = Animator.StringToHash(groundedParam);
        reloadingHash = Animator.StringToHash(reloadingParam);
        targetAnimator.applyRootMotion = false;
    }

    private void Update()
    {
        Vector3 horizontalVelocity = playerController.HorizontalVelocity;
        horizontalVelocity.y = 0f;

        float maxSpeed = Mathf.Max(0.01f, playerController.MaxGroundSpeed);
        float speed01 = Mathf.Clamp01(horizontalVelocity.magnitude / maxSpeed);

        Vector3 localVelocity = playerController.transform.InverseTransformDirection(horizontalVelocity);
        Vector2 localMove = new Vector2(localVelocity.x, localVelocity.z);
        Vector2 moveDir = localMove.sqrMagnitude > 0.0001f ? localMove.normalized : Vector2.zero;

        float moveX = 0f;
        float moveY = 0f;
        if (speed01 > 0.01f)
        {
            if (playerController.IsAiming)
            {
                moveX = moveDir.x;
                moveY = moveDir.y;
            }
            else
            {
                moveY = 1f;
            }
        }

        targetAnimator.SetFloat(moveXHash, moveX, dampTime, Time.deltaTime);
        targetAnimator.SetFloat(moveYHash, moveY, dampTime, Time.deltaTime);
        targetAnimator.SetFloat(speedHash, speed01, dampTime, Time.deltaTime);
        targetAnimator.SetBool(groundedHash, playerController.IsGrounded);

        WeaponBase currentWeapon = playerCombat != null ? playerCombat.GetCurrentWeapon() : null;
        bool isReloading = currentWeapon != null && currentWeapon.IsReloading();
        targetAnimator.SetBool(reloadingHash, isReloading);
    }
}
