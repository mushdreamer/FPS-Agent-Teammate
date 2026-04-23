using UnityEngine;

public class AgentCommandTargetRegistry : MonoBehaviour
{
    [SerializeField] private AgentCommandTarget[] targets;

    private void Awake()
    {
        RefreshIfNeeded();
    }

    public AgentCommandTarget FindBestMatch(string normalizedTranscript)
    {
        RefreshIfNeeded();

        for (int i = 0; i < targets.Length; i++)
        {
            AgentCommandTarget target = targets[i];
            if (target != null && target.Matches(normalizedTranscript))
            {
                return target;
            }
        }

        return null;
    }

    [ContextMenu("Refresh Targets")]
    public void RefreshTargets()
    {
        targets = FindObjectsOfType<AgentCommandTarget>(true);
    }

    private void RefreshIfNeeded()
    {
        if (targets == null || targets.Length == 0)
        {
            RefreshTargets();
        }
    }
}
