using System.Buffers.Text;
using UnityEngine;

public enum SkillType
{
    NormalAttack,
    SkillQ,
    SkillE,
    SkillR
}

[CreateAssetMenu(fileName = "New Skill", menuName = "Skill System/Skill")]
public class SkillData : ScriptableObject
{
    public Sprite icon;
    public string skillName;
    public float baseSkillMultiplier;
    public float skillMultiplier;
    public float cooldown;
    public float energyMax;
    public float currentEnergy;
    public float energyRegen;
    public float timeBuff;

    public string skillInfo;

    public int level = 1;
    public int maxLevel = 10;
    public float upgradePercent;

    private void OnValidate()
    {
        RecalculateStats();
    }

    public void UpgradeLevel()
    {
        if (level >= maxLevel) return;
        level++;
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        skillMultiplier = baseSkillMultiplier * Mathf.Pow(1f + upgradePercent, level - 1);
    }

    // Trả về mô tả với {multiplier} được thay bằng % và bọc rich-text color (TextMeshPro hỗ trợ)
    public string GetFormattedDescription()
    {
        string desc = string.IsNullOrEmpty(skillInfo) ? "" : skillInfo;

        // Chuẩn bị các giá trị với màu
        string coloredMultiplier = $"<color=#FF6B6B>{(skillMultiplier * 100f):F0}%</color>"; // 150%
        string coloredEnergyRegen = $"<color=#4CAF50>{energyRegen:F0}</color>";
        string coloredEnergy = $"<color=#FF6B6B>{energyMax:F0}</color>";
        string coloredTimeBuff = $"<color=#4CAF50>{timeBuff:F0}</color>";

        // Thay thế placeholder trong chuỗi
        if (desc.Contains("{skillMultiplier}"))
            desc = desc.Replace("{skillMultiplier}", coloredMultiplier);

        if (desc.Contains("{energyRegen}"))
            desc = desc.Replace("{energyRegen}", coloredEnergyRegen);

        if (desc.Contains("{energy}"))
            desc = desc.Replace("{energy}", coloredEnergy);

        if (desc.Contains("{timeBuff}"))
            desc = desc.Replace("{timeBuff}", coloredTimeBuff);

        return desc;
    }

}
