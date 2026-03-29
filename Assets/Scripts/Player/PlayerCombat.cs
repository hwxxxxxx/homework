using System;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private WeaponBase currentWeapon;

    public event Action<bool> OnAimStateChanged;

    public bool IsAiming { get; private set; }

    private void Update()
    {
        HandleAimState();
        HandleFire();
        HandleReload();
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

    public WeaponBase GetCurrentWeapon()
    {
        return currentWeapon;
    }
}
