using System;

public interface IVoiceInputProvider
{
    bool IsListening { get; }
    void StartListening();
    void StopListening(Action<string> onTranscriptReady);
}
