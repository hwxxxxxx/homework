using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private WeaponBase currentWeapon;

    private void Update()
    {
        HandleFire();
        HandleReload();
    }

    private void HandleFire()
    {
        if (gameInput.IsFirePressed())
        {
            currentWeapon.TryFire();
        }
    }

    private void HandleReload()
    {
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