using System.Collections.Generic;
using UnityEngine;

public class LootNotificationUI : MonoBehaviour
{
    public static LootNotificationUI Instance;

    [Header("UI Settings")]
    public Transform container;               // Panel cha (UI góc trái)
    public LootNotificationSlot slotPrefab;   // Prefab cho mỗi item hiển thị
    public int maxVisibleSlots = 4;

    private Queue<LootNotificationData> lootQueue = new Queue<LootNotificationData>();
    private List<LootNotificationSlot> activeSlots = new List<LootNotificationSlot>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void ShowLoot(ItemData item, int quantity)
    {
        lootQueue.Enqueue(new LootNotificationData(item, quantity));
        TryShowNext();
    }

    private void TryShowNext()
    {
        // Nếu còn slot trống và queue có item → hiển thị tiếp
        if (activeSlots.Count >= maxVisibleSlots || lootQueue.Count == 0) return;

        var data = lootQueue.Dequeue();
        var slot = Instantiate(slotPrefab, container);
        slot.Setup(data.item, data.quantity, OnSlotFinished);

        activeSlots.Add(slot);
    }

    private void OnSlotFinished(LootNotificationSlot slot)
    {
        activeSlots.Remove(slot);
        Destroy(slot.gameObject);

        // Hiển thị item tiếp theo trong queue
        TryShowNext();
    }
}

[System.Serializable]
public class LootNotificationData
{
    public ItemData item;
    public int quantity;

    public LootNotificationData(ItemData item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}
