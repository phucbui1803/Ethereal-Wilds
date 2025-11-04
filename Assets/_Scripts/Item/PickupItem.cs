using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections;

public class PickupItem : MonoBehaviour, IInteractable
{
    public ItemData itemData;
    private bool playerInRange = false;
    private bool isPicked = false;

    [Header("Respawn Settings")]
    public float respawnTime = 10f; // thời gian respawn
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Transform initialParent;

    private void Awake()
    {
        // Lưu vị trí, rotation và parent ban đầu
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialParent = transform.parent;
    }

    private void Update()
    {
        if (!playerInRange || isPicked) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            Interact();
            InteractUI.Instance.Hide();
        }
    }

    public void Interact()
    {
        if (isPicked) return;  // tránh nhặt nhiều lần
        isPicked = true;

        if (itemData != null)
        {
            // Thêm vào inventory 1 item
            InventoryManager.Instance.AddItem(itemData, 1);

            // Cập nhật UI inventory
            if (InventoryUI.Instance != null)
                InventoryUI.Instance.UpdateUI();

            // Hiển thị loot notification
            if (LootNotificationUI.Instance != null)
            {
                LootNotificationUI.Instance.ShowLoot(itemData, 1);
            }

            // Phát âm thanh nhặt item 2D
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX("Pickup Item");

            Debug.Log($"Nhặt được {itemData.itemName}");
        }
        else
        {
            Debug.LogWarning("PickupItem: itemData chưa được gán!");
        }

        // Ẩn UI 
        playerInRange = false;
        InteractUI.Instance.Hide();

        // Update quest progress
        QuestManager.Instance?.UpdateCollectProgress(itemData.itemName);

        // Ẩn item thay vì xóa
        gameObject.SetActive(false);

        // Gửi request respawn cho manager
        if (ItemRespawnManager.Instance != null)
            ItemRespawnManager.Instance.RespawnItem(gameObject, respawnTime);
    }

    public string GetInteractName()
    {
        return itemData != null ? itemData.itemName : "Flower";
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isPicked)
        {
            playerInRange = true;
            // Hiển thị panel và gán hành động nhấn
            InteractUI.Instance.Show(GetInteractName(), () =>
            {
                Interact();                 // hàm xử lý nhặt item
                InteractUI.Instance.Hide(); // ẩn panel sau khi nhấn
            });
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            InteractUI.Instance.Hide();
        }
    }
}
