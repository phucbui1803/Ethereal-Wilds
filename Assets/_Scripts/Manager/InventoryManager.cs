using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<InventorySlot> inventorySlots = new List<InventorySlot>();

    [Header("Tất cả item trong game")]
    public List<ItemData> allItems = new List<ItemData>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Khởi tạo item mặc định theo defaultQuantity trong ItemData
        foreach (var item in allItems)
        {
            if (item == null) continue;

            if (!inventorySlots.Exists(x => x.item == item))
            {
                int startAmount = Mathf.Max(0, item.defaultQuantity); // tránh âm
                inventorySlots.Add(new InventorySlot(item, startAmount));
                Debug.Log($"[InventoryManager] Khởi tạo {startAmount} x {item.itemName}");
            }
        }

        // Cập nhật UI (nếu có)
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.UpdateUI();
    }

    public void AddItem(ItemData item, int amount = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("AddItem được gọi nhưng ItemData = null!");
            return;
        }

        InventorySlot slot = inventorySlots.Find(x => x.item == item);
        if (slot != null)
        {
            slot.quantity += amount;
        }
        else
        {
            inventorySlots.Add(new InventorySlot(item, amount));
        }

        Debug.Log($"Đã thêm {amount} x {item.itemName} vào kho");

        // Cập nhật UI nếu có
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.UpdateUI();
    }

    public void RemoveItem(ItemData item, int amount)
    {
        if (item == null) return;

        InventorySlot slot = inventorySlots.Find(x => x.item == item);
        if (slot != null)
        {
            slot.quantity -= amount;
            if (slot.quantity <= 0)
                inventorySlots.Remove(slot);
        }

        // Cập nhật UI nếu có
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.UpdateUI();
    }

    public int GetItemCount(ItemData item)
    {
        if (item == null) return 0;

        InventorySlot slot = inventorySlots.Find(x => x.item == item);
        return slot != null ? slot.quantity : 0;
    }

    /// <summary>
    /// Kiểm tra xem có đủ số lượng item không
    /// </summary>
    public bool HasEnoughItem(ItemData item, int amount)
    {
        return GetItemCount(item) >= amount;
    }
}
