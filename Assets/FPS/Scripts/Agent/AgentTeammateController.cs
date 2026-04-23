using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentTeammateController : MonoBehaviour
{
    private enum AgentMoveMode
    {
        Idle,
        Follow,
        MoveToPoint
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

    [Header("Attack")]
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private float attackRange = 120f;
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float fireCooldown = 0.35f;
    [SerializeField] private string shootTrigger = "Shoot";
    [SerializeField] private LayerMask attackMask = ~0;

    private NavMeshAgent navMeshAgent;
    private AgentMoveMode moveMode;
    private Vector3 lastRequestedDestination;
    private float lastFireTime = -999f;

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
    }

    private void Update()
    {
        if (moveMode == AgentMoveMode.Follow)
        {
            UpdateFollowMovement();
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
        MoveToNavMesh(worldPosition);
    }

    public void SetIdleMode()
    {
        moveMode = AgentMoveMode.Idle;
        navMeshAgent.ResetPath();
    }

    public void AttackAt(Vector3 worldPosition)
    {
        if (Time.time - lastFireTime < fireCooldown)
        {
            return;
        }

        lastFireTime = Time.time;

        Vector3 shootStart = shootOrigin.position;
        Vector3 direction = (worldPosition - shootStart).normalized;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0f, direction.z));
        transform.rotation = lookRotation;

        if (characterAnimationController != null)
        {
            characterAnimationController.TriggerShoot();
        }
        else if (animator != null && !string.IsNullOrEmpty(shootTrigger))
        {
            animator.SetTrigger(shootTrigger);
        }

        if (Physics.Raycast(shootStart, direction, out RaycastHit hit, attackRange, attackMask, QueryTriggerInteraction.Ignore))
        {
            ObjectHealth objectHealth = hit.collider.GetComponentInParent<ObjectHealth>();
            if (objectHealth != null)
            {
                objectHealth.TakeDamage(attackDamage);
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

    private void UpdateAnimator()
    {
        float speed = navMeshAgent.velocity.magnitude;
        bool isRunning = speed > runThreshold;

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
