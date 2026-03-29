using Cinemachine;
using UnityEngine;

public class PlayerCameraAimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameInput gameInput;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private CinemachineVirtualCameraBase normalCamera;
    [SerializeField] private CinemachineVirtualCameraBase aimCamera;
    [SerializeField] private GameObject crosshair;

    [Header("Priority")]
    [SerializeField] private int activePriority = 20;
    [SerializeField] private int inactivePriority = 10;

    [Header("Aim CameraRoot Rotation")]
    [SerializeField] private float normalLookSensitivityX = 180f;
    [SerializeField] private float normalLookSensitivityY = 120f;
    [SerializeField] private float aimLookSensitivityMultiplier = 0.8f;
    [SerializeField] private float lookInputExponent = 1.35f;
    [SerializeField] private float lookInputSmoothingTime = 0.04f;
    [SerializeField] private float lookRotationDampingTime = 0.03f;
    [SerializeField] private float minPitch = -35f;
    [SerializeField] private float maxPitch = 70f;
    [SerializeField] private bool syncFreeLookYAxisWithPitch = true;
    [SerializeField] private bool overrideFreeLookInternalInput = true;
    
    [Header("Camera Recoil")]
    [SerializeField] private bool enableRecoil = true;
    [SerializeField] private float recoilPitchPerShot = 1.2f;
    [SerializeField] private float recoilYawJitterPerShot = 0.35f;
    [SerializeField] private float recoilKickInSpeed = 18f;
    [SerializeField] private float recoilRecoverSpeed = 10f;

    [Header("Camera Damping & Collision")]
    [SerializeField] private bool applyCameraDamping = true;
    [SerializeField] private float positionDamping = 0.2f;
    [SerializeField] private bool enableCameraCollision = true;
    [SerializeField] private float cameraCollisionRadius = 0.22f;
    [SerializeField] private LayerMask collisionLayers = ~0;
    [SerializeField] private string collisionIgnoreTag = "Player";
    [SerializeField] private float collisionMinDistanceFromTarget = 0.2f;
    [SerializeField] private float collisionSmoothingTime = 0.12f;
    [SerializeField] private float collisionDamping = 0.25f;
    [SerializeField] private float collisionDampingWhenOccluded = 0.35f;
    [SerializeField] private float collisionMinimumOcclusionTime = 0.04f;
    [SerializeField] private CinemachineCollider.ResolutionStrategy collisionStrategy =
        CinemachineCollider.ResolutionStrategy.PreserveCameraDistance;
    [SerializeField] private bool forceBrainLateUpdate = true;
    [SerializeField] private bool enableOutputStabilizer = false;
    [SerializeField] private float outputJumpThreshold = 0.45f;
    [SerializeField] private float outputMaxStepPerFrame = 0.22f;
    [SerializeField] private float outputRecoveryLerpSpeed = 12f;

    [Header("Debug")]
    [SerializeField] private bool enableCameraDebugLog;
    [SerializeField] private float debugLogInterval = 0.25f;
    [SerializeField] private bool drawDebugDirections = true;
    [SerializeField] private bool enableCameraJumpDebugLog = true;
    [SerializeField] private float cameraJumpDistanceThreshold = 0.45f;

    private float yaw;
    private float pitch;
    private float targetYaw;
    private float targetPitch;
    private float yawVelocity;
    private float pitchVelocity;
    private Vector2 smoothedLookInput;
    private Vector2 lookInputSmoothVelocity;
    private float recoilPitch;
    private float recoilYaw;
    private float recoilTargetPitch;
    private float recoilTargetYaw;
    private CinemachineFreeLook normalFreeLook;
    private WeaponBase currentWeapon;
    private float nextDebugLogTime;
    private Vector3 lastMainCameraPosition;
    private CinemachineCollider normalCameraCollider;
    private CinemachineCollider aimCameraCollider;

    private void OnEnable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnAimStateChanged += HandleAimStateChanged;
        }

        BindWeaponEvents();
    }

    private void Start()
    {
        normalFreeLook = normalCamera as CinemachineFreeLook;
        SetupFreeLookInputOverride();

        if (cameraRoot != null)
        {
            Vector3 euler = cameraRoot.rotation.eulerAngles;
            yaw = euler.y;
            pitch = NormalizeAngle(euler.x);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            targetYaw = yaw;
            targetPitch = pitch;
        }

        bool isAiming = playerCombat != null && playerCombat.IsAiming;
        ApplyAimState(isAiming);
        ApplyBrainUpdateMode();
        ApplyCinemachineTuning();
        UpdateCollisionOwner(isAiming);
        ApplyOutputStabilizer();

        Camera main = Camera.main;
        if (main != null)
        {
            lastMainCameraPosition = main.transform.position;
        }
    }

    private void LateUpdate()
    {
        if (gameInput == null || cameraRoot == null)
        {
            return;
        }

        bool isAiming = playerCombat != null && playerCombat.IsAiming;
        float sensitivityMultiplier = isAiming ? aimLookSensitivityMultiplier : 1f;

        Vector2 lookInput = ProcessLookInput(gameInput.GetLookInput());
        smoothedLookInput = Vector2.SmoothDamp(
            smoothedLookInput,
            lookInput,
            ref lookInputSmoothVelocity,
            lookInputSmoothingTime
        );

        targetYaw += smoothedLookInput.x * normalLookSensitivityX * sensitivityMultiplier * Time.deltaTime;
        targetPitch -= smoothedLookInput.y * normalLookSensitivityY * sensitivityMultiplier * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        UpdateRecoil();

        float desiredYaw = targetYaw + recoilYaw;
        float desiredPitch = Mathf.Clamp(targetPitch + recoilPitch, minPitch, maxPitch);
        yaw = Mathf.SmoothDampAngle(yaw, desiredYaw, ref yawVelocity, lookRotationDampingTime);
        pitch = Mathf.SmoothDampAngle(pitch, desiredPitch, ref pitchVelocity, lookRotationDampingTime);

        cameraRoot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        SyncFreeLookXAxisWithYaw();
        SyncFreeLookYAxisWithPitch();
        UpdateCollisionOwner(isAiming);
        TryDebugLog(isAiming);
        TryDebugCameraJump();

        if (drawDebugDirections)
        {
            Vector3 origin = cameraRoot.position + Vector3.up * 0.15f;
            Debug.DrawRay(origin, cameraRoot.forward * 2.5f, Color.cyan);
        }
    }

    private void OnDisable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnAimStateChanged -= HandleAimStateChanged;
        }

        UnbindWeaponEvents();
    }

    private void HandleAimStateChanged(bool isAiming)
    {
        ApplyAimState(isAiming);
    }

    private void ApplyAimState(bool isAiming)
    {
        SyncAnglesFromCameraRoot();
        SyncFreeLookXAxisWithYaw();
        SyncFreeLookYAxisWithPitch();

        if (normalCamera != null)
        {
            normalCamera.Priority = isAiming ? inactivePriority : activePriority;
        }

        if (aimCamera != null)
        {
            aimCamera.Priority = isAiming ? activePriority : inactivePriority;
        }

        if (crosshair != null)
        {
            crosshair.SetActive(isAiming);
        }

        UpdateCollisionOwner(isAiming);
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }

    private void SyncAnglesFromCameraRoot()
    {
        if (cameraRoot == null)
        {
            return;
        }

        Vector3 euler = cameraRoot.rotation.eulerAngles;
        yaw = euler.y;
        pitch = Mathf.Clamp(NormalizeAngle(euler.x), minPitch, maxPitch);
        targetYaw = yaw;
        targetPitch = pitch;
    }

    private void SyncFreeLookXAxisWithYaw()
    {
        if (normalFreeLook == null)
        {
            return;
        }

        float wrappedYaw = yaw % 360f;
        if (wrappedYaw < 0f)
        {
            wrappedYaw += 360f;
        }

        normalFreeLook.m_XAxis.Value = wrappedYaw;
    }

    private void SyncFreeLookYAxisWithPitch()
    {
        if (!syncFreeLookYAxisWithPitch || normalFreeLook == null)
        {
            return;
        }

        float normalized = Mathf.InverseLerp(minPitch, maxPitch, pitch);
        normalFreeLook.m_YAxis.Value = Mathf.Clamp01(normalized);
    }

    private void SetupFreeLookInputOverride()
    {
        if (!overrideFreeLookInternalInput || normalFreeLook == null)
        {
            return;
        }

        // Use CameraRoot as the single source of truth for yaw/pitch.
        normalFreeLook.m_XAxis.m_InputAxisName = string.Empty;
        normalFreeLook.m_YAxis.m_InputAxisName = string.Empty;
        normalFreeLook.m_XAxis.m_InputAxisValue = 0f;
        normalFreeLook.m_YAxis.m_InputAxisValue = 0f;
    }

    private Vector2 ProcessLookInput(Vector2 rawInput)
    {
        return new Vector2(ApplyInputCurve(rawInput.x), ApplyInputCurve(rawInput.y));
    }

    private float ApplyInputCurve(float value)
    {
        float sign = Mathf.Sign(value);
        float abs = Mathf.Abs(value);
        return sign * Mathf.Pow(abs, lookInputExponent);
    }

    private void BindWeaponEvents()
    {
        if (playerCombat == null)
        {
            return;
        }

        currentWeapon = playerCombat.GetCurrentWeapon();
        if (currentWeapon != null)
        {
            currentWeapon.OnFired += HandleWeaponFired;
        }
    }

    private void UnbindWeaponEvents()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnFired -= HandleWeaponFired;
            currentWeapon = null;
        }
    }

    private void HandleWeaponFired()
    {
        if (!enableRecoil)
        {
            return;
        }

        recoilTargetPitch += recoilPitchPerShot;
        recoilTargetYaw += Random.Range(-recoilYawJitterPerShot, recoilYawJitterPerShot);
    }

    private void UpdateRecoil()
    {
        if (!enableRecoil)
        {
            recoilPitch = 0f;
            recoilYaw = 0f;
            recoilTargetPitch = 0f;
            recoilTargetYaw = 0f;
            return;
        }

        recoilTargetPitch = Mathf.MoveTowards(recoilTargetPitch, 0f, recoilRecoverSpeed * Time.deltaTime);
        recoilTargetYaw = Mathf.MoveTowards(recoilTargetYaw, 0f, recoilRecoverSpeed * Time.deltaTime);

        recoilPitch = Mathf.MoveTowards(recoilPitch, recoilTargetPitch, recoilKickInSpeed * Time.deltaTime);
        recoilYaw = Mathf.MoveTowards(recoilYaw, recoilTargetYaw, recoilKickInSpeed * Time.deltaTime);
    }

    private void ApplyCinemachineTuning()
    {
        if (applyCameraDamping)
        {
            ApplyDamping();
        }

        if (enableCameraCollision)
        {
            normalCameraCollider = EnsureCollisionExtension(normalCamera);
            aimCameraCollider = EnsureCollisionExtension(aimCamera);
        }
    }

    private void ApplyDamping()
    {
        if (normalFreeLook != null)
        {
            for (int i = 0; i < 3; i++)
            {
                CinemachineVirtualCamera rig = normalFreeLook.GetRig(i);
                if (rig == null)
                {
                    continue;
                }

                CinemachineOrbitalTransposer orbital = rig.GetCinemachineComponent<CinemachineOrbitalTransposer>();
                if (orbital != null)
                {
                    orbital.m_XDamping = positionDamping;
                    orbital.m_YDamping = positionDamping;
                    orbital.m_ZDamping = positionDamping;
                }
            }
        }

        ApplyDampingToVirtualCamera(normalCamera as CinemachineVirtualCamera);
        ApplyDampingToVirtualCamera(aimCamera as CinemachineVirtualCamera);
    }

    private CinemachineCollider EnsureCollisionExtension(CinemachineVirtualCameraBase cameraBase)
    {
        if (cameraBase == null)
        {
            return null;
        }

        CinemachineCollider collider = cameraBase.GetComponent<CinemachineCollider>();
        if (collider == null)
        {
            collider = cameraBase.gameObject.AddComponent<CinemachineCollider>();
        }

        collider.m_AvoidObstacles = true;
        collider.m_CameraRadius = cameraCollisionRadius;
        collider.m_CollideAgainst = collisionLayers;
        collider.m_IgnoreTag = collisionIgnoreTag;
        collider.m_Damping = collisionDamping;
        collider.m_DampingWhenOccluded = collisionDampingWhenOccluded;
        collider.m_MinimumOcclusionTime = collisionMinimumOcclusionTime;
        collider.m_SmoothingTime = collisionSmoothingTime;
        collider.m_MinimumDistanceFromTarget = collisionMinDistanceFromTarget;
        collider.m_Strategy = collisionStrategy;
        return collider;
    }

    private void UpdateCollisionOwner(bool isAiming)
    {
        if (!enableCameraCollision)
        {
            return;
        }

        if (normalCameraCollider != null)
        {
            normalCameraCollider.enabled = !isAiming;
        }

        if (aimCameraCollider != null)
        {
            aimCameraCollider.enabled = isAiming;
        }
    }

    private void ApplyDampingToVirtualCamera(CinemachineVirtualCamera virtualCamera)
    {
        if (virtualCamera == null)
        {
            return;
        }

        Cinemachine3rdPersonFollow thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.Damping = new Vector3(positionDamping, positionDamping, positionDamping);
            return;
        }

        CinemachineTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_XDamping = positionDamping;
            transposer.m_YDamping = positionDamping;
            transposer.m_ZDamping = positionDamping;
        }
    }

    private void ApplyBrainUpdateMode()
    {
        if (!forceBrainLateUpdate)
        {
            return;
        }

        Camera main = Camera.main;
        if (main == null)
        {
            return;
        }

        CinemachineBrain brain = main.GetComponent<CinemachineBrain>();
        if (brain != null)
        {
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.LateUpdate;
        }
    }

    private void ApplyOutputStabilizer()
    {
        Camera main = Camera.main;
        if (main == null)
        {
            return;
        }

        CameraOutputStabilizer stabilizer = main.GetComponent<CameraOutputStabilizer>();
        if (stabilizer == null)
        {
            stabilizer = main.gameObject.AddComponent<CameraOutputStabilizer>();
        }

        stabilizer.Configure(
            enableOutputStabilizer,
            outputJumpThreshold,
            outputMaxStepPerFrame,
            outputRecoveryLerpSpeed,
            enableCameraJumpDebugLog
        );
    }

    private void OnValidate()
    {
        collisionSmoothingTime = Mathf.Clamp(collisionSmoothingTime, 0f, 1.5f);
        collisionDamping = Mathf.Clamp(collisionDamping, 0f, 2f);
        collisionDampingWhenOccluded = Mathf.Clamp(collisionDampingWhenOccluded, 0f, 2f);
        collisionMinimumOcclusionTime = Mathf.Clamp(collisionMinimumOcclusionTime, 0f, 0.5f);
        collisionMinDistanceFromTarget = Mathf.Max(0.01f, collisionMinDistanceFromTarget);
        cameraCollisionRadius = Mathf.Clamp(cameraCollisionRadius, 0.05f, 1f);
    }

    private void TryDebugLog(bool isAiming)
    {
        if (!enableCameraDebugLog || Time.time < nextDebugLogTime)
        {
            return;
        }

        nextDebugLogTime = Time.time + Mathf.Max(0.05f, debugLogInterval);

        Camera main = Camera.main;
        float mainYaw = main != null ? main.transform.eulerAngles.y : 0f;
        float rootYaw = cameraRoot != null ? cameraRoot.eulerAngles.y : 0f;
        float yawDiff = Mathf.DeltaAngle(mainYaw, rootYaw);

        string activeCameraName = "none";
        if (normalCamera != null && aimCamera != null)
        {
            activeCameraName = normalCamera.Priority >= aimCamera.Priority ? normalCamera.name : aimCamera.name;
        }

        string brainUpdate = "none";
        CinemachineBrain brain = main != null ? main.GetComponent<CinemachineBrain>() : null;
        if (brain != null)
        {
            brainUpdate = brain.m_UpdateMethod.ToString();
        }

        string collisionOwner = "none";
        if (enableCameraCollision)
        {
            collisionOwner = isAiming ? "AimCamera" : "NormalCamera";
        }

        Debug.Log(
            $"[CameraDebug] aiming={isAiming} activeCam={activeCameraName} brainUpdate={brainUpdate} " +
            $"rootYaw={rootYaw:F1} mainYaw={mainYaw:F1} yawDiff={yawDiff:F1} " +
            $"targetYaw={targetYaw:F1} smoothedYaw={yaw:F1} targetPitch={targetPitch:F1} smoothedPitch={pitch:F1} " +
            $"recoilPitch={recoilPitch:F2} recoilYaw={recoilYaw:F2} collisionOwner={collisionOwner}"
        );
    }

    private void TryDebugCameraJump()
    {
        if (!enableCameraJumpDebugLog)
        {
            return;
        }

        Camera main = Camera.main;
        if (main == null)
        {
            return;
        }

        Vector3 currentPos = main.transform.position;
        float delta = Vector3.Distance(currentPos, lastMainCameraPosition);
        if (delta > cameraJumpDistanceThreshold)
        {
            Debug.LogWarning(
                $"[CameraJumpDebug] delta={delta:F3} threshold={cameraJumpDistanceThreshold:F3} " +
                $"camPos={currentPos:F3} lastPos={lastMainCameraPosition:F3} " +
                $"strategy={collisionStrategy} collideMask={collisionLayers.value} ignoreTag={collisionIgnoreTag}"
            );
        }

        lastMainCameraPosition = currentPos;
    }
}
