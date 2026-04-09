using System.Collections;
using UnityEngine;

public class BossSkillController : MonoBehaviour
{
    [SerializeField] private BossSkillAsset[] skills;
    [SerializeField] private float thinkInterval = 0.1f;

    private Coroutine castingCoroutine;
    private float nextThinkTime;
    private bool isCasting;

    public bool IsCasting => isCasting;

    public void Tick(BossEnemyController caster, Transform target)
    {
        if (isCasting || caster == null || target == null || Time.time < nextThinkTime)
        {
            return;
        }

        nextThinkTime = Time.time + thinkInterval;
        BossSkillAsset selectedSkill = SelectSkill(caster, target);
        if (selectedSkill == null)
        {
            return;
        }

        castingCoroutine = StartCoroutine(CastRoutine(selectedSkill, caster, target));
    }

    public void ResetState()
    {
        if (castingCoroutine != null)
        {
            StopCoroutine(castingCoroutine);
            castingCoroutine = null;
        }

        nextThinkTime = 0f;
        isCasting = false;
    }

    public void Configure(BossSkillAsset[] configuredSkills, float configuredThinkInterval)
    {
        skills = configuredSkills;
        thinkInterval = configuredThinkInterval;
    }

    private BossSkillAsset SelectSkill(BossEnemyController caster, Transform target)
    {
        if (skills == null || skills.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < skills.Length; i++)
        {
            BossSkillAsset skill = skills[i];
            if (skill == null)
            {
                continue;
            }

            if (!skill.CanCast(caster, target))
            {
                continue;
            }

            return skill;
        }

        return null;
    }

    private IEnumerator CastRoutine(BossSkillAsset skill, BossEnemyController caster, Transform target)
    {
        isCasting = true;
        caster.BeginSkillCast();
        yield return skill.Cast(caster, target);
        caster.EndSkillCast();
        isCasting = false;
        castingCoroutine = null;
        skill.StartCooldown(caster);
    }
}
