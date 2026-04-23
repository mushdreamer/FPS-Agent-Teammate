using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentTeammateController : MonoBehaviour
{
    private enum AgentMoveMode
    {
        Idle,
        Follow,
        MoveToPoint,
        Attack
    }

    [Header("Movement")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private float repathDistance = 1.2f;
    [SerializeField] private float followStopDistance = 2.5f;
    [SerializeField] private float navMeshSampleDistance = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterAnimationController characterAnimationController;
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string runParam = "IsRunning";
    [SerializeField] private float runThreshold = 0.2f;
    [SerializeField] private bool forcePlayShootState = true;
    [SerializeField] private string shootStateName = "demo_combat_shoot";

    [Header("Attack")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private float attackRange = 120f;
    [SerializeField] private int fallbackAttackDamage = 20;
    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private float attackMovePauseSeconds = 0.15f;
    [SerializeField] private string shootTrigger = "Shoot";
    [SerializeField] private LayerMask attackMask = ~0;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject hitImpactEffect;
    [SerializeField] private WeaponDamageTable damageTable;
    [SerializeField] private WeaponType teammateWeaponType = WeaponType.Auto;

    private NavMeshAgent navMeshAgent;
    private AgentMoveMode moveMode;
    private Vector3 lastRequestedDestination;
    private float lastFireTime = -999f;
    private float movementResumeTime;

    private Vector3 attackPoint;
    private Transform attackTargetTransform;
    private bool usingExplicitAttackTarget;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = followStopDistance;
        navMeshAgent.autoRepath = true;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (characterAnimationController == null)
        {
            characterAnimationController = GetComponentInChildren<CharacterAnimationController>();
        }

        if (characterAnimationController != null)
        {
            characterAnimationController.SetUsePlayerInput(false);
        }

        if (shootOrigin == null)
        {
            shootOrigin = transform;
        }

        if (followTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                followTarget = player.transform;
            }
        }

        if (damageTable == null)
        {
            damageTable = FindObjectOfType<WeaponDamageTable>();
        }

        if (damageTable != null)
        {
            damageTable.EnsureInitialized();
        }
    }

    private void Update()
    {
        navMeshAgent.isStopped = Time.time < movementResumeTime || moveMode == AgentMoveMode.Attack;

        if (moveMode == AgentMoveMode.Follow && !navMeshAgent.isStopped)
        {
            UpdateFollowMovement();
        }
        else if (moveMode == AgentMoveMode.Attack)
        {
            UpdateAttackLoop();
        }

        UpdateAnimator();
    }

    public void SetFollowMode(bool enabled)
    {
        if (!enabled)
        {
            SetIdleMode();
            return;
        }

        moveMode = AgentMoveMode.Follow;
        navMeshAgent.stoppingDistance = followStopDistance;
        ClearAttackTarget();

        if (followTarget != null)
        {
            lastRequestedDestination = followTarget.position;
            MoveToNavMesh(lastRequestedDestination);
        }
    }

    public void MoveTo(Vector3 worldPosition, float stopDistance)
    {
        moveMode = AgentMoveMode.MoveToPoint;
        navMeshAgent.stoppingDistance = Mathf.Max(0.1f, stopDistance);
        lastRequestedDestination = worldPosition;
        ClearAttackTarget();
        MoveToNavMesh(worldPosition);
    }

    public void StartAttacking(Vector3 worldPosition, Transform explicitTarget = null)
    {
        moveMode = AgentMoveMode.Attack;
        attackPoint = worldPosition;
        attackTargetTransform = explicitTarget;
        usingExplicitAttackTarget = explicitTarget != null;
        navMeshAgent.ResetPath();
        movementResumeTime = Time.time + attackMovePauseSeconds;

        AttackAt(attackPoint);
    }

    public void SetIdleMode()
    {
        moveMode = AgentMoveMode.Idle;
        movementResumeTime = 0f;
        ClearAttackTarget();
        navMeshAgent.ResetPath();
    }

    public void AttackAt(Vector3 worldPosition)
    {
        if (Time.time - lastFireTime < fireCooldown)
        {
            return;
        }

        lastFireTime = Time.time;
        movementResumeTime = Time.time + attackMovePauseSeconds;

        Vector3 shootStart = shootOrigin.position;
        Vector3 direction = (worldPosition - shootStart).normalized;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = lookRotation;

        TriggerShootAnimation();

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (Physics.Raycast(shootStart, direction, out RaycastHit hit, attackRange, attackMask, QueryTriggerInteraction.Ignore))
        {
            if (hitImpactEffect != null)
            {
                Instantiate(hitImpactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

            ObjectHealth objectHealth = hit.collider.GetComponentInParent<ObjectHealth>();
            if (objectHealth != null)
            {
                int damage = fallbackAttackDamage;

                if (damageTable != null)
                {
                    damage = damageTable.GetDamage(teammateWeaponType, objectHealth.materialType);
                    if (damage <= 0)
                    {
                        damage = fallbackAttackDamage;
                    }
                }

                objectHealth.TakeDamage(damage);
                Debug.Log($"[AgentTeammateController] Hit {hit.collider.name}, material={objectHealth.materialType}, damage={damage}");
            }
            else
            {
                Debug.Log($"[AgentTeammateController] Hit {hit.collider.name}, but no ObjectHealth found.");
            }
        }
        else
        {
            Debug.Log("[AgentTeammateController] Attack raycast did not hit anything.");
        }
    }

    private void UpdateAttackLoop()
    {
        if (attackTargetTransform != null)
        {
            attackPoint = attackTargetTransform.position;
        }
        else if (usingExplicitAttackTarget)
        {
            Debug.Log("[AgentTeammateController] Explicit attack target destroyed, stopping attack loop.");
            SetIdleMode();
            return;
        }

        AttackAt(attackPoint);
    }

    private void TriggerShootAnimation()
    {
        if (characterAnimationController != null)
        {
            characterAnimationController.TriggerShoot();

            if (forcePlayShootState)
            {
                characterAnimationController.ForcePlayState(shootStateName);
            }

            return;
        }

        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(shootTrigger))
        {
            animator.SetTrigger(shootTrigger);
        }

        if (forcePlayShootState)
        {
            int stateHash = Animator.StringToHash(shootStateName);
            if (animator.HasState(0, stateHash))
            {
                animator.CrossFadeInFixedTime(stateHash, 0.02f, 0);
            }
        }
    }

    private void UpdateFollowMovement()
    {
        if (followTarget == null)
        {
            return;
        }

        Vector3 targetPosition = followTarget.position;
        float delta = Vector3.Distance(lastRequestedDestination, targetPosition);

        if (delta >= repathDistance)
        {
            MoveToNavMesh(targetPosition);
            lastRequestedDestination = targetPosition;
        }
    }

    private void MoveToNavMesh(Vector3 targetPosition)
    {
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navMeshHit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            navMeshAgent.SetDestination(navMeshHit.position);
            return;
        }

        Debug.LogWarning($"[AgentTeammateController] No valid NavMesh destination near {targetPosition}");
    }

    private void ClearAttackTarget()
    {
        attackTargetTransform = null;
        usingExplicitAttackTarget = false;
    }

    private void UpdateAnimator()
    {
        float speed = navMeshAgent.velocity.magnitude;
        bool movingByPath = navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance + 0.05f;
        bool isRunning = speed > runThreshold || movingByPath;

        if (characterAnimationController != null)
        {
            characterAnimationController.SetRunning(isRunning);
        }

        if (animator == null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(speedParam))
        {
            animator.SetFloat(speedParam, speed);
        }

        if (!string.IsNullOrEmpty(runParam))
        {
            animator.SetBool(runParam, isRunning);
        }
    }
}
