using UnityEngine;

public class InteractableEntity : MonoBehaviour
{
    [Tooltip("定义语音识别所匹配的具体名称，例如：门A、猫B")]
    public string uniqueEntityName;

    private void Start()
    {
        if (!string.IsNullOrEmpty(uniqueEntityName))
        {
            WorldEntityManager.Instance.RegisterEntity(uniqueEntityName, transform);
        }
        else
        {
            Debug.LogWarning("InteractableEntity 实体名称未配置，将无法响应语音指令: " + gameObject.name);
        }
    }

    private void OnDestroy()
    {
        if (WorldEntityManager.Instance != null && !string.IsNullOrEmpty(uniqueEntityName))
        {
            WorldEntityManager.Instance.UnregisterEntity(uniqueEntityName);
        }
    }
}