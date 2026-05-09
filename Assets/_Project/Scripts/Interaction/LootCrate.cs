using UnityEngine;

public class LootCrate : MonoBehaviour, IInteractable
{
    // 是否已经被搜刮
    private bool isLooted = false;

    // 提示文字：根据是否已经搜刮显示不同内容
    public string GetPromptText()
    {
        if (isLooted)
        {
            return "Already looted";
        }

        return "Press E to open";
    }

    // 按 E 时被 PlayerInteraction 调用
    public void Interact(GameObject player)
    {
        if (isLooted)
        {
            Debug.Log("这个箱子已经空了。");
            return;
        }

        // 这里先用 Debug 模拟获得物品
        Debug.Log("获得物品：医药包");
        Debug.Log("获得物品：子弹 x30");
        Debug.Log("获得物品：现金 500");

        // TODO：以后可以在这里真正给玩家加背包物品、子弹等

        isLooted = true;

        // 如果你希望打开后箱子直接消失，可以取消注释这一行：
        // Destroy(gameObject);
    }
}
