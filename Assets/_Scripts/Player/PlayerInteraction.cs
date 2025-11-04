using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private IInteractable currentTarget;

    void Update()
    {
        if (currentTarget != null && Input.GetKeyDown(KeyCode.F))
        {
            InteractUI.Instance.Press();  // Gọi hành động panel
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            currentTarget = interactable;
            string name = currentTarget.GetInteractName();

            if (!string.IsNullOrEmpty(name))
            {
                InteractUI.Instance.Show(name, () =>
                {
                    currentTarget.Interact();
                });
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();

        if (interactable != null && interactable == currentTarget)
        {
            currentTarget = null;
            InteractUI.Instance.Hide();
        }
    }
}
