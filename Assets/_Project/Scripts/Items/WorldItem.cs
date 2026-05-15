using UnityEngine;

/// <summary>
/// 世界上散落的可拾取物品（地上）
/// 玩家走近 + 按 E 直接捡进背包
/// </summary>
[RequireComponent(typeof(Collider))]
public class WorldItem : MonoBehaviour, IInteractable
{
    public Item item = new Item();

    public string GetPromptText()
    {
        return $"Press E to pick up {item.displayName}";
    }

    public void Interact(GameObject player)
    {
        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv == null) return;
        inv.AddItem(item);
        Destroy(gameObject);
    }

    void Reset()
    {
        // 确保 Collider 是 Trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }
}
