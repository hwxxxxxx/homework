using UnityEngine;
using UnityEngine.AI;

public class EnemyAIController : MonoBehaviour
{
    public enum EnemyStateId
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

    private EnemyStateMachine stateMachine;
    private EnemyStateId currentStateId = EnemyStateId.Idle;
    private bool isDead;
    private float nextRepathTime;

    public EnemyStateId CurrentState => currentStateId;
    public Transform Target => target;
    public float DetectionRange => detectionRange;
    public float AttackRange => enemyCombat != null ? enemyCombat.AttackRange : 2f;
    public bool IsDead => isDead;

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

        stateMachine = new EnemyStateMachine();
        stateMachine.RegisterState(new EnemyIdleState(this));
        stateMachine.RegisterState(new EnemyChaseState(this));
        stateMachine.RegisterState(new EnemyAttackState(this));
        stateMachine.RegisterState(new EnemyDeadState(this));
        stateMachine.ChangeState(EnemyStateId.Idle);
    }

    private void Update()
    {
        stateMachine?.Tick();
    }

    public void SetDead()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        stateMachine?.ChangeState(EnemyStateId.Dead);
    }

    public void ResetAI()
    {
        isDead = false;
        nextRepathTime = 0f;

        if (navMeshAgent != null && !navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        if (enemyCombat != null)
        {
            enemyCombat.ResetCombat();
        }

        stateMachine?.ChangeState(EnemyStateId.Idle);
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

    public bool EnsureTarget()
    {
        if (target != null)
        {
            return true;
        }

        TryFindPlayerTarget();
        return target != null;
    }

    public bool IsTargetInAttackRange()
    {
        if (target == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, target.position) <= AttackRange;
    }

    public bool IsTargetInDetectionRange()
    {
        if (target == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, target.position) <= detectionRange;
    }

    public void MoveToTarget()
    {
        if (target == null || navMeshAgent == null || !navMeshAgent.enabled)
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

    public void StopMoving()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    public void SetAttackEnabled(bool value)
    {
        if (enemyCombat == null)
        {
            return;
        }

        enemyCombat.SetTarget(value ? target : null);
        enemyCombat.SetCanAttack(value);
    }

    public void FaceTarget()
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

    public void DisableOnDeath()
    {
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

    public void ChangeState(EnemyStateId newStateId)
    {
        if (stateMachine == null)
        {
            return;
        }

        stateMachine.ChangeState(newStateId);
    }

    public void SetCurrentStateId(EnemyStateId stateId)
    {
        currentStateId = stateId;
    }
}
