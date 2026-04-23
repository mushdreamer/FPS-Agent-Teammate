using UnityEngine;

public class DebugVoiceInputProvider : MonoBehaviour, IVoiceInputProvider
{
    [Tooltip("模拟语音识别结果。为空表示本次没有识别到有效语音。")]
    [SerializeField] private string simulatedTranscript = string.Empty;

    [Tooltip("读取后自动清空，避免松开V时重复执行上一次指令。")]
    [SerializeField] private bool consumeOnce = true;

    public string GetTranscript()
    {
        string transcript = simulatedTranscript == null ? string.Empty : simulatedTranscript.Trim();

        if (consumeOnce)
        {
            simulatedTranscript = string.Empty;
        }

        return transcript;
    }

    public void SetSimulatedTranscript(string value)
    {
        simulatedTranscript = value;
    }
}
