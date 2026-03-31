using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
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

    [Header("Tutorial UI")]
    [SerializeField] private TextMeshProUGUI tutorialText;
    [SerializeField, TextArea(2, 4)] private string tutorialMessage =
        "RMB: Aim\nLMB: Fire\nShift: Sprint\nSpace: Jump\nR: Reload\nQ: Skill";

    private WeaponBase currentWeapon;

    private void Start()
    {
        if (playerCombat == null || playerStats == null || playerSkillSystem == null || playerEffectController == null ||
            ammoText == null || healthText == null || skillCooldownFillImage == null || skillCooldownText == null ||
            tutorialText == null)
        {
            Debug.LogError("PlayerHUD references are not fully assigned.", this);
            enabled = false;
            return;
        }

        tutorialText.text = tutorialMessage;

        playerCombat.OnCurrentWeaponChanged += HandleCurrentWeaponChanged;
        HandleCurrentWeaponChanged(playerCombat.GetCurrentWeapon());

        if (skillCooldownFillImage != null && skillCooldownFillImage.type != Image.Type.Filled)
        {
            skillCooldownFillImage.type = Image.Type.Filled;
            skillCooldownFillImage.fillMethod = Image.FillMethod.Radial360;
            skillCooldownFillImage.fillClockwise = false;
            skillCooldownFillImage.fillOrigin = (int)Image.Origin360.Top;
        }

        playerStats.OnHealthChanged += HandlePlayerHealthChanged;
        HandlePlayerHealthChanged(playerStats.CurrentHealth, playerStats.MaxHealth);

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
        ammoText.text = currentAmmo + " / " + reserveAmmo;
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
        skillCooldownFillImage.fillAmount = fill;
        skillCooldownFillImage.enabled = !ready;
        skillCooldownText.text = ready ? "Q READY" : $"Q {remaining:0.0}s";
    }
}
