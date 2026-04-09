using UnityEngine;

[CreateAssetMenu(menuName = "Game/Combat/Weapon Config", fileName = "WeaponConfig")]
public class WeaponConfigAsset : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string displayName;
    [SerializeField] private WeaponBase.WeaponKind weaponKind;

    [Header("Combat")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 5f;
    [SerializeField] private int magazineSize = 30;
    [SerializeField] private int reserveAmmo = 90;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("Ballistics")]
    [SerializeField] private float range = 100f;
    [SerializeField] private float projectileSpeed = 220f;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private float aimSpreadAngle = 0.1f;
    [SerializeField] private float hipFireSpreadAngle = 2.4f;

    [Header("Projectile")]
    [SerializeField] private float projectileLifetime = 4f;

    [Header("Effects")]
    [SerializeField] private float muzzleEffectLifetime = 1.2f;
    [SerializeField] private float impactEffectLifetime = 2f;

    [Header("Shotgun")]
    [SerializeField] private int pelletCount = 8;

    public string DisplayName => displayName;
    public WeaponBase.WeaponKind WeaponKind => weaponKind;
    public int Damage => damage;
    public float FireRate => fireRate;
    public int MagazineSize => magazineSize;
    public int ReserveAmmo => reserveAmmo;
    public float ReloadTime => reloadTime;
    public float Range => range;
    public float ProjectileSpeed => projectileSpeed;
    public LayerMask HitMask => hitMask;
    public float AimSpreadAngle => aimSpreadAngle;
    public float HipFireSpreadAngle => hipFireSpreadAngle;
    public float ProjectileLifetime => projectileLifetime;
    public float MuzzleEffectLifetime => muzzleEffectLifetime;
    public float ImpactEffectLifetime => impactEffectLifetime;
    public int PelletCount => pelletCount;
}
