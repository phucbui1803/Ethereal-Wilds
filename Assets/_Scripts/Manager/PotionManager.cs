using System.Collections.Generic;
using UnityEngine;

public class PotionManager : MonoBehaviour
{
    public static PotionManager Instance;

    [Header("UI Reference")]
    public PotionPanelUI potionPanelUI;

    [Header("All Potions Database")]
    public List<PotionData> allPotions = new List<PotionData>();

    [Header("Potion Inventory")]
    public List<PotionSlot> ownedPotions = new List<PotionSlot>();

    private void Awake()
    {
        // Singleton
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Khởi tạo slot cho tất cả potion
        foreach (var potion in allPotions)
        {
            if (!ownedPotions.Exists(x => x.potionData == potion))
                ownedPotions.Add(new PotionSlot(potion, 10));
        }
    }

    private void Start()
    {
        // Gửi dữ liệu slot đến UI hiển thị lần đầu
        ShowPotionInventory();
    }

    public void ShowPotionInventory()
    {
        if (potionPanelUI != null)
            potionPanelUI.ShowPotions(ownedPotions);
        else
            Debug.LogWarning("PotionPanelUI chưa được gán!");
    }

    public int GetPotionQuantity(PotionData potion)
    {
        var slot = ownedPotions.Find(x => x.potionData == potion);
        return slot != null ? slot.quantity : 0;
    }

    public bool CanCraft(PotionData potion)
    {
        if (potion == null) return false;
        int have1 = InventoryManager.Instance.GetItemCount(potion.material1);
        int have2 = InventoryManager.Instance.GetItemCount(potion.material2);
        return have1 >= potion.material1Amount && have2 >= potion.material2Amount;
    }
}
