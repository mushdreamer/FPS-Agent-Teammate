using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentTeammateController : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float repathDistance = 1.2f;
    [SerializeField] private float followStopDistance = 2.5f;

    private NavMeshAgent navMeshAgent;
    private bool followMode;
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
        if (!followMode || followTarget == null)
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

    public void SetFollowMode(bool enabled)
    {
        followMode = enabled;

        if (!enabled)
        {
            navMeshAgent.ResetPath();
            return;
        }

        if (followTarget != null)
        {
            lastRequestedDestination = followTarget.position;
            navMeshAgent.SetDestination(lastRequestedDestination);
        }
    }
}
