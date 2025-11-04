using UnityEngine;

public class EnemyLootDrop : MonoBehaviour
{
    [System.Serializable]
    public class LootItem
    {
        public ItemData itemData;
        [Range(0f, 1f)] public float dropChance = 1f;
        public int minAmount = 1;
        public int maxAmount = 1;
    }

    [Header("Loot Settings")]
    public LootItem[] lootTable;  // danh sách vật phẩm có thể rơi

    // Gọi hàm này khi enemy chết
    public void DropLootToPlayer()
    {
        if (lootTable == null || lootTable.Length == 0)
            return;

        foreach (var loot in lootTable)
        {
            if (Random.value <= loot.dropChance)
            {
                int amount = Random.Range(loot.minAmount, loot.maxAmount + 1);
                GiveLootToPlayer(loot.itemData, amount);
            }
        }
    }

    private void GiveLootToPlayer(ItemData itemData, int amount)
    {
        if (itemData == null) return;

        // Thêm item vào inventory người chơi
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(itemData, amount);
            Debug.Log($"[LootDrop] Nhận được {amount}x {itemData.itemName}");
        }

        // Cập nhật UI inventory (nếu có)
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.UpdateUI();

        // Hiển thị thông báo loot
        if (LootNotificationUI.Instance != null)
            LootNotificationUI.Instance.ShowLoot(itemData, amount);

        // Âm thanh nhặt item
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Pickup Item");
    }
}
