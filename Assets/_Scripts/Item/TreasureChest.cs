using System.Collections.Generic;
using UnityEngine;

public class TreasureChest : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class Loot
    {
        public ItemData item;
        public int minQuantity = 1;
        public int maxQuantity = 1;
        [Range(0f, 1f)]
        public float dropChance = 1f; // 1f = 100% drop
    }

    [Header("Chest Settings")]
    public List<Loot> lootTable = new List<Loot>();
    public Transform chestLid;
    public float openAngle = -90f;
    public float openSpeed = 5f;
    public bool isOpened = false;
    public float destroyDelay = 2f;

    [Header("XP Reward")]
    public float xpReward = 100f;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private bool isOpening = false;
    private bool playerInRange = false;

    void Start()
    {
        if (chestLid != null)
        {
            closedRotation = chestLid.localRotation;
            openRotation = Quaternion.Euler(openAngle, 0, 0);
        }
    }

    void Update()
    {
        if (isOpening && chestLid != null)
        {
            chestLid.localRotation = Quaternion.Lerp(
                chestLid.localRotation,
                openRotation,
                Time.deltaTime * openSpeed
            );
        }

        if (playerInRange && !isOpened && Input.GetKeyDown(KeyCode.F))
        {
            Interact();                    // gọi mở rương
            InteractUI.Instance.Hide();    // ẩn panel sau khi mở
        }
    }

    public void Interact()
    {
        OpenChest();
    }

    public string GetInteractName()
    {
        return isOpened ? "" : "Chest";
    }

    public void OpenChest()
    {
        if (isOpened) return;
        isOpened = true;
        isOpening = true;

        // Phát âm thanh mở rương 2D
        AudioManager.Instance.PlaySFX("Treasure Chest");

        // Cộng XP cho player
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.AddXP(xpReward);
            Debug.Log($"Player nhận {xpReward} XP từ rương.");
        }

        // Danh sách loot kết quả
        List<Loot> finalLoot = new List<Loot>();

        // Random loot
        foreach (var loot in lootTable)
        {
            float roll = Random.value; // 0..1
            if (roll <= loot.dropChance)
            {
                int qty = Random.Range(loot.minQuantity, loot.maxQuantity + 1);
                Loot pickedLoot = new Loot
                {
                    item = loot.item,
                    minQuantity = qty,
                    maxQuantity = qty,
                    dropChance = 1f
                };
                finalLoot.Add(pickedLoot);

                // Thêm vào inventory
                InventoryManager.Instance.AddItem(loot.item, qty);

                // Gọi HUD hiển thị từng item
                if (LootNotificationUI.Instance != null)
                {
                    LootNotificationUI.Instance.ShowLoot(loot.item, qty);
                }
            }
        }

        // Log kết quả loot cho debug
        foreach (var l in finalLoot)
        {
            Debug.Log($"Looted: {l.item.itemName} x{l.minQuantity}");
        }

        // Sau khi mở xong thì xóa rương
        Destroy(gameObject, destroyDelay);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpened)
        {
            playerInRange = true;
            // Hiển thị panel và gán hành động nhấn
            InteractUI.Instance.Show(GetInteractName(), () =>
            {
                Interact();                 // gọi hàm Interact hiện có
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
