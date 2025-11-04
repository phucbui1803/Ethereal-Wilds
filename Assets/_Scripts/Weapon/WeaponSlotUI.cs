using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class WeaponSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    public Button slotButton;
    public RawImage weaponImage;
    public Image weaponGlow;
    public Image lockIcon;
    public TMP_Text levelText;

    [HideInInspector] 
    public WeaponData weaponData;

    [HideInInspector] 
    public WeaponPanelUI panelUI;

    private bool isSelected = false;

    private void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);

        if (weaponGlow != null)
            weaponGlow.enabled = false;

        if (lockIcon != null)
            lockIcon.enabled = false;
    }

    public void SetWeapon(WeaponData data)
    {
        weaponData = data;

        if (levelText != null) 
            levelText.text = $"Lv. {weaponData.currentLevel}";

        // Nếu chưa có RenderTexture → render ngay
        if (weaponData.renderTexture == null && weaponData.weaponPrefab != null && panelUI != null)
        {
            weaponData.renderTexture = panelUI.previewGenerator.GeneratePreview(weaponData.weaponPrefab, 128);
        }

        UpdateWeaponImage();

        if (weaponGlow != null)
            weaponGlow.enabled = false;

        UpdateLockIcon();
    }

    public void UpdateWeaponImage() 
    {
        if (weaponImage != null && weaponData != null && weaponData.renderTexture != null)
            weaponImage.texture = weaponData.renderTexture;
    }

    private void OnSlotClicked()
    {
        AudioManager.Instance.PlaySFX("Click");

        if (weaponData == null || panelUI == null) 
            return;

        //if (!weaponData.isUnlocked)
        //{
        //    Debug.Log($"{weaponData.weaponName} đang bị khóa!");
        //    return;
        //}

        panelUI.SelectWeaponSlot(this);
        panelUI.ShowWeaponInfo(weaponData);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (weaponGlow != null)
            weaponGlow.enabled = selected;
    }

    public void SetGlow(bool glow)
    {
        if (!isSelected && weaponGlow != null)
            weaponGlow.enabled = glow;
    }

    public void OnPointerEnter(PointerEventData eventData) => SetGlow(true);
    public void OnPointerExit(PointerEventData eventData) => SetGlow(false);

    private void UpdateLockIcon()
    {
        if (lockIcon != null && weaponData != null)
            lockIcon.enabled = !weaponData.isUnlocked;
    }

    public void RefreshLockState()
    {
        UpdateLockIcon();
    }
}
