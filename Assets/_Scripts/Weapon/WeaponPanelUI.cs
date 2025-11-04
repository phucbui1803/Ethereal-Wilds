using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponPanelUI : MonoBehaviour
{
    [Header("Inventory References")]
    public GameObject weaponSlotPrefab;
    public Transform weaponGridParent;

    [Header("Info Panel References")]
    public RawImage previewRawImage;
    public TMP_Text weaponNameText;
    public TMP_Text levelText;
    public TMP_Text hpText;
    public TMP_Text atkText;
    public TMP_Text defText;
    public TMP_Text critRateText;
    public TMP_Text critDamageText;
    public Transform weaponPreviewHolder;

    [Header("Weapon Buttons")]
    public Button equipBtn;
    public Button actionBtn;
    public TMP_Text actionBtnText;

    [Header("Requirement UI")]
    public TMP_Text requirementText;
    public Image requirementIcon;

    [Header("Preview Settings")]
    public float previewScale = 1f;
    public Vector3 previewRotation = new Vector3(0, 180, 0);

    [Header("Preview System")]
    public WeaponPreviewCamera previewCamera;
    public WeaponPreviewGenerator previewGenerator;

    private List<WeaponSlotUI> weaponSlots = new List<WeaponSlotUI>();
    private WeaponSlotUI selectedSlot = null;
    private WeaponData selectedWeapon;
    private List<WeaponData> currentWeapons = new List<WeaponData>();

    public static WeaponPanelUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //previewCamera = FindFirstObjectByType<WeaponPreviewCamera>();

        equipBtn.onClick.AddListener(OnEquipButtonClicked);
        actionBtn.onClick.AddListener(OnActionButtonClicked);
    }

    //public void RefreshPanel()
    //{
    //    if (selectedWeapon != null) StartCoroutine(GeneratePreviewDelayed(selectedWeapon));
    //}

    public void ShowWeapons(List<WeaponData> weapons)
    {
        foreach (Transform child in weaponGridParent)
            Destroy(child.gameObject);

        weaponSlots.Clear();
        currentWeapons.Clear();

        if (weapons != null) currentWeapons.AddRange(weapons);

        if (weapons == null || weapons.Count == 0)
        {
            ClearInfoPanel(); 
            return;
        }

        // Render tất cả slot trước khi hiển thị
        foreach (var weapon in weapons)
        {
            if (weapon == null) continue;

            // Tạo slot UI
            GameObject slotGO = Instantiate(weaponSlotPrefab, weaponGridParent);
            WeaponSlotUI slotUI = slotGO.GetComponent<WeaponSlotUI>();

            if (slotUI != null)
            {
                slotUI.panelUI = this;

                // Render prefab nếu chưa có RenderTexture
                if (weapon.renderTexture == null && weapon.weaponPrefab != null)
                    weapon.renderTexture = previewGenerator.GeneratePreview(weapon.weaponPrefab, 128);

                slotUI.SetWeapon(weapon);
                weaponSlots.Add(slotUI);
            }
        }

        if (weaponSlots.Count > 0)
        {
            SelectWeaponSlot(weaponSlots[0]);
        }
    }

    public void SelectWeaponSlot(WeaponSlotUI slot)
    {
        if (slot == null) return;

        if (selectedSlot != null && selectedSlot != slot) 
            selectedSlot.SetSelected(false);

        selectedSlot = slot;

        selectedSlot.SetSelected(true);

        foreach (var s in weaponSlots) 
            if (s != selectedSlot) 
                s.SetSelected(false); 

        if (slot.weaponData != null) 
            ShowWeaponInfo(slot.weaponData);
    }

    public void ShowWeaponInfo(WeaponData weapon)
    {
        if (weapon == null) return;

        selectedWeapon = weapon;
        weaponNameText.text = weapon.weaponName;
        levelText.text = $"{weapon.currentLevel}/{weapon.maxLevel}";
        hpText.text = weapon.currentHP.ToString("F0");
        atkText.text = weapon.currentAtk.ToString("F0");
        defText.text = weapon.currentDef.ToString("F0");
        critRateText.text = $"{weapon.currentCritRate * 100f:F1}%";
        critDamageText.text = $"{weapon.currentCritDamage * 100f:F1}%";

        UpdateActionRequirementUI();

        // Gán texture đã render sẵn
        if (weapon.renderTexture != null)
            previewRawImage.texture = weapon.renderTexture;
    }

    //private IEnumerator GeneratePreviewDelayed(WeaponData weapon)
    //{
    //    yield return null;

    //    if (previewCamera == null) previewCamera = FindFirstObjectByType<WeaponPreviewCamera>();
    //    if (previewCamera == null || weaponPreviewHolder == null || weapon.weaponPrefab == null) yield break;

    //    foreach (Transform child in weaponPreviewHolder)
    //        Destroy(child.gameObject);

    //    GameObject temp = Instantiate(weapon.weaponPrefab, weaponPreviewHolder);
    //    temp.transform.localPosition = Vector3.zero; 
    //    temp.transform.localRotation = Quaternion.Euler(previewRotation); 
    //    temp.transform.localScale = Vector3.one * previewScale;

    //    int previewLayer = LayerMask.NameToLayer("WeaponPreview"); 
    //    SetLayerRecursively(temp, previewLayer); 

    //    yield return null; // đợi 1 frame để object được render

    //    RenderTexture rt = previewCamera.GetRenderTexture(); 
    //    weapon.renderTexture = rt; 
    //    previewRawImage.texture = rt;
    //}

    //private void SetLayerRecursively(GameObject obj, int layer)
    //{
    //    obj.layer = layer;
    //    foreach (Transform child in obj.transform)
    //        SetLayerRecursively(child.gameObject, layer);
    //}

    private void ClearInfoPanel()
    {
        selectedWeapon = null;
        weaponNameText.text = "";
        levelText.text = "";
        hpText.text = "";
        atkText.text = "";
        defText.text = "";
        critRateText.text = "";
        critDamageText.text = "";
        if (previewRawImage != null) previewRawImage.texture = null;
    }

    private void OnEquipButtonClicked()
    {
        // Âm thanh click
        AudioManager.Instance.PlaySFX("Click");

        if (selectedWeapon == null || PlayerStats.Instance == null)
        {
            Debug.LogWarning("[WeaponPanel] Select weapon and ensure PlayerStats exists!");
            return;
        }

        PlayerInventory.Instance.EquipWeapon(selectedWeapon);
        Debug.Log($"Equipped {selectedWeapon.weaponName} to Player");
    }

    private void OnActionButtonClicked()
    {
        // Âm thanh click trước
        AudioManager.Instance.PlaySFX("Click");

        if (selectedWeapon == null) return;

        // Nếu chưa unlock -> thử unlock
        if (!selectedWeapon.isUnlocked)
        {
            int current = InventoryManager.Instance.GetItemCount(selectedWeapon.unlockMaterial);
            if (current >= selectedWeapon.unlockCost)
            {
                InventoryManager.Instance.RemoveItem(selectedWeapon.unlockMaterial, selectedWeapon.unlockCost);
                selectedWeapon.isUnlocked = true;
                Debug.Log($"Unlocked weapon {selectedWeapon.weaponName}");

                // Refresh slot UI sau khi unlock
                if (selectedSlot != null)
                    selectedSlot.RefreshLockState();
            }
        }
        else
        {
            // Nếu unlock rồi -> upgrade
            if (selectedWeapon.currentLevel < selectedWeapon.maxLevel)
            {
                int current = InventoryManager.Instance.GetItemCount(selectedWeapon.upgradeMaterial);
                if (current >= selectedWeapon.upgradeCostPerLevel)
                {
                    InventoryManager.Instance.RemoveItem(selectedWeapon.upgradeMaterial, selectedWeapon.upgradeCostPerLevel);
                    selectedWeapon.UpgradeLevel();
                    Debug.Log($"Weapon {selectedWeapon.weaponName} upgraded to level {selectedWeapon.currentLevel}");
                }
            }
        }

        // Âm thanh level up skill
        AudioManager.Instance.PlaySFX("Upgrade");

        ShowWeaponInfo(selectedWeapon);
    }

    private void UpdateActionRequirementUI()
    {
        if (selectedWeapon == null || requirementText == null || actionBtnText == null || equipBtn == null) 
            return;

        // Chưa unlock
        if (!selectedWeapon.isUnlocked)
        {
            // Action button: unlock
            actionBtnText.text = "Unlock";

            // Equip button: Locked và disable
            equipBtn.interactable = false;
            equipBtn.GetComponentInChildren<TMP_Text>().text = "Locked";

            // Hiển thị requirement
            int current = InventoryManager.Instance.GetItemCount(selectedWeapon.unlockMaterial);
            int required = selectedWeapon.unlockCost;

            requirementIcon.sprite = selectedWeapon.unlockMaterial.icon;
            requirementIcon.enabled = true;

            requirementText.text = current < required
                ? $"<color=red>{current}</color>/{required}"
                : $"<color=green>{current}</color>/{required}";

            actionBtn.interactable = current >= required;
        }
        else
        {
            // Weapon đã unlock => equip và upgrade
            equipBtn.interactable = true;
            equipBtn.GetComponentInChildren<TMP_Text>().text = "Equip";

            // Upgrade action button
            if (selectedWeapon.currentLevel >= selectedWeapon.maxLevel)
            {
                actionBtnText.text = "Max Level";
                requirementText.text = "";
                actionBtn.interactable = false;
                requirementIcon.enabled = false;
            }
            else
            {
                actionBtnText.text = "Upgrade";

                int current = InventoryManager.Instance.GetItemCount(selectedWeapon.upgradeMaterial);
                int required = selectedWeapon.upgradeCostPerLevel;

                requirementIcon.sprite = selectedWeapon.upgradeMaterial.icon;
                requirementIcon.enabled = true;

                requirementText.text = current < required
                    ? $"<color=red>{current}</color>/{required}"
                    : $"<color=green>{current}</color>/{required}";

                actionBtn.interactable = current >= required;
            }
        }
    }
}
