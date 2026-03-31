using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ShockwaveKnockbackSkill : SkillBase
{
    [SerializeField] private float radius = 6f;
    [SerializeField] private float knockbackDistance = 4f;
    [SerializeField] private float knockbackDuration = 0.18f;
    [SerializeField] private LayerMask targetLayers = ~0;
    private readonly Dictionary<Transform, Coroutine> activeKnockbacks = new Dictionary<Transform, Coroutine>();

    protected override void Activate(GameObject owner)
    {
        Vector3 origin = owner.transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, radius, targetLayers, QueryTriggerInteraction.Ignore);
        HashSet<EnemyBase> affectedEnemies = new HashSet<EnemyBase>();

        for (int i = 0; i < hits.Length; i++)
        {
            EnemyBase enemy = hits[i].GetComponentInParent<EnemyBase>();
            if (enemy == null || !affectedEnemies.Add(enemy))
            {
                continue;
            }

            StartKnockback(enemy.transform, origin);
        }
    }

    private void OnDisable()
    {
        foreach (KeyValuePair<Transform, Coroutine> pair in activeKnockbacks)
        {
            if (pair.Value != null)
            {
                StopCoroutine(pair.Value);
            }
        }

        activeKnockbacks.Clear();
    }

    private void StartKnockback(Transform enemyTransform, Vector3 origin)
    {
        if (enemyTransform == null)
        {
            return;
        }

        if (activeKnockbacks.TryGetValue(enemyTransform, out Coroutine running) && running != null)
        {
            StopCoroutine(running);
        }

        Vector3 flatDirection = enemyTransform.position - origin;
        flatDirection.y = 0f;
        if (flatDirection.sqrMagnitude < 0.0001f)
        {
            flatDirection = enemyTransform.forward;
            flatDirection.y = 0f;
        }

        Coroutine routine = StartCoroutine(
            KnockbackRoutine(enemyTransform, flatDirection.normalized * knockbackDistance, knockbackDuration)
        );
        activeKnockbacks[enemyTransform] = routine;
    }

    private System.Collections.IEnumerator KnockbackRoutine(
        Transform enemyTransform,
        Vector3 totalDisplacement,
        float duration
    )
    {
        if (enemyTransform == null)
        {
            yield break;
        }

        float clampedDuration = Mathf.Max(0.05f, duration);
        Vector3 startPosition = enemyTransform.position;
        Vector3 previousPosition = startPosition;
        NavMeshAgent agent = enemyTransform.GetComponent<NavMeshAgent>();
        bool canUseAgentMove = agent != null && agent.enabled && agent.isOnNavMesh;
        if (canUseAgentMove)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }

        float elapsed = 0f;
        while (elapsed < clampedDuration)
        {
            if (enemyTransform == null)
            {
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / clampedDuration);
            float eased = 1f - (1f - t) * (1f - t);

            Vector3 desiredPosition = startPosition + totalDisplacement * eased;
            Vector3 frameDelta = desiredPosition - previousPosition;
            frameDelta.y = 0f;

            if (canUseAgentMove && agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.Move(frameDelta);
            }
            else
            {
                enemyTransform.position += frameDelta;
            }

            previousPosition += frameDelta;
            yield return null;
        }

        if (enemyTransform != null && canUseAgentMove && agent != null && agent.enabled && agent.isOnNavMesh)
        {
            Vector3 finalDelta = (startPosition + totalDisplacement) - previousPosition;
            finalDelta.y = 0f;
            agent.Move(finalDelta);
            agent.ResetPath();
        }

        if (enemyTransform != null)
        {
            activeKnockbacks.Remove(enemyTransform);
        }
    }
}
