using UnityEngine;

[DisallowMultipleComponent]
public class WeaponIkTargetBinder : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Transform leftHandTargetRuntime;
    [SerializeField] private string leftHandAnchorName = "LeftHandTarget";
    [SerializeField] private bool copyRotation = true;

    private Transform activeAnchor;

    private void Awake()
    {
        if (playerCombat == null)
        {
            playerCombat = GetComponent<PlayerCombat>();
        }
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
