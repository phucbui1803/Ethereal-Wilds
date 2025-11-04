using UnityEngine;
using System.Collections;

public class EnemyStats : MonoBehaviour
{
    [Header("Enemy Info")]
    public string enemyName = "Enemy";
    [SerializeField] private int enemyLevel;

    [Header("Base Stats")]
    public float baseHP = 2000f;
    public float baseATK = 150f;
    public float baseDEF = 100f;

    [Header("Current Stats")]
    public float maxHP;
    public float currentHP;
    public float atk;
    public float def;

    [Header("Balance Stats")]
    public float K = 200f;

    [Header("HP Bar UI")]
    public EnemyHPBar hpBarPrefab;
    public Transform hpBarAnchor;
    public Canvas worldCanvas;
    private EnemyHPBar hpBarUI;

    private Coroutine regenCoroutine;
    private bool inCombat = false;
    public bool InCombat => inCombat;

    [Header("Combat Settings")]
    public float regenDelay = 5f; // 5s sau khi mất combat sẽ hồi HP

    private EnemyRespawn respawn;

    public bool IsAlive()
    {
        return currentHP > 0;
    }

    public int GetLevel()
    {
        return enemyLevel;
    }

    private void Start()
    {
        respawn = GetComponent<EnemyRespawn>();

        // ✅ Lấy level từ player
        int playerLevel = PlayerStats.Instance != null ? PlayerStats.Instance.level : 1;
        SetEnemyLevel(GetEnemyLevel(playerLevel));

        // ✅ Spawn HP bar
        CreateHPBar();
    }

    private void CreateHPBar()
    {
        if (hpBarPrefab == null || hpBarAnchor == null || worldCanvas == null)
        {
            Debug.LogWarning($"{name}: hpBarPrefab, hpBarAnchor hoặc worldCanvas chưa được gán!");
            return;
        }

        hpBarUI = Instantiate(hpBarPrefab, worldCanvas.transform);
        UpdateHPBarUI(); // gán thông tin lần đầu
    }

    public void SetEnemyLevel(int level)
    {
        enemyLevel = level; 
        ScaleEnemyStats(enemyLevel);
        UpdateHPBarUI();
    }

    private int GetEnemyLevel(int playerLevel)
    {
        // Player 1-10 => Enemy 10, 11-20 => 20, ... tối đa 90
        int enemyLevel = Mathf.CeilToInt(playerLevel / 10f) * 10;
        return Mathf.Clamp(enemyLevel, 10, 90);
    }

    private void ScaleEnemyStats(int enemyLevel)
    {
        int levelSteps = (enemyLevel / 10) - 1;
        float multiplier = 1f + levelSteps * 0.2f; // +20% mỗi mốc level

        maxHP = baseHP * multiplier;
        atk = baseATK * multiplier;
        def = baseDEF * multiplier;
        currentHP = maxHP;
    }

    private void UpdateHPBarUI()
    {
        if (hpBarUI != null)
        {
            hpBarUI.Initialize(hpBarAnchor, $"Lv.{enemyLevel} {enemyName}", currentHP, maxHP);
        }
    }

    public float TakeDamage(float damage, int playerLevel)
    {
        inCombat = true; // bị đánh => chắc chắn đang combat

        float damageRes = def / (def + K + 10f * playerLevel);
        float finalDamage = damage * (1f - damageRes);

        currentHP = Mathf.Clamp(currentHP - finalDamage, 0, maxHP);
        Debug.Log($"{enemyName} Lv.{enemyLevel} nhận {finalDamage:0.0} sát thương. HP: {currentHP}/{maxHP}");

        hpBarUI?.UpdateHP(currentHP, maxHP);

        if (currentHP <= 0)
            Die();

        return finalDamage;
    }

    public void SetInCombat(bool state)
    {
        if (inCombat == state) return;

        inCombat = state;
        Debug.Log($"{enemyName}: InCombat = {state}");

        if (state)
        {
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
                regenCoroutine = null;
            }
        }
        else
        {
            if (regenCoroutine != null)
            {
                StopCoroutine(regenCoroutine);
            }

            regenCoroutine = StartCoroutine(RegenHPAfterDelay());
        }
    }

    public void OnPlayerOutOfRange()
    {
        SetInCombat(false); // tái sử dụng luôn hàm này, tránh duplicate
    }

    private IEnumerator RegenHPAfterDelay()
    {
        yield return new WaitForSeconds(regenDelay);

        // Nếu trong lúc chờ 5s mà enemy lại combat thì hủy
        if (inCombat) yield break;

        currentHP = maxHP;
        Debug.Log($"{enemyName} hồi phục đầy máu sau {regenDelay}s vì player đã bỏ đi.");
        hpBarUI?.UpdateHP(currentHP, maxHP);
    }

    private void Die()
    {
        Debug.Log($"{enemyName} Lv.{enemyLevel} đã chết.");

        // Gọi hệ thống loot
        EnemyLootDrop loot = GetComponent<EnemyLootDrop>();

        QuestManager.Instance.UpdateKillProgress(enemyName);

        if (loot != null)
        {
            loot.DropLootToPlayer();
            Debug.Log($"[EnemyStats] {enemyName} rơi vật phẩm cho người chơi!");
        }

        if (hpBarUI != null)
        {
            hpBarUI.FadeOutAndDestroy(0.5f); // ✅ fade-out mượt trong 0.5s
            hpBarUI = null;
        }

        if (respawn != null)
        {
            respawn.Die(() =>
            {
                // Reset chỉ số khi respawn
                int playerLevel = PlayerStats.Instance != null ? PlayerStats.Instance.level : 1;
                SetEnemyLevel(GetEnemyLevel(playerLevel));
                CreateHPBar();
            });
        }
    }

    public float CalculateDamage()
    {
        return atk;
    }
}
