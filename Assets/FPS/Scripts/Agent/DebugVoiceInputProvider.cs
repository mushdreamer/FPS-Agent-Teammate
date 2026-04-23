using UnityEngine;

public class DebugVoiceInputProvider : MonoBehaviour, IVoiceInputProvider
{
    [Tooltip("模拟语音识别结果；按住V并松开后会使用该文本。")]
    [SerializeField] private string simulatedTranscript = "follow";

    public string GetTranscript()
    {
        return simulatedTranscript;
    }

    public void SetSimulatedTranscript(string value)
    {
        simulatedTranscript = value;
    }
}
