using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillPanelUI : MonoBehaviour
{
    [Header("Skill Data")]
    public SkillData normalAttackSkill;
    public SkillData skillQ;
    public SkillData skillE;
    public SkillData skillR;

    [Header("UI References")]
    public TMP_Text skillNameText;
    public TMP_Text levelText;
    public TMP_Text cooldownText;
    public TMP_Text energyText;
    public TMP_Text energyRegenText;
    public TMP_Text descriptionText;

    public GameObject skillInfoPanel; // panel chứa thông tin skill
    public Button upgradeButton;      // nút nâng cấp skill
    public TMP_Text upgradeText;

    [Header("Upgrade Requirement")]
    public ItemData upgradeMaterial;   // Nguyên liệu nâng cấp skill
    public int costPerLevel = 1;       // Số lượng cần mỗi lần nâng cấp
    public TMP_Text upgradeRequirementText;
    public Image upgradeRequirementIcon;

    private SkillType selectedSkillType;

    private void Start()
    {
        // Ẩn panel khi bắt đầu game
        skillInfoPanel.SetActive(false);

        // Gán sự kiện cho nút upgrade
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
    }

    // --- Gọi từ các button ---
    public void OnSkillButtonClicked(int skillTypeIndex)
    {
        AudioManager.Instance.PlaySFX("Click");

        selectedSkillType = (SkillType)skillTypeIndex;
        ShowSkillInfo(selectedSkillType);
    }

    public void ShowSkillInfo(SkillType type)
    {
        SkillData selectedSkill = GetSkillData(type);

        if (selectedSkill == null)
        {
            Debug.LogWarning("Skill chưa được gán trong SkillPanelUI!");
            return;
        }

        // Hiển thị dữ liệu skill
        skillNameText.text = selectedSkill.skillName;
        levelText.text = $"{selectedSkill.level}/{selectedSkill.maxLevel}";
        cooldownText.text = $"{selectedSkill.cooldown:F0}s";
        energyText.text = $"{selectedSkill.energyMax:F0}";
        energyRegenText.text = $"{selectedSkill.energyRegen:F0}";

        // Dùng hàm GetFormattedDescription() để hiển thị mô tả
        descriptionText.text = selectedSkill.GetFormattedDescription();

        UpdateUpgradeRequirement(selectedSkill);

        skillInfoPanel.SetActive(true);
    }

    public void HideSkillInfo()
    {
        skillInfoPanel.SetActive(false);
    }

    private SkillData GetSkillData(SkillType type)
    {
        switch (type)
        {
            case SkillType.NormalAttack: return normalAttackSkill;
            case SkillType.SkillQ: return skillQ;
            case SkillType.SkillE: return skillE;
            case SkillType.SkillR: return skillR;
            default: return null;
        }
    }

    private void OnUpgradeButtonClicked()
    {
        // Âm thanh click trước
        AudioManager.Instance.PlaySFX("Click");

        SkillData selectedSkill = GetSkillData(selectedSkillType);
        if (selectedSkill == null) return;

        int currentCount = InventoryManager.Instance.GetItemCount(upgradeMaterial);
        if (currentCount < costPerLevel) return;
        if (selectedSkill.level >= selectedSkill.maxLevel) return;

        InventoryManager.Instance.RemoveItem(upgradeMaterial, costPerLevel);
        selectedSkill.UpgradeLevel();

        // Âm thanh level up skill
        AudioManager.Instance.PlaySFX("Upgrade");

        ShowSkillInfo(selectedSkillType);
    }

    private void UpdateUpgradeRequirement(SkillData skill)
    {
        if (upgradeRequirementText == null || upgradeMaterial == null || skill == null)
            return;

        int currentAmount = InventoryManager.Instance.GetItemCount(upgradeMaterial);

        if (upgradeRequirementIcon != null && upgradeMaterial.icon != null)
        {
            upgradeRequirementIcon.sprite = upgradeMaterial.icon;
            upgradeRequirementIcon.enabled = true;
        }

        if (skill.level >= skill.maxLevel)
        {
            // Nút Upgrade hiển thị Max Level
            if (upgradeText != null)
                upgradeText.text = "Max Level";

            upgradeButton.interactable = false;

            // Ẩn requirement bên dưới
            upgradeRequirementText.text = "";

            if (upgradeRequirementIcon != null)
                upgradeRequirementIcon.enabled = false;

            return;
        }

        // Nếu chưa max thì nút là "Upgrade"
        if (upgradeText != null)
            upgradeText.text = "Upgrade";

        if (currentAmount < costPerLevel)
        {
            upgradeRequirementText.text = $"<color=red>{currentAmount}</color>/{costPerLevel}";
            upgradeButton.interactable = false;
        }
        else
        {
            upgradeRequirementText.text = $"<color=green>{currentAmount}</color>/{costPerLevel}";
            upgradeButton.interactable = true;
        }
    }
}
