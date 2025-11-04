using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;

    [Header("UI References")]
    public Transform inventoryPanel;
    public GameObject slotPrefab;

    [Header("Item Info Panel")]
    public Image infoIcon;
    public TMP_Text infoName;
    public TMP_Text infoDescription;
    public Button useButton;

    private InventorySlotUI selectedSlotUI;
    private ItemData selectedItem;

    // Danh sách các slot UI đang hiển thị
    private List<InventorySlotUI> slotUIList = new List<InventorySlotUI>();

    void Awake()
    {
        Instance = this;
    }

    public void UpdateUI()
    {
        // Kiểm tra InventoryManager đã tồn tại chưa
        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("InventoryManager chưa được khởi tạo!");
            return;
        }

        if (InventoryManager.Instance.inventorySlots == null)
        {
            Debug.LogWarning("inventorySlots chưa được khởi tạo!");
            return;
        }

        foreach (Transform child in inventoryPanel)
        {
            if (child.GetComponent<InventorySlotUI>() != null)
                Destroy(child.gameObject);
        }

        // Nếu chưa có item nào, có thể hiện thông báo trống
        if (InventoryManager.Instance.inventorySlots.Count == 0)
        {
            Debug.Log("Inventory trống.");
            DeselectItemSlot();
            return;
        }

        // Tạo slot mới dựa trên dữ liệu trong InventoryManager
        foreach (var slot in InventoryManager.Instance.inventorySlots)
        {
            GameObject newSlot = Instantiate(slotPrefab, inventoryPanel);
            var slotUI = newSlot.GetComponent<InventorySlotUI>();
            slotUI.panelUI = this;
            slotUI.SetItem(slot.item, slot.quantity);

            slotUIList.Add(slotUI);
        }

        // Tự động chọn slot đầu tiên và hiển thị info
        if (slotUIList.Count > 0)
        {
            SelectItemSlot(slotUIList[0]);
            ShowItemInfo(slotUIList[0].itemData);
        }
        else
        {
            DeselectItemSlot();
        }
    }

    public void ShowItemInfo(ItemData item)
    {
        if (item == null)
        {
            Debug.LogWarning("Item null khi gọi ShowItemInfo!");
            return;
        }

        if (infoIcon != null)
        {
            infoIcon.enabled = true;   // Bật lại icon khi có item
            infoIcon.sprite = item.icon;
        }

        if (infoName != null) infoName.text = item.itemName;
        if (infoDescription != null) infoDescription.text = item.description;

        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(() => UseSelectedItem(item));
        }
    }

    public void SelectItemSlot(InventorySlotUI slotUI)
    {
        if (selectedSlotUI != null)
            selectedSlotUI.SetSelected(false);

        selectedSlotUI = slotUI;
        selectedItem = slotUI.itemData;
        selectedSlotUI.SetSelected(true);
    }

    public void DeselectItemSlot()
    {
        if (selectedSlotUI != null)
            selectedSlotUI.SetSelected(false);

        selectedSlotUI = null;
        selectedItem = null;

        // Ẩn icon khi chưa có item
        if (infoIcon != null)
            infoIcon.enabled = false;

        if (infoName != null) infoName.text = "";
        if (infoDescription != null) infoDescription.text = "";
    }

    private void UseSelectedItem(ItemData item)
    {
        Debug.Log($"Đã sử dụng: {item.itemName}");
        InventoryManager.Instance.RemoveItem(item, 1);
        UpdateUI();
    }
}
