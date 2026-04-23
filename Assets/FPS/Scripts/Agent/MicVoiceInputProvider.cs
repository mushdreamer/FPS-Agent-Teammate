using System;
using System.Text;
using UnityEngine;
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
using UnityEngine.Windows.Speech;
#endif

public class MicVoiceInputProvider : MonoBehaviour, IVoiceInputProvider
{
    [SerializeField] private float initialSilenceTimeoutSeconds = 5f;
    [SerializeField] private float autoSilenceTimeoutSeconds = 3f;

    public bool IsListening { get; private set; }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private DictationRecognizer recognizer;
    private readonly StringBuilder transcriptBuilder = new StringBuilder();
    private Action<string> pendingCallback;
#endif

    private void OnDestroy()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        if (recognizer != null)
        {
            recognizer.DictationResult -= OnDictationResult;
            recognizer.DictationComplete -= OnDictationComplete;
            recognizer.DictationError -= OnDictationError;
            recognizer.Dispose();
        }
#endif
    }

    public void StartListening()
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        EnsureRecognizer();

        if (recognizer.Status != SpeechSystemStatus.Running)
        {
            transcriptBuilder.Length = 0;
            recognizer.InitialSilenceTimeoutSeconds = initialSilenceTimeoutSeconds;
            recognizer.AutoSilenceTimeoutSeconds = autoSilenceTimeoutSeconds;
            recognizer.Start();
        }

        IsListening = true;
#else
        IsListening = false;
        Debug.LogWarning("[MicVoiceInputProvider] DictationRecognizer 仅在 Windows Editor/Standalone 下可用。请接入对应平台的 STT。") ;
#endif
    }

    public void StopListening(Action<string> onTranscriptReady)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        pendingCallback = onTranscriptReady;

        if (recognizer == null)
        {
            FinalizeTranscript(string.Empty);
            return;
        }

        if (recognizer.Status == SpeechSystemStatus.Running)
        {
            recognizer.Stop();
            return;
        }

        FinalizeTranscript(transcriptBuilder.ToString().Trim());
#else
        onTranscriptReady?.Invoke(string.Empty);
#endif
    }

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private void EnsureRecognizer()
    {
        if (recognizer != null)
        {
            return;
        }

        recognizer = new DictationRecognizer();
        recognizer.DictationResult += OnDictationResult;
        recognizer.DictationComplete += OnDictationComplete;
        recognizer.DictationError += OnDictationError;
    }

    private void OnDictationResult(string text, ConfidenceLevel confidence)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            if (transcriptBuilder.Length > 0)
            {
                transcriptBuilder.Append(' ');
            }

            transcriptBuilder.Append(text);
        }
    }

    private void OnDictationComplete(DictationCompletionCause cause)
    {
        string transcript = transcriptBuilder.ToString().Trim();
        if (cause != DictationCompletionCause.Complete)
        {
            Debug.LogWarning($"[MicVoiceInputProvider] Dictation completed with cause: {cause}");
        }

        FinalizeTranscript(transcript);
    }

    private void OnDictationError(string error, int hresult)
    {
        Debug.LogError($"[MicVoiceInputProvider] Dictation error: {error}, HRESULT: {hresult}");
        FinalizeTranscript(string.Empty);
    }

    private void FinalizeTranscript(string transcript)
    {
        IsListening = false;

        Action<string> callback = pendingCallback;
        pendingCallback = null;
        callback?.Invoke(transcript);
    }
#endif
}
