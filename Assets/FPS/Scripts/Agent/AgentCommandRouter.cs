using UnityEngine;

public class AgentCommandRouter : MonoBehaviour
{
    [SerializeField] private AgentTeammateController teammate;

    private void Awake()
    {
        if (teammate == null)
        {
            teammate = FindObjectOfType<AgentTeammateController>();
        }
    }

    public void Route(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        string normalized = transcript.Trim().ToLowerInvariant();

        if (normalized.Contains("follow"))
        {
            teammate?.SetFollowMode(true);
            Debug.Log($"[AgentCommandRouter] Command '{transcript}' => FOLLOW");
            return;
        }

        Debug.Log($"[AgentCommandRouter] Unhandled command: {transcript}");
    }
}
