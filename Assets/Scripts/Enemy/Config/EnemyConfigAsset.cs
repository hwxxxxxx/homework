using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Enemy Config", fileName = "EnemyConfig")]
public class EnemyConfigAsset : ScriptableObject
{
    [Header("Vitals")]
    [SerializeField] private int maxHealth = 100;

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

    [Header("Death Visual")]
    [SerializeField] private float deathDissolveDuration = 1.2f;
    [SerializeField] private float deathDissolveEdgeWidth = 0.15f;
    [SerializeField] private Color deathDissolveEdgeColor = new Color(1f, 0.5f, 0.2f, 1f);
    [SerializeField] private float deathDissolveNoiseScale = 2f;

    public int MaxHealth => maxHealth;
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
    public float DeathDissolveDuration => deathDissolveDuration;
    public float DeathDissolveEdgeWidth => deathDissolveEdgeWidth;
    public Color DeathDissolveEdgeColor => deathDissolveEdgeColor;
    public float DeathDissolveNoiseScale => deathDissolveNoiseScale;
}
