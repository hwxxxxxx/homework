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
    [SerializeField] protected string weaponDisplayName = "Weapon";
    [SerializeField] protected WeaponKind weaponKind = WeaponKind.Rifle;
    [SerializeField] protected int damage = 10;
    [SerializeField] protected float fireRate = 5f;

    [Header("Ammo Settings")]
    [SerializeField] protected int magazineSize = 30;
    [SerializeField] protected int reserveAmmo = 90;
    [SerializeField] protected float reloadTime = 1.5f;
    [SerializeField] protected StatBlock statBlock;

    protected float lastFireTime;
    protected int currentAmmoInMagazine;
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

        currentAmmoInMagazine = magazineSize;
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
        if (currentAmmoInMagazine >= magazineSize) return;
        if (reserveAmmo <= 0) return;

        runner.StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        isReloading = true;

        yield return new WaitForSeconds(GetReloadTimeValue());

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

    public string GetWeaponDisplayName()
    {
        if (!string.IsNullOrWhiteSpace(weaponDisplayName))
        {
            return weaponDisplayName;
        }

        return gameObject.name;
    }

    public WeaponKind GetWeaponKind()
    {
        return weaponKind;
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
        return Mathf.Max(1, Mathf.RoundToInt(GetStatOrDefault(StatIds.WeaponDamage, damage)));
    }

    protected float GetFireRateValue()
    {
        return Mathf.Max(0.01f, GetStatOrDefault(StatIds.WeaponFireRate, fireRate));
    }

    protected float GetReloadTimeValue()
    {
        return Mathf.Max(0.05f, GetStatOrDefault(StatIds.WeaponReloadTime, reloadTime));
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
        PoolService.DespawnAfterDelay(fx.gameObject, GetEffectLifetime(fx, fallbackLifetime));
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

    private float GetStatOrDefault(string statId, float defaultValue)
    {
        if (statBlock == null || string.IsNullOrWhiteSpace(statId) || !statBlock.HasStat(statId))
        {
            return defaultValue;
        }

        return statBlock.GetStatValue(statId);
    }

    private static float GetEffectLifetime(ParticleSystem effect, float fallbackLifetime)
    {
        if (effect == null)
        {
            return Mathf.Max(0.05f, fallbackLifetime);
        }

        ParticleSystem.MainModule main = effect.main;
        float duration = main.duration;
        float startLifetime = main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
            ? main.startLifetime.constantMax
            : main.startLifetime.constant;
        float computed = duration + startLifetime + 0.05f;
        return Mathf.Max(0.05f, Mathf.Max(fallbackLifetime, computed));
    }

    protected void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmoInMagazine, reserveAmmo);
    }

    protected abstract void Fire();
}
