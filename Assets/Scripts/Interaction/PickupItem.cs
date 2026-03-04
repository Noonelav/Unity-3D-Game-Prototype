using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public string itemName = "Ammo";
    public int amount = 30;

    public string GetPromptText()
    {
        return $"Press E to pick up {itemName}";
    }

    public void Interact(GameObject player)
    {
        Debug.Log($"Picked up {itemName} x{amount}");

        // 拾取后直接销毁这个物体
        Destroy(gameObject);
    }
}
