using UnityEngine;

public interface IInteractable_Old
{
    // 和所有交互物体统一：传入 GameObject player
    void Interact(GameObject player);

    // 返回提示文字
    string GetPromptText();
}
