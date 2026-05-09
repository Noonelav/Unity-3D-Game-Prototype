using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.E;
    public TextMeshProUGUI promptText; 

    IInteractable currentTarget;

    void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentTarget = interactable;
            ShowPrompt(interactable.GetPromptText());
        }
    }

    void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable == currentTarget)
        {
            currentTarget = null;
            HidePrompt();
        }
    }

    void Update()
    {
        if (currentTarget != null && Input.GetKeyDown(interactKey))
        {
            currentTarget.Interact(gameObject);
            currentTarget = null;
            HidePrompt();
        }
    }

    void ShowPrompt(string text)
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(true);
        promptText.text = text;
    }

    void HidePrompt()
    {
        if (promptText == null) return;
        promptText.gameObject.SetActive(false);
    }
}
