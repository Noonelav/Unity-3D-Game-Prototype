using UnityEngine;

public class ChestLoot : MonoBehaviour, IInteractable
{
    public GameObject inventoryPanel;
    private bool openedOnce = false;

    public string GetPromptText()
    {
        if (!openedOnce)
            return "Press E to open chest";

        return "Press E to loot chest";
    }

    public void Interact(GameObject player)
    {
        if (inventoryPanel == null) return;

        bool show = !inventoryPanel.activeSelf;
        inventoryPanel.SetActive(show);

        if (show)
        {
            openedOnce = true;
            Debug.Log("Chest UI opened");
            // 将来在这里把宝箱里的物品填到格子里
        }
        else
        {
            Debug.Log("Chest UI closed");
        }
    }
}
