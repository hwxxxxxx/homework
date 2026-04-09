using System;
using System.Collections;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public enum WeaponKind
    {
        Rifle = 0,
        Shotgun = 1
    }

    [Header("Weapon Settings")]
    [SerializeField] protected WeaponConfigAsset weaponConfig;
    [SerializeField] protected StatBlock statBlock;

    protected float lastFireTime;
    protected int currentAmmoInMagazine;
    protected int reserveAmmo;
    protected bool isReloading;
    protected PlayerCombat ownerCombat;

    public event Action<int, int> OnAmmoChanged;
    public event Action OnFired;

    protected virtual void Awake()
    {
        if (statBlock == null)
        {
            statBlock = GetComponentInParent<StatBlock>();
        }

        statBlock.SetBaseValue(StatIds.WeaponDamage, weaponConfig.Damage);
        statBlock.SetBaseValue(StatIds.WeaponFireRate, weaponConfig.FireRate);
        statBlock.SetBaseValue(StatIds.WeaponReloadTime, weaponConfig.ReloadTime);

        currentAmmoInMagazine = weaponConfig.MagazineSize;
        reserveAmmo = weaponConfig.ReserveAmmo;
        NotifyAmmoChanged();
    }

    public virtual bool CanFire()
    {
        if (isReloading) return false;
        if (currentAmmoInMagazine <= 0) return false;

        float currentFireRate = GetFireRateValue();
        return Time.time >= lastFireTime + 1f / currentFireRate;
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
        if (currentAmmoInMagazine >= weaponConfig.MagazineSize) return;
        if (reserveAmmo <= 0) return;

        runner.StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(GetReloadTimeValue());

        int neededAmmo = weaponConfig.MagazineSize - currentAmmoInMagazine;
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

    public string GetWeaponDisplayName()
    {
        return weaponConfig.DisplayName;
    }

    public WeaponKind GetWeaponKind()
    {
        return weaponConfig.WeaponKind;
    }

    public virtual void BindOwner(PlayerCombat owner)
    {
        ownerCombat = owner;

        if (statBlock == null && ownerCombat != null)
        {
            statBlock = ownerCombat.GetComponent<StatBlock>();
        }
    }

    protected int GetDamageValue()
    {
        return Mathf.RoundToInt(GetStatValue(StatIds.WeaponDamage));
    }

    protected float GetFireRateValue()
    {
        return GetStatValue(StatIds.WeaponFireRate);
    }

    protected float GetReloadTimeValue()
    {
        return GetStatValue(StatIds.WeaponReloadTime);
    }

    protected void SpawnPooledEffect(
        ParticleSystem prefab,
        Vector3 position,
        Quaternion rotation,
        float fallbackLifetime
    )
    {
        if (prefab == null)
        {
            return;
        }

        ParticleSystem fx = PoolService.Spawn(prefab, position, rotation);
        if (fx == null)
        {
            return;
        }

        fx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        fx.Clear(true);
        fx.Play(true);
        PoolService.DespawnAfterDelay(fx.gameObject, fallbackLifetime);
    }

    protected void PrepareParticleTemplate(ParticleSystem template)
    {
        if (template == null)
        {
            return;
        }

        ParticleSystem.MainModule main = template.main;
        main.playOnAwake = false;

        if (!template.gameObject.scene.IsValid())
        {
            return;
        }

        template.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        template.Clear(true);
    }

    protected static bool TryGetDamageableFromCollider(Collider collider, out IDamageable damageable)
    {
        return CombatTargetResolver.TryResolveDamageable(collider, out damageable);
    }

    private float GetStatValue(string statId)
    {
        return statBlock.GetStatValue(statId);
    }

    protected void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmoInMagazine, reserveAmmo);
    }

    protected abstract void Fire();
}
