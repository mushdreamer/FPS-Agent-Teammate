using UnityEngine;

public class AgentCommandTarget : MonoBehaviour
{
    [Tooltip("用于语音指令匹配的唯一主名称，例如: wall_a_front / room_outside。")]
    [SerializeField] private string commandId = "target";

    [Tooltip("可选别名，会一并参与匹配，例如: 外侧区域, outside area。")]
    [SerializeField] private string[] aliases;

    [Tooltip("如果指定，将移动到该点；否则移动到当前物体位置。")]
    [SerializeField] private Transform approachPoint;

    [Tooltip("到达该目标时的停止距离。")]
    [SerializeField] private float stopDistance = 1.2f;

    public Vector3 WorldPosition => approachPoint != null ? approachPoint.position : transform.position;
    public float StopDistance => Mathf.Max(0.1f, stopDistance);

    public bool Matches(string normalizedTranscript)
    {
        if (string.IsNullOrWhiteSpace(normalizedTranscript))
        {
            return false;
        }

        if (ContainsToken(normalizedTranscript, commandId))
        {
            return true;
        }

        if (aliases == null)
        {
            return false;
        }

        for (int i = 0; i < aliases.Length; i++)
        {
            if (ContainsToken(normalizedTranscript, aliases[i]))
            {
                return true;
            }
        }

        return false;
    }

    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(commandId) ? gameObject.name : commandId;
    }

    private static bool ContainsToken(string haystack, string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        return haystack.Contains(token.Trim().ToLowerInvariant());
    }
}
