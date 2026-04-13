using UnityEngine;
using UnityEngine.UIElements;
using System;

[RequireComponent(typeof(UIDocument))]
public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerSkillSystem playerSkillSystem;
    [SerializeField] private EffectController playerEffectController;

    private WeaponBase currentWeapon;
    private UIDocument hudDocument;
    private Label ammoText;
    private Label healthText;
    private Label skillCooldownText;
    private Label tutorialText;
    private Label buffDamageBonusText;
    private Label buffFireRateBonusText;
    private Label buffReloadBonusText;
    private VisualElement skillCooldownFill;
    private VisualElement hudRoot;
    private VisualElement boundDocumentRoot;
    private UiTextConfigAsset textConfig;
    private bool presentationReady;

    private void Awake()
    {
        EnsurePresentationReady();
    }

    private void OnEnable()
    {
        if (EnsurePresentationReady())
        {
            SetGameplayActive(false);
        }
    }

    private void OnDisable()
    {
        UnbindPlayerEvents();
    }

    private void OnDestroy()
    {
        UnbindPlayerEvents();
    }

    private void Update()
    {
        if (!EnsurePresentationReady())
        {
            return;
        }

        if (hudRoot.style.display == DisplayStyle.Flex)
        {
            WeaponBase latestWeapon = playerCombat.GetCurrentWeapon();
            if (latestWeapon != currentWeapon)
            {
                HandleCurrentWeaponChanged(latestWeapon);
            }

            if (currentWeapon != null)
            {
                UpdateAmmoText(currentWeapon.GetCurrentAmmoInMagazine(), currentWeapon.GetReserveAmmo());
            }

            HandlePlayerHealthChanged(playerStats.CurrentHealth, playerStats.MaxHealth);
            RefreshSkillCooldownUI();
            RefreshBuffStatUI();
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
            SetBuffStatTexts(0f, 0f, 0f);
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
        healthText.text = string.Format(textConfig.HudHealthTemplate, current, max);
    }

    private void UpdateAmmoText(int currentAmmo, int reserveAmmo)
    {
        ammoText.text = currentAmmo + " / " + reserveAmmo;
    }

    private void RefreshSkillCooldownUI()
    {
        SkillBase skill = playerSkillSystem.EquippedSkill;
        if (skill == null)
        {
            SetSkillCooldownUI(0f, true, 0f);
            return;
        }

        EffectAsset cooldownEffect = skill.CooldownEffect;
        if (cooldownEffect == null)
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
        skillCooldownFill.style.width = Length.Percent(fill * 100f);
        skillCooldownFill.style.display = ready ? DisplayStyle.None : DisplayStyle.Flex;
        skillCooldownText.text = ready
            ? textConfig.HudSkillReadyText
            : string.Format(textConfig.HudSkillCooldownTemplate, remaining);
    }

    public void SetGameplayActive(bool active)
    {
        if (!EnsurePresentationReady())
        {
            throw new InvalidOperationException("PlayerHUD presentation is not ready.");
        }

        hudRoot.style.display = active ? DisplayStyle.Flex : DisplayStyle.None;

        if (active)
        {
            return;
        }

        UnbindPlayerEvents();
        UpdateAmmoText(0, 0);
        HandlePlayerHealthChanged(0, 0);
        SetSkillCooldownUI(0f, true, 0f);
        SetBuffStatTexts(0f, 0f, 0f);
    }

    public void ConfigureSceneReferences(
        PlayerCombat combat,
        PlayerStats stats,
        PlayerSkillSystem skillSystem,
        EffectController effectController
    )
    {
        if (!EnsurePresentationReady())
        {
            throw new InvalidOperationException("PlayerHUD presentation is not ready.");
        }

        UnbindPlayerEvents();
        playerCombat = combat;
        playerStats = stats;
        playerSkillSystem = skillSystem;
        playerEffectController = effectController;

        playerCombat.OnCurrentWeaponChanged += HandleCurrentWeaponChanged;
        HandleCurrentWeaponChanged(playerCombat.GetCurrentWeapon());

        playerStats.OnHealthChanged += HandlePlayerHealthChanged;
        HandlePlayerHealthChanged(playerStats.CurrentHealth, playerStats.MaxHealth);

        RefreshSkillCooldownUI();
        RefreshBuffStatUI();
    }

    private void UnbindPlayerEvents()
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
            currentWeapon = null;
        }
    }

    public bool EnsurePresentationReady()
    {
        if (hudDocument == null)
        {
            hudDocument = GetComponent<UIDocument>();
        }

        VisualElement root = hudDocument.rootVisualElement;
        if (root == null)
        {
            boundDocumentRoot = null;
            presentationReady = false;
            return false;
        }

        if (presentationReady && ReferenceEquals(root, boundDocumentRoot))
        {
            return true;
        }

        return RebindPresentation(root);
    }

    private bool RebindPresentation(VisualElement root)
    {
        hudRoot = root.Q<VisualElement>("hud-root");
        ammoText = root.Q<Label>("ammo-text");
        healthText = root.Q<Label>("health-text");
        skillCooldownText = root.Q<Label>("skill-cooldown-text");
        tutorialText = root.Q<Label>("tutorial-text");
        buffDamageBonusText = root.Q<Label>("buff-damage-bonus-text");
        buffFireRateBonusText = root.Q<Label>("buff-fire-rate-bonus-text");
        buffReloadBonusText = root.Q<Label>("buff-reload-bonus-text");
        skillCooldownFill = root.Q<VisualElement>("skill-cooldown-fill");

        if (hudRoot == null ||
            ammoText == null ||
            healthText == null ||
            skillCooldownText == null ||
            tutorialText == null ||
            buffDamageBonusText == null ||
            buffFireRateBonusText == null ||
            buffReloadBonusText == null ||
            skillCooldownFill == null)
        {
            return false;
        }

        textConfig = UiTextConfigProvider.Config;
        tutorialText.text = textConfig.TutorialMessage;
        SetBuffStatTexts(0f, 0f, 0f);
        boundDocumentRoot = root;
        presentationReady = true;
        return true;
    }

    private void RefreshBuffStatUI()
    {
        if (currentWeapon == null)
        {
            SetBuffStatTexts(0f, 0f, 0f);
            return;
        }

        float damageBonus = ComputePercentDelta(currentWeapon.GetBaseDamageValue(), currentWeapon.GetCurrentDamageValue());
        float fireRateBonus = ComputePercentDelta(currentWeapon.GetBaseFireRateValue(), currentWeapon.GetCurrentFireRateValue());
        float reloadBonus = -ComputePercentDelta(currentWeapon.GetBaseReloadTimeValue(), currentWeapon.GetCurrentReloadTimeValue());

        SetBuffStatTexts(damageBonus, fireRateBonus, reloadBonus);
    }

    private void SetBuffStatTexts(float damageBonus, float fireRateBonus, float reloadBonus)
    {
        buffDamageBonusText.text = $"Damage Buff: {FormatSignedPercent(damageBonus)}";
        buffFireRateBonusText.text = $"FireRate Buff: {FormatSignedPercent(fireRateBonus)}";
        buffReloadBonusText.text = $"Reload Buff: {FormatSignedPercent(reloadBonus)}";
    }

    private static float ComputePercentDelta(float baseValue, float currentValue)
    {
        if (Mathf.Abs(baseValue) <= 0.0001f)
        {
            return 0f;
        }

        return (currentValue / baseValue - 1f) * 100f;
    }

    private static string FormatSignedPercent(float value)
    {
        if (Mathf.Abs(value) < 0.05f)
        {
            return "+0%";
        }

        return value > 0f ? $"+{value:0.#}%" : $"{value:0.#}%";
    }
}
