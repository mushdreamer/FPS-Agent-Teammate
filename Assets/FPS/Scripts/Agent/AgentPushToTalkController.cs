using UnityEngine;

public class AgentPushToTalkController : MonoBehaviour
{
    [Header("Push-To-Talk")]
    [SerializeField] private KeyCode pushToTalkKey = KeyCode.V;
    [SerializeField] private MonoBehaviour voiceInputProviderBehaviour;
    [SerializeField] private AgentCommandRouter commandRouter;

    private IVoiceInputProvider voiceInputProvider;
    private bool isListening;

    private void Awake()
    {
        if (voiceInputProviderBehaviour == null)
        {
            voiceInputProviderBehaviour = GetComponent<DebugVoiceInputProvider>();
        }

        voiceInputProvider = voiceInputProviderBehaviour as IVoiceInputProvider;

        if (commandRouter == null)
        {
            commandRouter = FindObjectOfType<AgentCommandRouter>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pushToTalkKey))
        {
            StartListening();
        }

        if (Input.GetKeyUp(pushToTalkKey))
        {
            StopListeningAndDispatch();
        }
    }

    private void StartListening()
    {
        isListening = true;
        Debug.Log("[PTT] Listening started...");
    }

    private void StopListeningAndDispatch()
    {
        if (!isListening)
        {
            return;
        }

        isListening = false;

        if (voiceInputProvider == null)
        {
            Debug.LogWarning("[PTT] Voice provider is missing.");
            return;
        }

        string transcript = voiceInputProvider.GetTranscript();
        Debug.Log($"[PTT] Listening stopped. Transcript: {transcript}");
        commandRouter?.Route(transcript);
    }
}
