using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyCombat))]
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
    private NavMeshAgent navMeshAgent;
    private EnemyCombat enemyCombat;

    private EnemyStateMachine stateMachine;
    private EnemyStateId currentStateId = EnemyStateId.Idle;
    private bool isDead;
    private float nextRepathTime;
    private float detectionRange;
    private float repathInterval;
    private float faceTargetSpeed;

    public EnemyStateId CurrentState => currentStateId;
    public Transform Target => target;
    public float DetectionRange => detectionRange;
    public float AttackRange => enemyCombat.AttackRange;
    public bool IsDead => isDead;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyCombat = GetComponent<EnemyCombat>();

        stateMachine = new EnemyStateMachine();
        stateMachine.RegisterState(new EnemyIdleState(this));
        stateMachine.RegisterState(new EnemyChaseState(this));
        stateMachine.RegisterState(new EnemyAttackState(this));
        stateMachine.RegisterState(new EnemyDeadState(this));
        stateMachine.ChangeState(EnemyStateId.Idle);
    }

    private void Update()
    {
        stateMachine.Tick();
    }

    public void SetDead()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        stateMachine.ChangeState(EnemyStateId.Dead);
    }

    public void ResetAI()
    {
        isDead = false;
        nextRepathTime = 0f;

        if (!navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }

        if (CanOperateNavMeshAgent())
        {
            navMeshAgent.isStopped = true;
            navMeshAgent.ResetPath();
        }

        enemyCombat.ResetCombat();

        stateMachine.ChangeState(EnemyStateId.Idle);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void ApplyConfig(EnemyConfigAsset config)
    {
        detectionRange = config.DetectionRange;
        repathInterval = config.RepathInterval;
        faceTargetSpeed = config.FaceTargetSpeed;
    }

    public bool EnsureTarget()
    {
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
        if (target == null || !CanOperateNavMeshAgent())
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
        if (!CanOperateNavMeshAgent())
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    public void SetAttackEnabled(bool value)
    {
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
        enemyCombat.SetDead();

        if (navMeshAgent.enabled)
        {
            navMeshAgent.isStopped = true;
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.ResetPath();
            }

            navMeshAgent.enabled = false;
        }
    }

    private bool CanOperateNavMeshAgent()
    {
        return navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh;
    }

    public void ChangeState(EnemyStateId newStateId)
    {
        stateMachine.ChangeState(newStateId);
    }

    public void SetCurrentStateId(EnemyStateId stateId)
    {
        currentStateId = stateId;
    }
}
