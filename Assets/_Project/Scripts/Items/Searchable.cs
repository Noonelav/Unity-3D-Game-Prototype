using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可搜索容器 — 用于箱子、房屋光圈
/// 按 E 后打开搜索面板，玩家点击物品逐个拾取
/// </summary>
[RequireComponent(typeof(Collider))]
public class Searchable : MonoBehaviour, IInteractable
{
    [Header("容器配置")]
    public string containerName = "Container";
    public List<Item> contents = new List<Item>();   // 容器里的物品

    [Header("UI 显示")]
    [Tooltip("0=用 SearchUI 默认 4x4；>0 强制覆盖格子总数")]
    public int      slotCountOverride = 0;
    public int      columnsOverride   = 0;

    [Header("UI")]
    public SearchUI searchUI;   // 弹出的搜索面板（可自动找）

    private bool isOpen = false;
    private GameObject currentPlayer;

    void Awake()
    {
        if (searchUI == null) searchUI = FindFirstObjectByType<SearchUI>(FindObjectsInactive.Include);

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    public string GetPromptText()
    {
        if (contents.Count == 0) return $"{containerName} (Empty)";
        return $"Press E to search {containerName}";
    }

    public void Interact(GameObject player)
    {
        if (searchUI == null)
        {
            Debug.LogWarning("[Searchable] 没有 SearchUI 引用！");
            return;
        }
        currentPlayer = player;
        searchUI.Open(this, player);
    }

    /// <summary>玩家从面板里拿走一个物品时由 SearchUI 调用</summary>
    public void TakeItem(int index, GameObject player)
    {
        if (index < 0 || index >= contents.Count) return;
        PlayerInventory inv = player.GetComponent<PlayerInventory>();
        if (inv == null) return;
        inv.AddItem(contents[index]);
        contents.RemoveAt(index);
    }
}
