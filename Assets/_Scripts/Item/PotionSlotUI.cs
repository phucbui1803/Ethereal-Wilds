using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PotionSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Button slotButton;
    public Image potionIcon;
    public Image glowImage;
    public TMP_Text quantityText;

    [HideInInspector]
    public PotionSlot potionSlot;   // reference đến slot thật
    [HideInInspector]
    public PotionPanelUI panelUI;

    private bool isSelected = false;

    private void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);

        if (glowImage != null)
            glowImage.enabled = false;
    }

    // Thiết lập slot UI với PotionSlot
    public void SetPotion(PotionSlot slot)
    {
        if (slot == null) return;

        potionSlot = slot;

        if (potionIcon != null)
            potionIcon.sprite = slot.potionData.icon;

        UpdateQuantity();

        if (glowImage != null)
            glowImage.enabled = false;
    }

    private void OnSlotClicked()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (potionSlot == null || panelUI == null) return;

        panelUI.SelectPotionSlot(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        if (glowImage != null)
            glowImage.enabled = selected;
    }

    public void SetGlow(bool glow)
    {
        if (!isSelected && glowImage != null)
            glowImage.enabled = glow;
    }

    public void OnPointerEnter(PointerEventData eventData) => SetGlow(true);
    public void OnPointerExit(PointerEventData eventData) => SetGlow(false);

    // Cập nhật số lượng UI
    public void UpdateQuantity()
    {
        if (quantityText != null && potionSlot != null)
            quantityText.text = potionSlot.quantity.ToString();
    }
}
