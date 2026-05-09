using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public Transform gridParent;    // 指向 Grid
    public GameObject slotPrefab;   // Slot 预制体
    public int width = 8;
    public int height = 8;

    void Awake()
    {
        CreateSlots();
        gameObject.SetActive(false);   // 确保一开始是隐藏
    }

    void CreateSlots()
    {
        if (gridParent == null || slotPrefab == null) return;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Instantiate(slotPrefab, gridParent);
            }
        }
    }
}
