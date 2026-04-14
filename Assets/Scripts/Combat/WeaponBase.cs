using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IModifiableStatProvider
{
    public enum WeaponKind
    {
        Rifle = 0,
        Shotgun = 1,
        RocketLauncher = 2
    }

    [Header("Weapon Settings")]
    [SerializeField] protected WeaponConfigAsset weaponConfig;

    protected float lastFireTime;
    protected int currentAmmoInMagazine;
    protected int reserveAmmo;
    protected bool isReloading;
    protected PlayerCombat ownerCombat;
    private readonly Dictionary<string, StatValue> weaponStats = new Dictionary<string, StatValue>();

    public event Action<int, int> OnAmmoChanged;
    public event Action OnFired;

    protected virtual void Awake()
    {
        RebuildWeaponStats();
        currentAmmoInMagazine = weaponConfig.InitialAmmoInMagazine;
        reserveAmmo = weaponConfig.ReserveAmmo;
        NotifyAmmoChanged();
    }

    public virtual bool CanFire()
    {
        if (isReloading)
        {
            return false;
        }

        if (currentAmmoInMagazine <= 0)
        {
            return false;
        }

        float currentFireRate = GetFireRateValue();
        return Time.time >= lastFireTime + 1f / currentFireRate;
    }

    public virtual void TryFire()
    {
        if (!CanFire())
        {
            return;
        }

        Fire();
        currentAmmoInMagazine--;
        lastFireTime = Time.time;
        OnFired?.Invoke();
        EventBus.Publish(new WeaponFiredEvent(transform.position));

        NotifyAmmoChanged();
    }

    public virtual void StartReload(MonoBehaviour runner)
    {
        if (isReloading) return;
        if (currentAmmoInMagazine >= weaponConfig.MagazineSize) return;
        if (reserveAmmo <= 0) return;

        EventBus.Publish(new WeaponReloadStartedEvent(gameObject, transform.position));
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

    public void AddReserveAmmo(int amount)
    {
        reserveAmmo += amount;
        NotifyAmmoChanged();
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

    public float GetBaseDamageValue()
    {
        return weaponConfig != null ? weaponConfig.Damage : 0f;
    }

    public float GetBaseFireRateValue()
    {
        return weaponConfig != null ? weaponConfig.FireRate : 0f;
    }

    public float GetBaseReloadTimeValue()
    {
        return weaponConfig != null ? weaponConfig.ReloadTime : 0f;
    }

    public float GetCurrentDamageValue()
    {
        return GetDamageValue();
    }

    public float GetCurrentFireRateValue()
    {
        return GetFireRateValue();
    }

    public float GetCurrentReloadTimeValue()
    {
        return GetReloadTimeValue();
    }

    public virtual void BindOwner(PlayerCombat owner)
    {
        ownerCombat = owner;

        RebuildWeaponStats();
    }

    protected int GetDamageValue()
    {
        return Mathf.RoundToInt(GetStatValue(StatIds.WeaponDamage));
    }

    protected float GetFireRateValue()
    {
        return Mathf.Max(0.01f, GetStatValue(StatIds.WeaponFireRate));
    }

    protected float GetReloadTimeValue()
    {
        return Mathf.Max(0.05f, GetStatValue(StatIds.WeaponReloadTime));
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

    protected void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(currentAmmoInMagazine, reserveAmmo);
    }

    public bool HasStat(string statId)
    {
        return !string.IsNullOrWhiteSpace(statId) && weaponStats.ContainsKey(statId);
    }

    public float GetStatValue(string statId)
    {
        if (!weaponStats.TryGetValue(statId, out StatValue stat))
        {
            return 0f;
        }

        return stat.CurrentValue;
    }

    public string AddModifier(string statId, StatModifier modifier)
    {
        if (string.IsNullOrWhiteSpace(statId))
        {
            return null;
        }

        if (!weaponStats.TryGetValue(statId, out StatValue stat))
        {
            stat = new StatValue(0f);
            weaponStats[statId] = stat;
        }

        stat.AddModifier(modifier);
        return modifier.ModifierId;
    }

    public bool RemoveModifier(string statId, string modifierId)
    {
        if (!weaponStats.TryGetValue(statId, out StatValue stat))
        {
            return false;
        }

        return stat.RemoveModifier(modifierId);
    }

    public int RemoveModifiersBySource(object source)
    {
        int totalRemoved = 0;
        List<string> keys = new List<string>(weaponStats.Keys);
        for (int i = 0; i < keys.Count; i++)
        {
            totalRemoved += weaponStats[keys[i]].RemoveModifiersBySource(source);
        }

        return totalRemoved;
    }

    public void SetBaseValue(string statId, float baseValue)
    {
        if (string.IsNullOrWhiteSpace(statId))
        {
            return;
        }

        if (!weaponStats.TryGetValue(statId, out StatValue stat))
        {
            weaponStats[statId] = new StatValue(baseValue);
            return;
        }

        stat.SetBaseValue(baseValue);
    }

    private void RebuildWeaponStats()
    {
        if (weaponConfig == null)
        {
            return;
        }

        SetBaseValue(StatIds.WeaponDamage, weaponConfig.Damage);
        SetBaseValue(StatIds.WeaponFireRate, weaponConfig.FireRate);
        SetBaseValue(StatIds.WeaponReloadTime, weaponConfig.ReloadTime);
    }

    protected abstract void Fire();
}
