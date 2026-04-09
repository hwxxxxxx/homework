using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(menuName = "Game/Skills/Boss/Jump Slam", fileName = "BossJumpSlamSkill")]
public class BossJumpSlamSkillAsset : BossSkillAsset
{
    [Header("Jump Motion")]
    [SerializeField] private float jumpDuration = 0.7f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float landingOffsetFromTarget = 0.5f;

    [Header("Impact")]
    [SerializeField] private float impactRadius = 4.5f;
    [SerializeField] private int impactDamage = 24;
    [SerializeField] private LayerMask impactLayers = ~0;

    public override IEnumerator Cast(BossEnemyController caster, Transform target)
    {
        if (caster == null || target == null)
        {
            yield break;
        }

        Transform casterTransform = caster.transform;
        Vector3 startPosition = casterTransform.position;
        Vector3 landingPosition = target.position;
        landingPosition.y = startPosition.y;

        Vector3 toTarget = landingPosition - startPosition;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude > 0.001f && landingOffsetFromTarget > 0f)
        {
            landingPosition -= toTarget.normalized * landingOffsetFromTarget;
            landingPosition.y = startPosition.y;
        }

        NavMeshAgent agent = caster.Agent;
        bool agentWasEnabled = agent != null && agent.enabled;
        bool shouldRestoreAgent = false;

        if (agentWasEnabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
            shouldRestoreAgent = true;
        }

        float duration = jumpDuration;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 position = Vector3.Lerp(startPosition, landingPosition, t);
            position.y = Mathf.Lerp(startPosition.y, landingPosition.y, t) + 4f * jumpHeight * t * (1f - t);
            casterTransform.position = position;

            yield return null;
        }

        casterTransform.position = landingPosition;
        ApplyImpact(casterTransform.position, caster.gameObject);

        if (shouldRestoreAgent && agent != null)
        {
            agent.enabled = true;
            if (agent.isOnNavMesh)
            {
                agent.Warp(casterTransform.position);
            }
        }
    }

    private void ApplyImpact(Vector3 center, GameObject owner)
    {
        Collider[] hits = Physics.OverlapSphere(center, impactRadius, impactLayers, QueryTriggerInteraction.Ignore);
        HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

        for (int i = 0; i < hits.Length; i++)
        {
            Collider hit = hits[i];
            if (hit == null)
            {
                continue;
            }

            if (!CombatTargetResolver.TryResolveDamageable(hit, out IDamageable damageable))
            {
                continue;
            }

            Component component = damageable as Component;
            if (component != null &&
                (component.gameObject == owner || component.transform.IsChildOf(owner.transform)))
            {
                continue;
            }

            if (!damagedTargets.Add(damageable))
            {
                continue;
            }

            damageable.TakeDamage(impactDamage);
        }
    }
}
