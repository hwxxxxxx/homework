using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private PlayerSkillSystem playerSkillSystem;
    [SerializeField] private EffectController playerEffectController;
    [SerializeField, TextArea(2, 4)] private string tutorialMessage =
        "RMB: Aim\nLMB: Fire\nShift: Sprint\nSpace: Jump\nR: Reload\nQ: Skill";

    private WeaponBase currentWeapon;
    private UIDocument hudDocument;
    private Label ammoText;
    private Label healthText;
    private Label skillCooldownText;
    private Label tutorialText;
    private VisualElement skillCooldownFill;
    private VisualElement hudRoot;

    private void Awake()
    {
        hudDocument = GetComponent<UIDocument>();
        VisualElement root = hudDocument.rootVisualElement;
        hudRoot = root.Q<VisualElement>("hud-root");
        ammoText = root.Q<Label>("ammo-text");
        healthText = root.Q<Label>("health-text");
        skillCooldownText = root.Q<Label>("skill-cooldown-text");
        tutorialText = root.Q<Label>("tutorial-text");
        skillCooldownFill = root.Q<VisualElement>("skill-cooldown-fill");
        tutorialText.text = tutorialMessage;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        RebindForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        UnbindPlayerEvents();
    }

    private void OnDestroy()
    {
        UnbindPlayerEvents();
    }

    private void Update()
    {
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
        healthText.text = $"HP {current} / {max}";
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
        skillCooldownText.text = ready ? "Q READY" : $"Q {remaining:0.0}s";
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindForScene(scene.name);
    }

    private void RebindForScene(string sceneName)
    {
        bool gameplayScene = IsGameplayScene(sceneName);
        hudRoot.style.display = gameplayScene ? DisplayStyle.Flex : DisplayStyle.None;

        if (!gameplayScene)
        {
            UnbindPlayerEvents();
            UpdateAmmoText(0, 0);
            HandlePlayerHealthChanged(0, 0);
            SetSkillCooldownUI(0f, true, 0f);
            return;
        }

        BindPlayerReferences();
    }

    private void BindPlayerReferences()
    {
        UnbindPlayerEvents();

        playerCombat = FindObjectOfType<PlayerCombat>(true);
        playerStats = FindObjectOfType<PlayerStats>(true);
        playerSkillSystem = FindObjectOfType<PlayerSkillSystem>(true);
        playerEffectController = FindObjectOfType<EffectController>(true);

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

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName == "GameScene" ||
               sceneName == "Level_Body_2" ||
               sceneName == "Level_Soul_1" ||
               sceneName == "Level_Memory_1";
    }
}
