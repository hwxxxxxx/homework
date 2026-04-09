using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Enemy Config", fileName = "EnemyConfig")]
public class EnemyConfigAsset : ScriptableObject
{
    [Header("Vitals")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float despawnDelayOnDeath = 2f;

    [Header("Combat")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackInterval = 1f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private bool requireLineOfSight;
    [SerializeField] private LayerMask lineOfSightMask = ~0;
    [SerializeField] private float attackOriginHeightOffset = 0.5f;
    [SerializeField] private float targetHeightOffset = 1f;

    [Header("AI")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float faceTargetSpeed = 10f;

    public int MaxHealth => maxHealth;
    public float DespawnDelayOnDeath => despawnDelayOnDeath;
    public int AttackDamage => attackDamage;
    public float AttackInterval => attackInterval;
    public float AttackRange => attackRange;
    public bool RequireLineOfSight => requireLineOfSight;
    public LayerMask LineOfSightMask => lineOfSightMask;
    public float AttackOriginHeightOffset => attackOriginHeightOffset;
    public float TargetHeightOffset => targetHeightOffset;
    public float DetectionRange => detectionRange;
    public float RepathInterval => repathInterval;
    public float FaceTargetSpeed => faceTargetSpeed;
}
