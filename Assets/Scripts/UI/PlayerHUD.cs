using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    private const string HealthTextNodeName = "HealthText";
    private const string SkillCooldownFillNodeName = "SkillCooldownFill";
    private const string SkillCooldownTextNodeName = "SkillCooldownText";

    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerSkillSystem playerSkillSystem;
    [SerializeField] private EffectController playerEffectController;

    [Header("Ammo UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Health UI")]
    [SerializeField] private TextMeshProUGUI healthText;

    [Header("Skill Cooldown UI")]
    [SerializeField] private Image skillCooldownFillImage;
    [SerializeField] private TextMeshProUGUI skillCooldownText;

    private WeaponBase currentWeapon;

    private void Start()
    {
        if (playerSkillSystem == null && playerCombat != null)
        {
            playerSkillSystem = playerCombat.GetComponent<PlayerSkillSystem>();
        }

        if (playerEffectController == null && playerCombat != null)
        {
            playerEffectController = playerCombat.GetComponent<EffectController>();
        }

        AutoBindUiReferences();

        if (playerCombat != null)
        {
            playerCombat.OnCurrentWeaponChanged += HandleCurrentWeaponChanged;
            HandleCurrentWeaponChanged(playerCombat.GetCurrentWeapon());
        }
        else
        {
            Debug.LogWarning("PlayerHUD: missing PlayerCombat reference.", this);
        }

        if (skillCooldownFillImage != null && skillCooldownFillImage.type != Image.Type.Filled)
        {
            skillCooldownFillImage.type = Image.Type.Filled;
            skillCooldownFillImage.fillMethod = Image.FillMethod.Radial360;
            skillCooldownFillImage.fillClockwise = false;
            skillCooldownFillImage.fillOrigin = (int)Image.Origin360.Top;
        }

        if (playerStats != null)
        {
            playerStats.OnHealthChanged += HandlePlayerHealthChanged;
            HandlePlayerHealthChanged(playerStats.CurrentHealth, playerStats.MaxHealth);
        }
        else
        {
            Debug.LogWarning("PlayerHUD: missing PlayerStats reference.", this);
        }

        RefreshSkillCooldownUI();
    }

    private void Update()
    {
        RefreshSkillCooldownUI();
    }

    private void OnDestroy()
    {
        if (playerCombat != null)
        {
            playerCombat.OnCurrentWeaponChanged -= HandleCurrentWeaponChanged;
        }

        if (playerStats != null)
        {
            playerStats.OnHealthChanged -= HandlePlayerHealthChanged;
        }

        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoText;
        }
    }

    private void HandleCurrentWeaponChanged(WeaponBase nextWeapon)
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoText;
        }

        currentWeapon = nextWeapon;
        if (currentWeapon == null)
        {
            UpdateAmmoText(0, 0);
            return;
        }

        currentWeapon.OnAmmoChanged += UpdateAmmoText;
        UpdateAmmoText(
            currentWeapon.GetCurrentAmmoInMagazine(),
            currentWeapon.GetReserveAmmo()
        );
    }

    private void HandlePlayerHealthChanged(int current, int max)
    {
        if (healthText == null)
        {
            return;
        }

        healthText.text = $"HP {current} / {max}";
    }

    private void UpdateAmmoText(int currentAmmo, int reserveAmmo)
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo + " / " + reserveAmmo;
        }
    }

    private void RefreshSkillCooldownUI()
    {
        SkillBase skill = playerSkillSystem != null ? playerSkillSystem.EquippedSkill : null;
        EffectAsset cooldownEffect = skill != null ? skill.CooldownEffect : null;
        if (skill == null || cooldownEffect == null || playerEffectController == null)
        {
            SetSkillCooldownUI(0f, true, 0f);
            return;
        }

        if (!playerEffectController.TryGetEffectRemaining(cooldownEffect.EffectId, out float remaining, out float duration))
        {
            SetSkillCooldownUI(0f, true, 0f);
            return;
        }

        float fill = duration <= 0f ? 0f : Mathf.Clamp01(remaining / duration);
        SetSkillCooldownUI(fill, false, remaining);
    }

    private void SetSkillCooldownUI(float fill, bool ready, float remaining)
    {
        if (skillCooldownFillImage != null)
        {
            skillCooldownFillImage.fillAmount = fill;
            skillCooldownFillImage.enabled = !ready;
        }

        if (skillCooldownText != null)
        {
            skillCooldownText.text = ready ? "Q READY" : $"Q {remaining:0.0}s";
        }
    }

    private void AutoBindUiReferences()
    {
        if (healthText == null)
        {
            healthText = FindNamedUi<TextMeshProUGUI>(HealthTextNodeName);
        }

        if (skillCooldownFillImage == null)
        {
            skillCooldownFillImage = FindNamedUi<Image>(SkillCooldownFillNodeName);
        }

        if (skillCooldownText == null)
        {
            skillCooldownText = FindNamedUi<TextMeshProUGUI>(SkillCooldownTextNodeName);
        }
    }

    private T FindNamedUi<T>(string objectName) where T : Component
    {
        if (string.IsNullOrWhiteSpace(objectName))
        {
            return null;
        }

        Canvas canvas = ammoText != null ? ammoText.GetComponentInParent<Canvas>() : null;
        if (canvas != null)
        {
            Transform node = canvas.transform.Find(objectName);
            if (node != null)
            {
                T component = node.GetComponent<T>();
                if (component != null)
                {
                    return component;
                }
            }
        }

        Debug.LogWarning($"PlayerHUD: missing UI node '{objectName}'.", this);
        return null;
    }
}
