using System;
using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float fireRate = 5f;

    [Header("Ammo Settings")]
    [SerializeField] protected int magazineSize = 30;
    [SerializeField] protected int reserveAmmo = 90;
    [SerializeField] protected float reloadTime = 1.5f;

    protected float lastFireTime;
    protected int currentAmmoInMagazine;
    protected bool isReloading;

    public event Action<int, int> OnAmmoChanged;
    public event Action OnFired;

    protected virtual void Awake()
    {
        currentAmmoInMagazine = magazineSize;
        NotifyAmmoChanged();
    }

    public virtual bool CanFire()
    {
        if (isReloading) return false;
        if (currentAmmoInMagazine <= 0) return false;

        return Time.time >= lastFireTime + 1f / fireRate;
    }

    public virtual void TryFire()
    {
        if (!CanFire()) return;

        Fire();
        currentAmmoInMagazine--;
        lastFireTime = Time.time;
        OnFired?.Invoke();

        NotifyAmmoChanged();
    }

    public virtual void StartReload(MonoBehaviour runner)
    {
        if (isReloading) return;
        if (currentAmmoInMagazine >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        runner.StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        int neededAmmo = magazineSize - currentAmmoInMagazine;
        int ammoToLoad = Mathf.Min(neededAmmo, reserveAmmo);

        currentAmmoInMagazine += ammoToLoad;
        reserveAmmo -= ammoToLoad;

        isReloading = false;
        NotifyAmmoChanged();
    }

    public int GetCurrentAmmoInMagazine()
    {
        return currentAmmoInMagazine;
    }

    public int GetReserveAmmo()
    {
        return reserveAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    protected void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmoInMagazine, reserveAmmo);
    }

    protected abstract void Fire();
}
