using System.Collections;
using UnityEngine;

public abstract class BossSkillAsset : ScriptableObject
{
    [SerializeField] private string skillId;
    [SerializeField] private EffectAsset cooldownEffect;
    [SerializeField] private float minTriggerDistance = 0f;
    [SerializeField] private float maxTriggerDistance = 8f;

    public string SkillId => skillId;

    public virtual bool CanCast(BossEnemyController caster, Transform target)
    {
        if (caster == null || target == null)
        {
            return false;
        }

        if (!IsReady(caster))
        {
            return false;
        }

        float distance = Vector3.Distance(caster.transform.position, target.position);
        return distance >= minTriggerDistance && distance <= maxTriggerDistance;
    }

    public void StartCooldown(BossEnemyController caster)
    {
        if (caster == null || cooldownEffect == null || caster.EffectController == null)
        {
            return;
        }

        caster.EffectController.ApplyEffect(cooldownEffect, caster.gameObject);
    }

    private bool IsReady(BossEnemyController caster)
    {
        if (cooldownEffect == null)
        {
            return true;
        }

        return caster != null &&
            caster.EffectController != null &&
            !caster.EffectController.HasEffect(cooldownEffect.EffectId);
    }

    public abstract IEnumerator Cast(BossEnemyController caster, Transform target);
}
