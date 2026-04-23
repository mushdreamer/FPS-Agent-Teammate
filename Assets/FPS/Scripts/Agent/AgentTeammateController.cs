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

    [SerializeField] private Transform followTarget;
    [SerializeField] private float repathDistance = 1.2f;
    [SerializeField] private float followStopDistance = 2.5f;

    private NavMeshAgent navMeshAgent;
    private AgentMoveMode moveMode;
    private Vector3 lastRequestedDestination;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.stoppingDistance = followStopDistance;

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
            navMeshAgent.SetDestination(lastRequestedDestination);
        }
    }

    public void MoveTo(Vector3 worldPosition, float stopDistance)
    {
        moveMode = AgentMoveMode.MoveToPoint;
        navMeshAgent.stoppingDistance = Mathf.Max(0.1f, stopDistance);
        lastRequestedDestination = worldPosition;
        navMeshAgent.SetDestination(worldPosition);
    }

    public void SetIdleMode()
    {
        moveMode = AgentMoveMode.Idle;
        navMeshAgent.ResetPath();
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
            navMeshAgent.SetDestination(targetPosition);
            lastRequestedDestination = targetPosition;
        }
    }
}
