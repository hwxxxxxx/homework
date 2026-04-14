using UnityEngine;
using UnityEngine.Animations.Rigging;

[DisallowMultipleComponent]
public class WeaponIkTargetBinder : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TwoBoneIKConstraint leftHandIkConstraint;
    [SerializeField] private Transform leftHandTargetRuntime;
    [SerializeField] private string leftHandAnchorName = "LeftHandTarget";
    [SerializeField] private bool copyRotation = true;
    [SerializeField] private bool disableLeftHandIkWhenAirborne = true;
    [SerializeField] private bool disableLeftHandIkWhenReloading = true;
    [SerializeField, Range(0f, 1f)] private float groundedIkWeight = 1f;
    [SerializeField, Range(0f, 1f)] private float airborneIkWeight = 0f;

    private Transform activeAnchor;

    private void Awake()
    {
        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
        }

        if (playerController == null)
        {
            playerController = GetComponent<PlayerController>();
        }

        CacheLeftHandIkConstraint();
    }

    private void OnEnable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnCurrentWeaponChanged += HandleWeaponChanged;
        }

        HandleWeaponChanged(playerCombat != null ? playerCombat.GetCurrentWeapon() : null);
    }

    private void OnDisable()
    {
        if (playerCombat != null)
        {
            playerCombat.OnCurrentWeaponChanged -= HandleWeaponChanged;
        }
    }

    private void LateUpdate()
    {
        UpdateLeftHandIkWeight();
        SyncRuntimeTarget();
    }

    private void HandleWeaponChanged(WeaponBase weapon)
    {
        activeAnchor = ResolveAnchor(weapon);
        SyncRuntimeTarget();
    }

    private Transform ResolveAnchor(WeaponBase weapon)
    {
        if (weapon == null)
        {
            return null;
        }

        return FindDeepChildByName(weapon.transform, leftHandAnchorName);
    }

    private void SyncRuntimeTarget()
    {
        if (leftHandTargetRuntime == null || activeAnchor == null)
        {
            return;
        }

        leftHandTargetRuntime.position = activeAnchor.position;
        if (copyRotation)
        {
            leftHandTargetRuntime.rotation = activeAnchor.rotation;
        }
    }

    private void UpdateLeftHandIkWeight()
    {
        if (leftHandIkConstraint == null)
        {
            CacheLeftHandIkConstraint();
        }

        if (leftHandIkConstraint == null)
        {
            return;
        }

        float targetWeight = groundedIkWeight;
        if (disableLeftHandIkWhenAirborne && playerController != null && !playerController.IsGrounded)
        {
            targetWeight = airborneIkWeight;
        }

        if (disableLeftHandIkWhenReloading && playerCombat != null)
        {
            WeaponBase currentWeapon = playerCombat.GetCurrentWeapon();
            if (currentWeapon != null && currentWeapon.IsReloading())
            {
                targetWeight = airborneIkWeight;
            }
        }

        if (!Mathf.Approximately(leftHandIkConstraint.weight, targetWeight))
        {
            leftHandIkConstraint.weight = targetWeight;
        }
    }

    private void CacheLeftHandIkConstraint()
    {
        if (leftHandIkConstraint != null)
        {
            return;
        }

        TwoBoneIKConstraint[] constraints = GetComponentsInChildren<TwoBoneIKConstraint>(true);
        for (int i = 0; i < constraints.Length; i++)
        {
            TwoBoneIKConstraint candidate = constraints[i];
            if (candidate == null)
            {
                continue;
            }

            if (candidate.name == "LeftArmIK")
            {
                leftHandIkConstraint = candidate;
                return;
            }
        }

        if (constraints.Length > 0)
        {
            leftHandIkConstraint = constraints[0];
        }
    }

    private static Transform FindDeepChildByName(Transform root, string childName)
    {
        if (root == null || string.IsNullOrWhiteSpace(childName))
        {
            return null;
        }

        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.name == childName)
            {
                return child;
            }

            Transform nested = FindDeepChildByName(child, childName);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}
