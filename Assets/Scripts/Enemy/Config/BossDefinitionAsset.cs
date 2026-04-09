using UnityEngine;

[CreateAssetMenu(menuName = "Game/Enemy/Boss Definition", fileName = "BossDefinition")]
public class BossDefinitionAsset : ScriptableObject
{
    [Header("Identity")]
    public bool applyBossScale = true;
    public Vector3 bossScale = new Vector3(2.3f, 2.6f, 2.3f);

    [Header("Base Stats")]
    public float maxHealth = 1200f;
    public float attackDamage = 22f;
    public float attackInterval = 0.85f;
    public float moveSpeed = 4.5f;

    [Header("Enrage")]
    [Range(0.05f, 1f)] public float enrageHealthThresholdRatio = 0.35f;
    public float enrageDamagePercentAdd = 0.4f;
    public float enrageAttackSpeedPercentAdd = 0.25f;
    public float enrageMoveSpeedPercentAdd = 0.2f;
    public string enrageDamageModifierId = "boss.enrage.damage";
    public string enrageAttackIntervalModifierId = "boss.enrage.interval";
    public int enrageDamageModifierOrder = 100;
    public int enrageAttackIntervalModifierOrder = 100;

    [Header("Skills")]
    public BossSkillAsset[] skills;
    public float skillThinkInterval = 0.1f;
}
