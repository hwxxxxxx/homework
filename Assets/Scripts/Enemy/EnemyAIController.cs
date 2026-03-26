using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Dead
    }

    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private EnemyCombat enemyCombat;

    [Header("AI")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float repathInterval = 0.1f;
    [SerializeField] private float faceTargetSpeed = 10f;

    private EnemyState currentState = EnemyState.Idle;
    private bool isDead;
    private float nextRepathTime;

    public EnemyState CurrentState => currentState;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        if (enemyCombat == null)
        {
            enemyCombat = GetComponent<EnemyCombat>();
        }
    }

    private void Update()
    {
        if (isDead)
        {
            return;
        }

        if (target == null)
        {
            TryFindPlayerTarget();
        }

        if (target == null)
        {
            EnterIdle();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        float attackRange = enemyCombat != null ? enemyCombat.AttackRange : 2f;

        if (distanceToTarget <= attackRange)
        {
            EnterAttack();
            return;
        }

        if (distanceToTarget <= detectionRange)
        {
            EnterChase();
            return;
        }

        EnterIdle();
    }

    public void SetDead()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentState = EnemyState.Dead;

        if (enemyCombat != null)
        {
            enemyCombat.SetDead();
        }

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    private void TryFindPlayerTarget()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            target = playerObject.transform;
        }
    }

    private void EnterIdle()
    {
        currentState = EnemyState.Idle;

        if (enemyCombat != null)
        {
            enemyCombat.SetCanAttack(false);
            enemyCombat.SetTarget(null);
        }

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }
    }

    private void EnterChase()
    {
        currentState = EnemyState.Chase;

        if (enemyCombat != null)
        {
            enemyCombat.SetCanAttack(false);
            enemyCombat.SetTarget(target);
        }

        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            return;
        }

        navMeshAgent.isStopped = false;
        if (Time.time >= nextRepathTime)
        {
            navMeshAgent.SetDestination(target.position);
            nextRepathTime = Time.time + repathInterval;
        }
    }

    private void EnterAttack()
    {
        currentState = EnemyState.Attack;

        if (enemyCombat != null)
        {
            enemyCombat.SetTarget(target);
            enemyCombat.SetCanAttack(true);
        }

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        FaceTarget();
    }

    private void FaceTarget()
    {
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            faceTargetSpeed * Time.deltaTime
        );
    }
}
