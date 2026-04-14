using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Enemy Config", fileName = "EnemyConfig")]
public class EnemyConfigAsset : ScriptableObject
{
    public enum EnemyAttackMode
    {
        Melee = 0,
        RangedProjectile = 1
    }

    [Header("Vitals")]
    [SerializeField] private int maxHealth = 100;

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private EnemyAttackMode attackMode = EnemyAttackMode.Melee;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [SerializeField] private float attackOriginHeightOffset = 0.5f;
    [SerializeField] private float targetHeightOffset = 1f;
    [SerializeField] private EnemyProjectile projectilePrefab;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float projectileLifetime = 3f;
    [SerializeField] private LayerMask projectileHitMask = ~0;
    [SerializeField] private float projectileSpawnForwardOffset = 0.8f;
    [SerializeField] private float projectileSpawnHeightOffset = 0.1f;

    [Header("AI")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float faceTargetSpeed = 10f;

    [Header("Death Visual")]
    [SerializeField] private float deathDissolveDuration = 1.2f;
    [SerializeField] private float deathDissolveEdgeWidth = 0.15f;
    [SerializeField] private Color deathDissolveEdgeColor = new Color(1f, 0.5f, 0.2f, 1f);
    [SerializeField] private float deathDissolveNoiseScale = 2f;

    public int MaxHealth => maxHealth;
    public int AttackDamage => attackDamage;
    public float AttackInterval => attackInterval;
    public EnemyAttackMode AttackMode => attackMode;
    public float AttackRange => attackRange;
    public bool RequireLineOfSight => requireLineOfSight;
    public LayerMask LineOfSightMask => lineOfSightMask;
    public float AttackOriginHeightOffset => attackOriginHeightOffset;
    public float TargetHeightOffset => targetHeightOffset;
    public EnemyProjectile ProjectilePrefab => projectilePrefab;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifetime => projectileLifetime;
    public LayerMask ProjectileHitMask => projectileHitMask;
    public float ProjectileSpawnForwardOffset => projectileSpawnForwardOffset;
    public float ProjectileSpawnHeightOffset => projectileSpawnHeightOffset;
    public float DetectionRange => detectionRange;
    public float RepathInterval => repathInterval;
    public float FaceTargetSpeed => faceTargetSpeed;
    public float DeathDissolveDuration => deathDissolveDuration;
    public float DeathDissolveEdgeWidth => deathDissolveEdgeWidth;
    public Color DeathDissolveEdgeColor => deathDissolveEdgeColor;
    public float DeathDissolveNoiseScale => deathDissolveNoiseScale;
}
