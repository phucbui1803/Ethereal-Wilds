using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PotionPanelUI : MonoBehaviour
{
    public static PotionPanelUI Instance;

    [Header("Inventory References")]
    public GameObject potionSlotPrefab;
    public Transform potionGridParent;

    [Header("Info Panel References")]
    public Image potionIcon;
    public TMP_Text potionNameText;
    public TMP_Text potionDescriptionText;

    [Header("Potion Buttons")]
    public Button useButton;
    public Button craftButton;

    [Header("Craft Materials UI")]
    public Image mat1Icon;
    public TMP_Text mat1Text;
    public Image mat2Icon;
    public TMP_Text mat2Text;

    private List<PotionSlotUI> potionSlots = new List<PotionSlotUI>();
    private PotionSlotUI selectedSlot = null;
    private PotionSlot selectedPotion = null;
    private List<PotionSlot> currentPotions = new List<PotionSlot>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (useButton != null)
            useButton.onClick.AddListener(OnUseButtonClicked);

        if (craftButton != null)
            craftButton.onClick.AddListener(OnCraftButtonClicked);
    }

    public void ShowPotions(List<PotionSlot> potions)
    {
        if (potionSlots.Count == 0)
        {
            foreach (var slot in potions)
            {
                GameObject slotGO = Instantiate(potionSlotPrefab, potionGridParent);
                PotionSlotUI slotUI = slotGO.GetComponent<PotionSlotUI>();
                slotUI.panelUI = this;
                slotUI.SetPotion(slot);   // reference gốc
                potionSlots.Add(slotUI);
            }
        }
        else
        {
            foreach (var slotUI in potionSlots)
            {
                slotUI.UpdateQuantity(); // dùng reference gốc
            }
        }

        if (selectedSlot == null && potionSlots.Count > 0)
            SelectPotionSlot(potionSlots[0]);
    }


    public void SelectPotionSlot(PotionSlotUI slot)
    {
        if (slot == null) return;

        if (selectedSlot != null && selectedSlot != slot)
            selectedSlot.SetSelected(false);

        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        // Lưu reference gốc luôn, không tạo mới
        selectedPotion = slot.potionSlot;
        ShowPotionInfo(selectedPotion);
    }

    private void ShowPotionInfo(PotionSlot slot)
    {
        if (slot == null) return;

        potionNameText.text = slot.potionData.potionName;

        // 🧠 nếu có mô tả động -> hiển thị tự động buff/debuff
        if (!string.IsNullOrEmpty(slot.potionData.GetDynamicDescription()))
            potionDescriptionText.text = slot.potionData.GetDynamicDescription();
        else
            potionDescriptionText.text = slot.potionData.description;

        if (potionIcon != null)
        {
            potionIcon.sprite = slot.potionData.icon;
            potionIcon.enabled = true;
        }

        // Update buttons
        if (useButton != null)
            useButton.interactable = slot.quantity > 0;

        if (craftButton != null)
            craftButton.interactable = PotionManager.Instance.CanCraft(slot.potionData);

        UpdateCraftMaterialsUI(slot.potionData);
    }

    private void ClearInfoPanel()
    {
        potionNameText.text = "";
        potionDescriptionText.text = "";
        potionIcon.enabled = false;
        if (useButton != null) useButton.interactable = false;
        if (craftButton != null) craftButton.interactable = false;
    }

    private void UpdateCraftMaterialsUI(PotionData potion)
    {
        if (potion == null) return;

        if (mat1Icon != null && mat1Text != null && potion.material1 != null)
        {
            mat1Icon.sprite = potion.material1.icon;
            int have1 = InventoryManager.Instance.GetItemCount(potion.material1);
            int need1 = potion.material1Amount;
            mat1Text.text = have1 >= need1
                ? $"<color=green>{have1}</color>/{need1}"
                : $"<color=red>{have1}</color>/{need1}";
        }

        if (mat2Icon != null && mat2Text != null && potion.material2 != null)
        {
            mat2Icon.sprite = potion.material2.icon;
            int have2 = InventoryManager.Instance.GetItemCount(potion.material2);
            int need2 = potion.material2Amount;
            mat2Text.text = have2 >= need2
                ? $"<color=green>{have2}</color>/{need2}"
                : $"<color=red>{have2}</color>/{need2}";
        }
    }

    private void OnUseButtonClicked()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (selectedPotion == null) return;

        if (selectedPotion.quantity > 0)
        {
            selectedPotion.quantity--;
            Debug.Log($"Used potion {selectedPotion.potionData.potionName}");

            // Cập nhật UI slot
            selectedSlot.UpdateQuantity();
            ShowPotionInfo(selectedPotion);

            // Áp dụng hiệu ứng potion lên player
            PlayerStats player = Object.FindFirstObjectByType<PlayerStats>();

            if (player != null)
            {
                PotionEffect.ApplyPotionEffect(selectedPotion.potionData, player);
            }
            else
            {
                Debug.LogWarning("Không tìm thấy PlayerStats trong scene!");
            }
        }
    }

    private void OnCraftButtonClicked()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (selectedPotion == null) return;

        if (PotionManager.Instance.CanCraft(selectedPotion.potionData))
        {
            InventoryManager.Instance.RemoveItem(selectedPotion.potionData.material1, selectedPotion.potionData.material1Amount);
            InventoryManager.Instance.RemoveItem(selectedPotion.potionData.material2, selectedPotion.potionData.material2Amount);

            selectedPotion.quantity++;
            Debug.Log($"Crafted potion {selectedPotion.potionData.potionName}");

            // Cập nhật tiến độ Quest Craft nếu có
            QuestManager.Instance.UpdateCraftProgress(selectedPotion.potionData.potionName);
        }

        AudioManager.Instance.PlaySFX("Upgrade");

        // Cập nhật nguyên liệu
        UpdateCraftMaterialsUI(selectedPotion.potionData);

        // Cập nhật UI slot
        selectedSlot.UpdateQuantity();
        ShowPotionInfo(selectedPotion);
    }
}
