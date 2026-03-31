using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private WeaponBase currentWeapon;
    [SerializeField] private WeaponBase[] weaponSlots;
    [SerializeField] private int defaultWeaponIndex;
    [SerializeField] private PlayerSkillSystem playerSkillSystem;

    public event Action<bool> OnAimStateChanged;
    public event Action<WeaponBase> OnCurrentWeaponChanged;

    public bool IsAiming { get; private set; }

    private void Awake()
    {
        if (playerSkillSystem == null)
        {
            playerSkillSystem = GetComponent<PlayerSkillSystem>();
        }

        InitializeWeapons();
    }

    private void Update()
    {
        if (GamePauseController.IsPaused)
        {
            if (IsAiming)
            {
                IsAiming = false;
                OnAimStateChanged?.Invoke(false);
            }

            return;
        }

        HandleAimState();
        HandleWeaponSwitch();
        HandleFire();
        HandleReload();
        HandleSkill();
    }

    private void InitializeWeapons()
    {
        List<WeaponBase> validWeapons = new List<WeaponBase>();

        if (weaponSlots != null)
        {
            foreach (WeaponBase weapon in weaponSlots)
            {
                if (weapon != null && !validWeapons.Contains(weapon))
                {
                    validWeapons.Add(weapon);
                }
            }
        }

        if (validWeapons.Count == 0)
        {
            WeaponBase[] discovered = GetComponentsInChildren<WeaponBase>(true);
            foreach (WeaponBase weapon in discovered)
            {
                if (weapon != null && !validWeapons.Contains(weapon))
                {
                    validWeapons.Add(weapon);
                }
            }
        }

        if (validWeapons.Count == 0 && currentWeapon != null)
        {
            validWeapons.Add(currentWeapon);
        }

        weaponSlots = validWeapons.ToArray();

        foreach (WeaponBase weapon in weaponSlots)
        {
            weapon.BindOwner(this);
        }

        int initialIndex = Mathf.Clamp(defaultWeaponIndex, 0, Mathf.Max(0, weaponSlots.Length - 1));
        if (weaponSlots.Length > 0)
        {
            currentWeapon = weaponSlots[initialIndex];
            UpdateWeaponVisibility();
            OnCurrentWeaponChanged?.Invoke(currentWeapon);
        }
    }

    private void HandleAimState()
    {
        if (gameInput == null)
        {
            return;
        }

        bool newAimState = gameInput.IsAimPressed();
        if (newAimState == IsAiming)
        {
            return;
        }

        IsAiming = newAimState;
        OnAimStateChanged?.Invoke(IsAiming);
    }

    private void HandleFire()
    {
        if (gameInput == null || currentWeapon == null)
        {
            return;
        }

        if (gameInput.IsFirePressed())
        {
            currentWeapon.TryFire();
        }
    }

    private void HandleReload()
    {
        if (gameInput == null || currentWeapon == null)
        {
            return;
        }

        if (gameInput.IsReloadPressed())
        {
            currentWeapon.StartReload(this);
        }
    }

    private void HandleSkill()
    {
        if (gameInput == null || playerSkillSystem == null)
        {
            return;
        }

        playerSkillSystem.TryHandleInput(gameInput);
    }

    private void HandleWeaponSwitch()
    {
        if (gameInput == null || weaponSlots == null || weaponSlots.Length <= 1)
        {
            return;
        }

        int directIndex = gameInput.GetDirectWeaponSlotInput();
        if (directIndex >= 0)
        {
            TrySwitchToSlot(directIndex);
            return;
        }

        if (gameInput.IsNextWeaponPressed())
        {
            CycleWeapon(1);
            return;
        }

        if (gameInput.IsPreviousWeaponPressed())
        {
            CycleWeapon(-1);
        }
    }

    private void TrySwitchToSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length)
        {
            return;
        }

        WeaponBase nextWeapon = weaponSlots[slotIndex];
        if (nextWeapon == null || nextWeapon == currentWeapon)
        {
            return;
        }

        currentWeapon = nextWeapon;
        currentWeapon.BindOwner(this);
        UpdateWeaponVisibility();
        OnCurrentWeaponChanged?.Invoke(currentWeapon);
    }

    private void CycleWeapon(int direction)
    {
        if (weaponSlots == null || weaponSlots.Length <= 1)
        {
            return;
        }

        int currentIndex = Array.IndexOf(weaponSlots, currentWeapon);
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        for (int i = 1; i <= weaponSlots.Length; i++)
        {
            int candidate = (currentIndex + direction * i + weaponSlots.Length) % weaponSlots.Length;
            WeaponBase nextWeapon = weaponSlots[candidate];
            if (nextWeapon == null || nextWeapon == currentWeapon)
            {
                continue;
            }

            currentWeapon = nextWeapon;
            currentWeapon.BindOwner(this);
            UpdateWeaponVisibility();
            OnCurrentWeaponChanged?.Invoke(currentWeapon);
            return;
        }
    }

    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }

    private void UpdateWeaponVisibility()
    {
        if (weaponSlots == null)
        {
            return;
        }

        for (int i = 0; i < weaponSlots.Length; i++)
        {
            WeaponBase weapon = weaponSlots[i];
            if (weapon == null)
            {
                continue;
            }

            bool isCurrent = weapon == currentWeapon;
            if (weapon.gameObject.activeSelf != isCurrent)
            {
                weapon.gameObject.SetActive(isCurrent);
            }
        }
    }
}
