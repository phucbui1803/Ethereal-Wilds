using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Basic Info")]
    public string weaponName;
    public GameObject weaponPrefab;

    [HideInInspector] 
    public RenderTexture renderTexture;

    [Header("Base Stats")]
    public float baseHP;
    public float baseAtk;
    public float baseDef;
    public float baseCritRate;    // 0-1
    public float baseCritDamage;  // 0-1

    [Header("Leveling")]
    public int maxLevel = 90;
    public int currentLevel = 1;
    public float upgradePercent = 0.03f;

    [Header("Unlock Settings")]
    public bool isUnlocked = false;   // <--- thêm biến này
    public ItemData unlockMaterial;   // vật phẩm cần để mở khóa
    public int unlockCost = 100;       // số lượng vật phẩm cần

    [Header("Upgrade Settings")]
    public ItemData upgradeMaterial;  // vật phẩm nâng cấp
    public int upgradeCostPerLevel = 10;

    [Header("Current Stats (calculated)")]
    public float currentHP { get; private set; }
    public float currentAtk { get; private set; }
    public float currentDef { get; private set; }
    public float currentCritRate { get; private set; }
    public float currentCritDamage { get; private set; }

    private void OnEnable()
    {
        RecalculateStats();
    }

    public void UpgradeLevel()
    {
        if (currentLevel >= maxLevel) return;
        currentLevel++;
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        currentHP = baseHP * Mathf.Pow(1f + upgradePercent, currentLevel - 1);
        currentAtk = baseAtk * Mathf.Pow(1f + upgradePercent, currentLevel - 1);
        currentDef = baseDef * Mathf.Pow(1f + upgradePercent, currentLevel - 1);
        currentCritRate = baseCritRate * Mathf.Pow(1f + upgradePercent, currentLevel - 1);
        currentCritDamage = baseCritDamage * Mathf.Pow(1f + upgradePercent, currentLevel - 1);
    }

    // Getter current stats
    public float GetHP() => currentHP;
    public float GetATK() => currentAtk;
    public float GetDEF() => currentDef;
    public float GetCritRate() => currentCritRate;
    public float GetCritDamage() => currentCritDamage;
}
