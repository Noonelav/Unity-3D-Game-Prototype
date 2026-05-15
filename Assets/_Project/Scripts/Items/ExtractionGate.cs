using UnityEngine;

/// <summary>
/// 撤离大门 - 玩家走近 + 拥有 3 把不同钥匙（keyId 1/2/3）+ 按 E → 胜利
/// 没集齐则提示失败，游戏继续
/// </summary>
[RequireComponent(typeof(Collider))]
public class ExtractionGate : MonoBehaviour, IInteractable
{
    [Header("条件")]
    public int requiredKeys = 3;

    [Header("UI 引用（可不填，自动找）")]
    public VictoryUI victoryUI;

    void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        if (victoryUI == null) victoryUI = FindFirstObjectByType<VictoryUI>(FindObjectsInactive.Include);
    }

    public string GetPromptText()
    {
        var inv = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerInventory>();
        int have = inv != null ? inv.keyCount : 0;
        if (have >= requiredKeys)
            return $"Press E to EXTRACT ({have}/{requiredKeys} keys)";
        return $"Press E to try extracting (NEED {requiredKeys - have} more keys)";
    }

    public void Interact(GameObject player)
    {
        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv == null) return;

        // 检查是否集齐 3 把不同 keyId
        bool hasAll = HasAllUniqueKeys(inv);

        if (hasAll)
        {
            Debug.Log("🎉 VICTORY! Successfully extracted!");
            if (victoryUI == null) victoryUI = FindFirstObjectByType<VictoryUI>(FindObjectsInactive.Include);
            victoryUI?.Show();
        }
        else
        {
            int have = inv.keyCount;
            Debug.Log($"❌ Extraction failed: only have {have}/{requiredKeys} keys");
            // 显示瞬时提示
            ExtractFailedToast.Show($"Extraction Failed — need {requiredKeys - have} more keys!");
        }
    }

    bool HasAllUniqueKeys(PlayerInventory inv)
    {
        if (inv.keys.Count < requiredKeys) return false;
        var ids = new System.Collections.Generic.HashSet<int>();
        foreach (var k in inv.keys) ids.Add(k.keyId);
        return ids.Count >= requiredKeys;
    }
}
