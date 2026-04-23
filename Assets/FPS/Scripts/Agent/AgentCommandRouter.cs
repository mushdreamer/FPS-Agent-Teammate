using UnityEngine;

public class AgentCommandRouter : MonoBehaviour
{
    [SerializeField] private AgentTeammateController teammate;
    [SerializeField] private AgentCommandTargetRegistry targetRegistry;
    [SerializeField] private Camera commandCamera;
    [SerializeField] private float mouseRayDistance = 200f;
    [SerializeField] private LayerMask mouseRayMask = ~0;
    [SerializeField] private float defaultMoveStopDistance = 1.2f;

    private static readonly string[] MoveKeywords =
    {
        "go to", "move to", "goto", "go", "move", "去", "前往", "移动到", "移动"
    };

    private static readonly string[] AttackKeywords =
    {
        "shoot", "fire", "attack", "开火", "射击", "攻击"
    };

    private static readonly string[] StopKeywords =
    {
        "stop", "hold", "stay", "停", "停止", "原地"
    };

    private void Awake()
    {
        if (teammate == null)
        {
            teammate = FindObjectOfType<AgentTeammateController>();
        }

        if (targetRegistry == null)
        {
            targetRegistry = FindObjectOfType<AgentCommandTargetRegistry>();
        }

        if (commandCamera == null)
        {
            commandCamera = Camera.main;
        }
    }

    public void Route(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        string normalized = transcript.Trim().ToLowerInvariant();
        AgentCommandTarget explicitTarget = targetRegistry == null ? null : targetRegistry.FindBestMatch(normalized);

        if (normalized.Contains("follow") || normalized.Contains("跟随"))
        {
            teammate?.SetFollowMode(true);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => FOLLOW");
            return;
        }

        if (ContainsAny(normalized, StopKeywords))
        {
            teammate?.SetIdleMode();
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => STOP");
            return;
        }

        if (ContainsAny(normalized, AttackKeywords))
        {
            HandleAttackCommand(transcript, explicitTarget);
            return;
        }

        if (ContainsAny(normalized, MoveKeywords) || explicitTarget != null)
        {
            HandleMoveCommand(transcript, explicitTarget);
            return;
        }

        Debug.Log($"[AgentCommandRouter] Unhandled command: {transcript}");
    }

    private void HandleMoveCommand(string transcript, AgentCommandTarget explicitTarget)
    {
        if (teammate == null)
        {
            Debug.LogWarning("[AgentCommandRouter] Teammate is missing.");
            return;
        }

        if (explicitTarget != null)
        {
            teammate.MoveTo(explicitTarget.WorldPosition, explicitTarget.StopDistance);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => MOVE TO '{explicitTarget.GetDisplayName()}'");
            return;
        }

        if (TryGetMouseRayDestination(out Vector3 destination))
        {
            teammate.MoveTo(destination, defaultMoveStopDistance);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => MOVE TO MOUSE RAY {destination}");
            return;
        }

        Debug.LogWarning($"[AgentCommandRouter] Move command had no explicit target and mouse raycast failed: {transcript}");
    }

    private void HandleAttackCommand(string transcript, AgentCommandTarget explicitTarget)
    {
        if (teammate == null)
        {
            Debug.LogWarning("[AgentCommandRouter] Teammate is missing.");
            return;
        }

        if (explicitTarget != null)
        {
            teammate.StartAttacking(explicitTarget.WorldPosition, explicitTarget.transform);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => ATTACK '{explicitTarget.GetDisplayName()}'");
            return;
        }

        if (TryGetMouseRayDestination(out Vector3 destination))
        {
            teammate.StartAttacking(destination);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => ATTACK MOUSE RAY {destination}");
            return;
        }

        Debug.LogWarning($"[AgentCommandRouter] Attack command had no explicit target and mouse raycast failed: {transcript}");
    }

    private bool TryGetMouseRayDestination(out Vector3 destination)
    {
        destination = Vector3.zero;

        if (commandCamera == null)
        {
            return false;
        }

        Ray ray = commandCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, mouseRayDistance, mouseRayMask, QueryTriggerInteraction.Ignore))
        {
            destination = hitInfo.point;
            return true;
        }

        return false;
    }

    private static bool ContainsAny(string content, string[] keywords)
    {
        for (int i = 0; i < keywords.Length; i++)
        {
            if (content.Contains(keywords[i]))
            {
                return true;
            }
        }

        return false;
    }
}
