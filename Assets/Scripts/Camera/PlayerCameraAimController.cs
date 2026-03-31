using Cinemachine;
using UnityEngine;

public class PlayerCameraAimController : MonoBehaviour
{
    public struct LookSensitivitySettings
    {
        public float normalX;
        public float normalY;
        public float aimMultiplier;
        public float globalScale;
    }

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
    [SerializeField] private float globalLookSensitivityScale = 1f;
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
    [SerializeField] private float collisionDampingInto = 0.12f;
    [SerializeField] private float collisionDampingFrom = 0.18f;
    [SerializeField] private bool autoSanitizeCollisionMask = true;
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

    private void OnEnable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnAimStateChanged += HandleAimStateChanged;
            playerCombat.OnCurrentWeaponChanged += HandleCurrentWeaponChanged;
        }

        BindWeaponEvents();
    }

    private void Start()
    {
        normalFreeLook = normalCamera as CinemachineFreeLook;
        SetupFreeLookInputOverride();
        EnsureValidCollisionMask();

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
        float effectiveSensitivityScale = Mathf.Max(0.05f, globalLookSensitivityScale);

        Vector2 lookInput = ProcessLookInput(gameInput.GetLookInput());
        smoothedLookInput = Vector2.SmoothDamp(
            smoothedLookInput,
            lookInput,
            ref lookInputSmoothVelocity,
            lookInputSmoothingTime
        );

        targetYaw += smoothedLookInput.x * normalLookSensitivityX * sensitivityMultiplier * effectiveSensitivityScale * Time.deltaTime;
        targetPitch -= smoothedLookInput.y * normalLookSensitivityY * sensitivityMultiplier * effectiveSensitivityScale * Time.deltaTime;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        UpdateRecoil();

        float desiredYaw = targetYaw + recoilYaw;
        float desiredPitch = Mathf.Clamp(targetPitch + recoilPitch, minPitch, maxPitch);
        yaw = Mathf.SmoothDampAngle(yaw, desiredYaw, ref yawVelocity, lookRotationDampingTime);
        pitch = Mathf.SmoothDampAngle(pitch, desiredPitch, ref pitchVelocity, lookRotationDampingTime);

        cameraRoot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        SyncFreeLookXAxisWithYaw();
        SyncFreeLookYAxisWithPitch();
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
            playerCombat.OnCurrentWeaponChanged -= HandleCurrentWeaponChanged;
        }

        UnbindWeaponEvents();
    }

    private void HandleAimStateChanged(bool isAiming)
    {
        ApplyAimState(isAiming);
    }

    private void HandleCurrentWeaponChanged(WeaponBase nextWeapon)
    {
        UnbindWeaponEvents();
        currentWeapon = nextWeapon;
        if (currentWeapon != null)
        {
            currentWeapon.OnFired += HandleWeaponFired;
        }
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

        bool enableCollisionRuntime = enableCameraCollision && HasValidCollisionMask();
        ConfigureThirdPersonFollowCollision(enableCollisionRuntime);
    }

    private void ApplyDamping()
    {
        const float aimDampingMultiplier = 0.55f;
        float normalDamping = positionDamping;
        float aimDamping = positionDamping * aimDampingMultiplier;

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
                    orbital.m_XDamping = normalDamping;
                    orbital.m_YDamping = normalDamping;
                    orbital.m_ZDamping = normalDamping;
                }
            }
        }

        ApplyDampingToVirtualCamera(normalCamera as CinemachineVirtualCamera, normalDamping);
        ApplyDampingToVirtualCamera(aimCamera as CinemachineVirtualCamera, aimDamping);
    }

    private void ConfigureThirdPersonFollowCollision(bool enableCollision)
    {
        ConfigureThirdPersonFollowCollision(normalCamera as CinemachineVirtualCamera, enableCollision);
        ConfigureThirdPersonFollowCollision(aimCamera as CinemachineVirtualCamera, enableCollision);
    }

    private void ConfigureThirdPersonFollowCollision(CinemachineVirtualCamera virtualCamera, bool enableCollision)
    {
        if (virtualCamera == null)
        {
            return;
        }

        Cinemachine3rdPersonFollow thirdPersonFollow =
            virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (thirdPersonFollow == null)
        {
            return;
        }

        if (!enableCollision)
        {
            thirdPersonFollow.CameraCollisionFilter = 0;
            thirdPersonFollow.IgnoreTag = string.Empty;
            return;
        }

        thirdPersonFollow.CameraCollisionFilter = collisionLayers;
        thirdPersonFollow.IgnoreTag = collisionIgnoreTag;
        thirdPersonFollow.CameraRadius = cameraCollisionRadius;
        thirdPersonFollow.DampingIntoCollision = collisionDampingInto;
        thirdPersonFollow.DampingFromCollision = collisionDampingFrom;
    }

    private void ApplyDampingToVirtualCamera(CinemachineVirtualCamera virtualCamera, float dampingValue)
    {
        if (virtualCamera == null)
        {
            return;
        }

        Cinemachine3rdPersonFollow thirdPersonFollow = virtualCamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        if (thirdPersonFollow != null)
        {
            thirdPersonFollow.Damping = new Vector3(dampingValue, dampingValue, dampingValue);
            return;
        }

        CinemachineTransposer transposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer != null)
        {
            transposer.m_XDamping = dampingValue;
            transposer.m_YDamping = dampingValue;
            transposer.m_ZDamping = dampingValue;
        }
    }

    private void EnsureValidCollisionMask()
    {
        if (!enableCameraCollision)
        {
            return;
        }

        if (autoSanitizeCollisionMask)
        {
            collisionLayers = SanitizeCollisionMask(collisionLayers);
        }

        if (HasValidCollisionMask())
        {
            return;
        }

        collisionLayers = BuildFallbackCollisionMask();
        if (enableCameraDebugLog)
        {
            Debug.LogWarning($"[CameraDebug] collisionLayers invalid, fallback mask={collisionLayers.value}.");
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
        normalLookSensitivityX = Mathf.Max(1f, normalLookSensitivityX);
        normalLookSensitivityY = Mathf.Max(1f, normalLookSensitivityY);
        aimLookSensitivityMultiplier = Mathf.Clamp(aimLookSensitivityMultiplier, 0.1f, 2f);
        globalLookSensitivityScale = Mathf.Clamp(globalLookSensitivityScale, 0.05f, 3f);

        collisionDampingInto = Mathf.Clamp(collisionDampingInto, 0f, 2f);
        collisionDampingFrom = Mathf.Clamp(collisionDampingFrom, 0f, 2f);
        cameraCollisionRadius = Mathf.Clamp(cameraCollisionRadius, 0.05f, 1f);
    }

    private bool HasValidCollisionMask()
    {
        return collisionLayers.value != 0;
    }

    private LayerMask SanitizeCollisionMask(LayerMask sourceMask)
    {
        int value = sourceMask.value;
        if (value == 0)
        {
            return BuildFallbackCollisionMask();
        }

        int dedicatedCameraObstacleMask = AddLayerIfExists("CameraObstacle");
        int groundMask = AddLayerIfExists("ground") | AddLayerIfExists("Ground");
        if (dedicatedCameraObstacleMask != 0 && value == groundMask)
        {
            return dedicatedCameraObstacleMask;
        }

        if (value == ~0)
        {
            LayerMask curatedMask = BuildNamedCollisionMask(includeDefault: true);
            if (curatedMask.value != 0)
            {
                return curatedMask;
            }
        }

        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
        {
            value &= ~(1 << playerLayer);
        }

        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycastLayer >= 0)
        {
            value &= ~(1 << ignoreRaycastLayer);
        }

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer >= 0)
        {
            value &= ~(1 << uiLayer);
        }

        if (value == 0)
        {
            return BuildFallbackCollisionMask();
        }

        return value;
    }

    private LayerMask BuildFallbackCollisionMask()
    {
        int value = BuildNamedCollisionMask(includeDefault: true).value;

        if (value == 0)
        {
            value = ~0;
            int playerLayer = LayerMask.NameToLayer("Player");
            if (playerLayer >= 0)
            {
                value &= ~(1 << playerLayer);
            }

            int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
            if (ignoreRaycastLayer >= 0)
            {
                value &= ~(1 << ignoreRaycastLayer);
            }

            int uiLayer = LayerMask.NameToLayer("UI");
            if (uiLayer >= 0)
            {
                value &= ~(1 << uiLayer);
            }
        }

        return value;
    }

    private LayerMask BuildNamedCollisionMask(bool includeDefault)
    {
        int value = AddLayerIfExists("CameraObstacle");
        value |= AddLayerIfExists("Environment");
        value |= AddLayerIfExists("Obstacle");

        if (value != 0)
        {
            return value;
        }

        if (includeDefault)
        {
            value |= AddLayerIfExists("Default");
        }

        value |= AddLayerIfExists("ground");
        value |= AddLayerIfExists("Ground");
        return value;
    }

    private static int AddLayerIfExists(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer < 0)
        {
            return 0;
        }

        return 1 << layer;
    }

    public void SetLookSensitivity(float normalX, float normalY, float aimMultiplier)
    {
        normalLookSensitivityX = Mathf.Max(1f, normalX);
        normalLookSensitivityY = Mathf.Max(1f, normalY);
        aimLookSensitivityMultiplier = Mathf.Clamp(aimMultiplier, 0.1f, 2f);
    }

    public void SetGlobalLookSensitivityScale(float scale)
    {
        globalLookSensitivityScale = Mathf.Clamp(scale, 0.05f, 3f);
    }

    public LookSensitivitySettings GetLookSensitivitySettings()
    {
        return new LookSensitivitySettings
        {
            normalX = normalLookSensitivityX,
            normalY = normalLookSensitivityY,
            aimMultiplier = aimLookSensitivityMultiplier,
            globalScale = globalLookSensitivityScale
        };
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

        Debug.Log(
            $"[CameraDebug] aiming={isAiming} activeCam={activeCameraName} brainUpdate={brainUpdate} " +
            $"rootYaw={rootYaw:F1} mainYaw={mainYaw:F1} yawDiff={yawDiff:F1} " +
            $"targetYaw={targetYaw:F1} smoothedYaw={yaw:F1} targetPitch={targetPitch:F1} smoothedPitch={pitch:F1} " +
            $"recoilPitch={recoilPitch:F2} recoilYaw={recoilYaw:F2}"
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
                $"collideMask={collisionLayers.value} ignoreTag={collisionIgnoreTag}"
            );
        }

        lastMainCameraPosition = currentPos;
    }
}
