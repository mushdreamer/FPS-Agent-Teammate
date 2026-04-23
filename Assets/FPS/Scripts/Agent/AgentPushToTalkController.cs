using UnityEngine;

public class AgentPushToTalkController : MonoBehaviour
{
    [Header("Push-To-Talk")]
    [SerializeField] private KeyCode pushToTalkKey = KeyCode.V;
    [SerializeField] private MonoBehaviour voiceInputProviderBehaviour;
    [SerializeField] private AgentCommandRouter commandRouter;

    private IVoiceInputProvider voiceInputProvider;

    private void Awake()
    {
        if (voiceInputProviderBehaviour == null)
        {
            voiceInputProviderBehaviour = GetComponent<MicVoiceInputProvider>();

            if (voiceInputProviderBehaviour == null)
            {
                voiceInputProviderBehaviour = GetComponent<DebugVoiceInputProvider>();
            }
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
        if (voiceInputProvider == null)
        {
            Debug.LogWarning("[PTT] Voice provider is missing.");
            return;
        }

        voiceInputProvider.StartListening();
        Debug.Log("[PTT] Listening started...");
    }

    private void StopListeningAndDispatch()
    {
        if (voiceInputProvider == null)
        {
            Debug.LogWarning("[PTT] Voice provider is missing.");
            return;
        }

        if (!voiceInputProvider.IsListening)
        {
            return;
        }

        voiceInputProvider.StopListening(OnTranscriptReady);
    }

    private void OnTranscriptReady(string transcript)
    {
        Debug.Log($"[PTT] Listening stopped. Transcript: {transcript}");
        commandRouter?.Route(transcript);
    }
}
