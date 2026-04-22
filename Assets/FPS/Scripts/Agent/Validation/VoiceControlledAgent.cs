using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Windows.Speech;

[RequireComponent(typeof(NavMeshAgent))]
public class VoiceControlledAgent : MonoBehaviour
{
    private DictationRecognizer dictationRecognizer;
    private NavMeshAgent agent;
    private Transform player;
    private bool isFollowing = false;

    // 新增：记录玩家上一次的位置
    private Vector3 lastPlayerPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // 关键修复 1：通过代码强制设置停止距离为 2 米，防止它试图和你挤在同一个坐标点
        agent.stoppingDistance = 2.0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            lastPlayerPosition = player.position;
        }
        else
        {
            Debug.LogError("未找到标签为 Player 的物体！");
        }

        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.Log("Heard: " + text);
            string lowerText = text.ToLower();

            if (lowerText.Contains("follow"))
            {
                isFollowing = true;
                Debug.Log("Command accepted: Following player");
            }
            else if (lowerText.Contains("stop"))
            {
                isFollowing = false;
                agent.ResetPath();
                Debug.Log("Command accepted: Stopped");
            }
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogError("Dictation Error: " + error);
        };
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (dictationRecognizer != null && dictationRecognizer.Status != SpeechSystemStatus.Running)
            {
                Debug.Log("Hold V: Listening...");
                dictationRecognizer.Start();
            }
        }
        if (Input.GetKeyUp(KeyCode.V))
        {
            if (dictationRecognizer != null && dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                Debug.Log("Released V: Stopped listening.");
                dictationRecognizer.Stop();
            }
        }

        if (isFollowing && player != null)
        {
            // 关键修复 2：只有当玩家移动超过 0.5 米时，才给 Agent 发送新的寻路指令
            // 这给了 Agent 足够的时间去绕过墙角，而不是每帧都在重置大脑
            if (Vector3.Distance(lastPlayerPosition, player.position) > 0.5f)
            {
                agent.SetDestination(player.position);
                lastPlayerPosition = player.position;
            }
        }
    }

    void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
            {
                dictationRecognizer.Stop();
            }
            dictationRecognizer.Dispose();
        }
    }
}