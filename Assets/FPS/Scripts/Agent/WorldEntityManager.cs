using System.Collections.Generic;
using UnityEngine;

// 实体注册中心：应挂载在场景全局管理空物体上
public class WorldEntityManager : MonoBehaviour
{
    public static WorldEntityManager Instance { get; private set; }

    private Dictionary<string, Transform> activeEntities = new Dictionary<string, Transform>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterEntity(string entityName, Transform entityTransform)
    {
        if (!activeEntities.ContainsKey(entityName))
        {
            activeEntities.Add(entityName, entityTransform);
        }
        else
        {
            Debug.LogWarning("尝试重复注册实体名: " + entityName);
        }
    }

    public void UnregisterEntity(string entityName)
    {
        if (activeEntities.ContainsKey(entityName))
        {
            activeEntities.Remove(entityName);
        }
    }

    public Transform GetEntityByName(string partialName)
    {
        foreach (var kvp in activeEntities)
        {
            // 支持模糊匹配，例如语音识别出"去门A那里"，只要包含"门A"即视为匹配
            if (partialName.Contains(kvp.Key))
            {
                return kvp.Value;
            }
        }
        return null;
    }
}