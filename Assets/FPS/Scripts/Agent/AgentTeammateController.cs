using UnityEngine;
using UnityEngine.AI;

public enum TeammateState
{
    Idle,
    Following,
    MovingTo,
    Attacking
}

[RequireComponent(typeof(NavMeshAgent))]
public class AgentTeammateController : MonoBehaviour
{
    private NavMeshAgent navAgent;
    public TeammateState currentState = TeammateState.Idle;
    private Transform currentTarget;

    [Tooltip("攻击行为的触发判定距离")]
    public float attackRange = 2.0f;
    [Tooltip("跟随玩家时的最小保持距离")]
    public float stopFollowDistance = 3.0f;

    private float attackCooldown = 1.0f;
    private float lastAttackTime;

    private void Awake()
    {
        navAgent = GetComponent<NavMeshAgent>();
    }

    private void OnEnable()
    {
        VoiceCommandManager.OnCommandParsed += HandleVoiceCommand;
    }

    private void OnDisable()
    {
        VoiceCommandManager.OnCommandParsed -= HandleVoiceCommand;
    }

    private void HandleVoiceCommand(string action, Transform target)
    {
        currentTarget = target;

        switch (action)
        {
            case "跟随":
                currentState = TeammateState.Following;
                break;
            case "移动":
                currentState = TeammateState.MovingTo;
                break;
            case "攻击":
                currentState = TeammateState.Attacking;
                break;
            default:
                currentState = TeammateState.Idle;
                break;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case TeammateState.Idle:
                navAgent.isStopped = true;
                break;

            case TeammateState.Following:
                if (currentTarget != null)
                {
                    navAgent.isStopped = false;
                    float dist = Vector3.Distance(transform.position, currentTarget.position);
                    if (dist > stopFollowDistance)
                    {
                        navAgent.SetDestination(currentTarget.position);
                    }
                    else
                    {
                        navAgent.ResetPath();
                    }
                }
                break;

            case TeammateState.MovingTo:
                if (currentTarget != null)
                {
                    navAgent.isStopped = false;
                    navAgent.SetDestination(currentTarget.position);

                    if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
                    {
                        if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                        {
                            currentState = TeammateState.Idle;
                        }
                    }
                }
                break;

            case TeammateState.Attacking:
                if (currentTarget != null)
                {
                    navAgent.isStopped = false;
                    float distToTarget = Vector3.Distance(transform.position, currentTarget.position);

                    if (distToTarget > attackRange)
                    {
                        navAgent.SetDestination(currentTarget.position);
                    }
                    else
                    {
                        navAgent.ResetPath();
                        ExecuteAttack();
                    }
                }
                else
                {
                    currentState = TeammateState.Idle;
                }
                break;
        }
    }

    private void ExecuteAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            // 尝试获取目标上的健康状态组件进行交互
            ObjectHealth targetHealth = currentTarget.GetComponent<ObjectHealth>();
            if (targetHealth != null)
            {
                transform.LookAt(new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z));
                targetHealth.TakeDamage(25); // 触发扣血
                Debug.Log("Agent对 " + currentTarget.name + " 造成了攻击！");
            }
            lastAttackTime = Time.time;
        }
    }
}