using UnityEngine;

public class AgentCommandRouter : MonoBehaviour
{
    [SerializeField] private AgentTeammateController teammate;
    [SerializeField] private AgentCommandTargetRegistry targetRegistry;

    private static readonly string[] MoveKeywords =
    {
        "go to", "move to", "goto", "去", "前往", "移动到", "move"
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
    }

    public void Route(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        string normalized = transcript.Trim().ToLowerInvariant();

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

        if (ContainsAny(normalized, MoveKeywords))
        {
            HandleMoveCommand(transcript, normalized);
            return;
        }

        Debug.Log($"[AgentCommandRouter] Unhandled command: {transcript}");
    }

    private void HandleMoveCommand(string transcript, string normalized)
    {
        if (targetRegistry == null)
        {
            Debug.LogWarning("[AgentCommandRouter] Target registry is missing.");
            return;
        }

        AgentCommandTarget target = targetRegistry.FindBestMatch(normalized);
        if (target == null)
        {
            Debug.LogWarning($"[AgentCommandRouter] Move command has no target match: {transcript}");
            return;
        }

        teammate?.MoveTo(target.WorldPosition, target.StopDistance);
        Debug.Log($"[AgentCommandRouter] Command '{transcript}' => MOVE TO '{target.GetDisplayName()}'");
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
