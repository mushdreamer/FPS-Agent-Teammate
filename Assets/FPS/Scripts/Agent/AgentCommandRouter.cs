using System.Text;
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
        "stop", "hold", "stay", "停", "停下", "停止", "原地"
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
        Transform sceneNameTarget = explicitTarget == null ? FindSceneTargetByTranscript(normalized) : null;

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
            HandleAttackCommand(transcript, explicitTarget, sceneNameTarget);
            return;
        }

        if (ContainsAny(normalized, MoveKeywords) || explicitTarget != null || sceneNameTarget != null)
        {
            HandleMoveCommand(transcript, explicitTarget, sceneNameTarget);
            return;
        }

        Debug.Log($"[AgentCommandRouter] Unhandled command: {transcript}");
    }

    private void HandleMoveCommand(string transcript, AgentCommandTarget explicitTarget, Transform sceneNameTarget)
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

        if (sceneNameTarget != null)
        {
            teammate.MoveTo(sceneNameTarget.position, defaultMoveStopDistance);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => MOVE TO SCENE OBJECT '{sceneNameTarget.name}'");
            return;
        }

        if (TryGetMouseRayHit(out RaycastHit hitInfo))
        {
            teammate.MoveTo(hitInfo.point, defaultMoveStopDistance);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => MOVE TO MOUSE RAY {hitInfo.point}");
            return;
        }

        Debug.LogWarning($"[AgentCommandRouter] Move command had no explicit target and mouse raycast failed: {transcript}");
    }

    private void HandleAttackCommand(string transcript, AgentCommandTarget explicitTarget, Transform sceneNameTarget)
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

        if (sceneNameTarget != null)
        {
            teammate.StartAttacking(sceneNameTarget.position, sceneNameTarget);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => ATTACK SCENE OBJECT '{sceneNameTarget.name}'");
            return;
        }

        if (TryGetMouseRayHit(out RaycastHit hitInfo))
        {
            teammate.StartAttacking(hitInfo.point, hitInfo.transform);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => ATTACK MOUSE RAY {hitInfo.point}");
            return;
        }

        Debug.LogWarning($"[AgentCommandRouter] Attack command had no explicit target and mouse raycast failed: {transcript}");
    }

    private bool TryGetMouseRayHit(out RaycastHit hitInfo)
    {
        hitInfo = default;

        if (commandCamera == null)
        {
            return false;
        }

        Ray ray = commandCamera.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out hitInfo, mouseRayDistance, mouseRayMask, QueryTriggerInteraction.Ignore);
    }

    private Transform FindSceneTargetByTranscript(string normalizedTranscript)
    {
        string normalizedSpeech = SimplifyForNameMatch(normalizedTranscript);
        if (string.IsNullOrEmpty(normalizedSpeech))
        {
            return null;
        }

        Transform[] transforms = FindObjectsOfType<Transform>(true);
        for (int i = 0; i < transforms.Length; i++)
        {
            Transform candidate = transforms[i];
            if (candidate == null)
            {
                continue;
            }

            string candidateNormalized = SimplifyForNameMatch(candidate.name);
            if (string.IsNullOrEmpty(candidateNormalized))
            {
                continue;
            }

            if (normalizedSpeech.Contains(candidateNormalized) || candidateNormalized.Contains(normalizedSpeech))
            {
                return candidate;
            }
        }

        return null;
    }

    private static string SimplifyForNameMatch(string value)
    {
        StringBuilder sb = new StringBuilder(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char c = char.ToLowerInvariant(value[i]);
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
            else if (c == '_' || c == '-' || c == '(' || c == ')' || c == ' ')
            {
                continue;
            }
        }

        return sb.ToString();
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
