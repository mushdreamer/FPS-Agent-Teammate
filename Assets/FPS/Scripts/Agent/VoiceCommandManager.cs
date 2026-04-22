using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class VoiceCommandManager : MonoBehaviour
{
    private DictationRecognizer dictationRecognizer;

    // 预设的可识别动词库
    private readonly List<string> actionVerbs = new List<string> { "跟随", "攻击", "移动" };

    // 静态事件，向Agent分发解析成功后的指令（包含动作和目标Transform）
    public static event Action<string, Transform> OnCommandParsed;

    private void Start()
    {
        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            ParseCommand(text);
        };

        dictationRecognizer.DictationHypothesis += (text) =>
        {
            // 可在此处添加UI显示正在听写的半成品文本
        };

        dictationRecognizer.DictationComplete += (cause) =>
        {
            if (cause != DictationCompletionCause.Complete)
            {
                Debug.LogWarning("Dictation error: " + cause.ToString());
            }
            dictationRecognizer.Start(); // 保持持续监听
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogError("Dictation error: " + error);
        };

        dictationRecognizer.Start();
    }

    private void ParseCommand(string speechText)
    {
        Debug.Log("Captured Speech: " + speechText);
        string matchedVerb = string.Empty;

        foreach (string verb in actionVerbs)
        {
            if (speechText.Contains(verb))
            {
                matchedVerb = verb;
                break;
            }
        }

        if (string.IsNullOrEmpty(matchedVerb))
        {
            return;
        }

        // 提取动词之后的剩余字符串作为目标名词
        int verbIndex = speechText.IndexOf(matchedVerb);
        string potentialNoun = speechText.Substring(verbIndex + matchedVerb.Length).Trim();

        // 特殊目标硬编码处理
        if (potentialNoun.Contains("玩家") || potentialNoun.Contains("我"))
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                OnCommandParsed?.Invoke(matchedVerb, player.transform);
            }
            return;
        }

        // 检索实体注册中心，查找匹配的ABCD编号物体
        Transform targetTransform = WorldEntityManager.Instance.GetEntityByName(potentialNoun);
        if (targetTransform != null)
        {
            OnCommandParsed?.Invoke(matchedVerb, targetTransform);
        }
        else
        {
            Debug.LogWarning("识别到指令，但场景中不存在实体: " + potentialNoun);
        }
    }

    private void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
        }
    }
}