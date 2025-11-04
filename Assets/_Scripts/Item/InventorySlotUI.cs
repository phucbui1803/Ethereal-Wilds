using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Button slotButton;
    public Image itemIcon;
    public Image itemGlow;
    public TMP_Text quantityText;

    [HideInInspector]
    public ItemData itemData;
    [HideInInspector]
    public InventoryUI panelUI;

    private bool isSelected = false;

    private void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);

        if (itemGlow != null)
            itemGlow.enabled = false;
    }

    public void SetItem(ItemData data, int quantity)
    {
        itemData = data;

        if (itemIcon != null)
            itemIcon.sprite = itemData.icon;

        if (quantityText != null)
            quantityText.text = quantity.ToString();

        if (itemGlow != null)
            itemGlow.enabled = false;
    }

    private void OnSlotClicked()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (itemData == null || panelUI == null) return;
        panelUI.ShowItemInfo(itemData);
        panelUI.SelectItemSlot(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (itemGlow != null)
            itemGlow.enabled = selected;
    }

    public void SetGlow(bool glow)
    {
        if (!isSelected && itemGlow != null)
            itemGlow.enabled = glow;
    }

    public void OnPointerEnter(PointerEventData eventData) => SetGlow(true);
    public void OnPointerExit(PointerEventData eventData) => SetGlow(false);
}
