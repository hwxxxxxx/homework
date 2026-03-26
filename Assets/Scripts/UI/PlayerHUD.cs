using TMPro;
using UnityEngine;

public class PlayerHUD : MonoBehaviour
{
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private TextMeshProUGUI ammoText;

    private WeaponBase currentWeapon;

    private void Start()
    {
        currentWeapon = playerCombat.GetCurrentWeapon();

        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged += UpdateAmmoText;
            UpdateAmmoText(
                currentWeapon.GetCurrentAmmoInMagazine(),
                currentWeapon.GetReserveAmmo()
            );
        }
    }

    private void OnDestroy()
    {
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= UpdateAmmoText;
        }
    }

    private void UpdateAmmoText(int currentAmmo, int reserveAmmo)
    {
        ammoText.text = currentAmmo + " / " + reserveAmmo;
    }
}