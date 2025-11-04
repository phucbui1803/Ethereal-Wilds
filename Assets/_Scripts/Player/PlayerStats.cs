using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    public float baseHP = 1200f;
    public float baseAtk = 30f;
    public float baseDef = 50f;
    public float baseCritRate = 0.1f; // 10%
    public float baseCritDamage = 1.5f;

    [Header("Current Stats")]
    public float maxHP;
    public float atk;
    public float def;
    public float currentHP;
    public float critRate;
    public float critDamage;

    [Header("HP / Stamina Regen Tick Settings")]
    public float hpRegen = 100f;
    private float regenTimer = 0f;
    public float hpRegenTickAmount = 100f;
    public float hpRegenTickInterval = 5f;
    private float hpRegenTimer = 0f;
    public float maxStamina = 100f;
    public float currentStamina;
    public float staminaRegen = 10f;
    public float regenDelay = 0.5f;

    [Header("Weapon")]
    public WeaponData weaponData;

    [Header("Character Name")]
    public string characterName = "Kachujin";

    [Header("XP / Level")]
    public int level = 1;
    public int maxLevel = 90;
    public float currentXP = 0f;
    public float xpToNextLevel = 100f; // XP cần cho level tiếp theo
    public float xpGrowthRate = 1.05f;  // Hệ số tăng XP mỗi level

    public GameObject xpTextPrefab;

    private Animator animator;
    private bool isDead = false;
    public bool IsDead => isDead;

    // ===============================
    // BUFF SYSTEM
    // ===============================
    private float currentBuffAtkPercent = 0f;
    private float currentBuffDefPercent = 0f;
    private float currentBuffCritRate = 0f;
    private float currentBuffCritDamage = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        maxHP = baseHP;
        currentHP = maxHP;
        currentStamina = maxStamina;
        RecalculateStats();
    }

    void Update()
    {
        if (isDead) return;
        RegenerateHP();
        RegenerateStamina();
    }

    #region HP & Stamina
    public float TakeDamage(float damage)
    {
        if (isDead) return 0f;

        float damageRes = def / (def * 1.5f + 200f + 10f * level);
        float finalDamage = damage * (1f - damageRes);

        currentHP -= finalDamage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"Player nhận {finalDamage:F2} sát thương (giảm {damageRes * 100f:F2}%). HP: {currentHP}/{maxHP}");

        AudioManager.Instance.PlaySFX("GetHit");

        if (currentHP <= 0)
            Die();

        return finalDamage;
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
    }

    private void RegenerateHP()
    {
        if (currentHP >= maxHP) return;

        hpRegenTimer += Time.deltaTime;

        if (hpRegenTimer >= hpRegenTickInterval)
        {
            currentHP += hpRegenTickAmount;
            currentHP = Mathf.Min(currentHP, maxHP);

            Debug.Log($"HP regen tick: +{hpRegenTickAmount}, Current HP: {currentHP}/{maxHP}");

            hpRegenTimer = 0f;
        }
    }

    private void Die()
    {
        if (isDead) return; // tránh gọi lại nhiều lần
        isDead = true;

        Debug.Log("Player died!");

        if (animator != null)
            animator.SetTrigger("Death");

        AudioManager.Instance.PlaySFX("Death");

        // Tắt controller
        var controller = GetComponent<ThirdPersonController>();
        if (controller != null) controller.enabled = false;

        // Tắt input
        //var input = GetComponent<StarterAssetsInputs>();
        //if (input != null) input.enabled = false;
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
            regenTimer = regenDelay;
            return true;
        }
        Debug.Log("Not enough stamina!");
        return false;
    }

    private void RegenerateStamina()
    {
        if (currentStamina >= maxStamina) return;

        if (regenTimer > 0f)
        {
            regenTimer -= Time.deltaTime;
            return;
        }

        currentStamina = Mathf.Clamp(currentStamina + staminaRegen * Time.deltaTime, 0, maxStamina);
    }
    #endregion

    #region Combat
    public struct DamageInfo
    {
        public float damage;
        public bool isCrit;
    }

    public DamageInfo CalculateDamage()
    {
        bool isCrit = Random.value < critRate;
        float dmg = atk;
        if (isCrit) dmg *= critDamage;
        return new DamageInfo { damage = dmg, isCrit = isCrit };
    }
    #endregion

    #region Weapon
    public void EquipWeapon(WeaponData weapon)
    {
        weaponData = weapon;
        RecalculateStats();
    }

    private void RecalculateStats()
    {
        // Base stats + tăng theo level
        maxHP = baseHP + baseHP * 0.1f * (level - 1);
        atk = baseAtk + baseAtk * 0.1f * (level - 1);
        def = baseDef + baseDef * 0.1f * (level - 1);
        critRate = baseCritRate;
        critDamage = baseCritDamage;

        // Cộng chỉ số vũ khí
        if (weaponData != null)
        {
            maxHP += weaponData.currentHP;
            atk += weaponData.currentAtk;
            def += weaponData.currentDef;
            critRate = Mathf.Clamp01(critRate + weaponData.currentCritRate);
            critDamage += weaponData.currentCritDamage;
        }

        // Áp buff
        atk += atk * currentBuffAtkPercent;
        def += def * currentBuffDefPercent;
        critRate = Mathf.Clamp01(critRate + currentBuffCritRate);
        critDamage += currentBuffCritDamage;

        float oldMaxHP = maxHP;

        //Nếu maxHP tăng, hồi đầy máu
        if (maxHP > oldMaxHP)
            currentHP = maxHP;
        else
            currentHP = Mathf.Min(currentHP, maxHP);

        currentStamina = Mathf.Min(currentStamina, maxStamina);
    }
    #endregion

    #region Buff / Debuff
    // ==== ATK BUFF ====
    public void ApplyAttackBuff(float percent)
    {
        currentBuffAtkPercent += percent;
        RecalculateStats();
        Debug.Log($"[Buff] +{percent * 100f}% ATK. ATK hiện tại: {atk:F1}");
    }

    public void RemoveAttackBuff(float percent)
    {
        currentBuffAtkPercent -= percent;
        RecalculateStats();
        Debug.Log($"[Buff] Hết buff ATK. ATK hiện tại: {atk:F1}");
    }

    // ==== DEF BUFF ====
    public void ApplyDefenseBuff(float percent)
    {
        currentBuffDefPercent += percent;
        RecalculateStats();
        Debug.Log($"[Buff] +{percent * 100f}% DEF. DEF hiện tại: {def:F1}");
    }

    public void RemoveDefenseBuff(float percent)
    {
        currentBuffDefPercent -= percent;
        RecalculateStats();
        Debug.Log($"[Buff] Hết buff DEF. DEF hiện tại: {def:F1}");
    }

    // ==== CRIT RATE BUFF ====
    public void ApplyCritRateBuff(float amount)
    {
        currentBuffCritRate += amount;
        RecalculateStats();
        Debug.Log($"[Buff] +{amount * 100f}% CritRate. Crit hiện tại: {critRate * 100f}%");
    }

    public void RemoveCritRateBuff(float amount)
    {
        currentBuffCritRate -= amount;
        RecalculateStats();
        Debug.Log($"[Buff] Hết buff CritRate. Crit hiện tại: {critRate * 100f}%");
    }

    // ==== CRIT DAMAGE BUFF ====
    public void ApplyCritDamageBuff(float amount)
    {
        currentBuffCritDamage += amount;
        RecalculateStats();
        Debug.Log($"[Buff] +{amount * 100f}% CritDamage. CritDamage hiện tại: {critDamage * 100f}%");
    }

    public void RemoveCritDamageBuff(float amount)
    {
        currentBuffCritDamage -= amount;
        RecalculateStats();
        Debug.Log($"[Buff] Hết buff CritDamage. CritDamage hiện tại: {critDamage * 100f}%");
    }
    #endregion

    #region XP & Level Up
    public void AddXP(float amount)
    {
        if (level >= maxLevel) return;

        currentXP += amount;
        Debug.Log($"Nhận {amount} XP. Tổng XP: {currentXP}/{xpToNextLevel}");

        // Spawn popup XP tại vị trí player
        if (xpTextPrefab != null)
        {
            GameObject go = Instantiate(xpTextPrefab);
            go.GetComponent<FloatXPUI>().Setup(transform.position, (int)amount, Color.green);
        }

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void UpdateStatsByLevel()
    {
        maxHP = baseHP + baseHP * 0.1f * (level - 1);
        atk = baseAtk + baseAtk * 0.1f * (level - 1);
        def = baseDef + baseDef * 0.1f * (level - 1);

        // Clamp currentHP nếu vượt max
        currentHP = Mathf.Min(currentHP, maxHP);
    }

    private void LevelUp()
    {
        if (level >= maxLevel)
        {
            currentXP = 0; // Nếu muốn, reset XP khi đạt max
            Debug.Log("Player đã đạt level tối đa!");
            return;
        }

        level++;
        xpToNextLevel *= xpGrowthRate;

        // Cập nhật chỉ số theo level
        UpdateStatsByLevel();

        // Full HP khi level up
        currentHP = maxHP;

        Debug.Log($"Level Up! Player lên level {level}. HP: {maxHP}, ATK: {atk}, DEF: {def}");

        // Phát âm thanh khi lên cấp
        AudioManager.Instance.PlaySFX("Level Up");
    }
    #endregion
}
