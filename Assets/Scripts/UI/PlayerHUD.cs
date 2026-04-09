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
    private VisualElement skillCooldownFill;
    private VisualElement hudRoot;
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
        if (!presentationReady)
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
        if (presentationReady)
        {
            return true;
        }

        if (hudDocument == null)
        {
            hudDocument = GetComponent<UIDocument>();
        }

        VisualElement root = hudDocument.rootVisualElement;
        if (root == null)
        {
            return false;
        }

        hudRoot = root.Q<VisualElement>("hud-root");
        ammoText = root.Q<Label>("ammo-text");
        healthText = root.Q<Label>("health-text");
        skillCooldownText = root.Q<Label>("skill-cooldown-text");
        tutorialText = root.Q<Label>("tutorial-text");
        skillCooldownFill = root.Q<VisualElement>("skill-cooldown-fill");

        if (hudRoot == null || ammoText == null || healthText == null || skillCooldownText == null || tutorialText == null || skillCooldownFill == null)
        {
            return false;
        }

        textConfig = UiTextConfigProvider.Config;
        tutorialText.text = textConfig.TutorialMessage;
        presentationReady = true;
        return true;
    }
}
