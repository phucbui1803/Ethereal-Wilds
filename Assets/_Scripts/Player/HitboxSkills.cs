using UnityEngine;
using System.Collections.Generic;

public class HitboxSkills : MonoBehaviour
{
    public enum HitboxType {Attack, Q, E, R}
    public HitboxType hitboxType;   

    [Header("Stats")]
    public float lifeTime = 0.2f;
    private float damage;
    private bool isCrit;
    private int playerLevel;

    private readonly HashSet<Collider> hitEnemies = new HashSet<Collider>();

    // Prefab TextMeshPro 3D để hiển thị damage
    public GameObject damageTextPrefab;

    public void Init(float dmg, bool crit, int level, HitboxType type, float customLifeTime = -1)
    {
        damage = dmg;
        isCrit = crit;
        playerLevel = level;
        hitboxType = type;

        if (customLifeTime > 0)
        {
            lifeTime = customLifeTime;
        }

        // Xóa hitbox sau thời gian lifeTime
        Destroy(gameObject, lifeTime);
    }

    private void OnEnable()
    {
        hitEnemies.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy"))
        {
            return;
        }

        // Ngăn không cho cùng 1 enemy bị trúng nhiều lần
        if (hitEnemies.Contains(other))
        {
            return;
        }

        hitEnemies.Add(other);
        EnemyStats enemy = other.GetComponent<EnemyStats>();

        if (enemy != null)
        {
            float finalDamage = enemy.TakeDamage(damage, playerLevel);

            // Hiển thị floating damage
            if (damageTextPrefab != null)
            {
                Vector3 dmgPos = other.transform.position + Vector3.up;
                GameObject dmgText = Instantiate(damageTextPrefab, dmgPos, Quaternion.identity);
                var ui = dmgText.GetComponent<FloatDamageUI>();

                if (ui != null)
                {
                    Color color = isCrit ? Color.red : new Color(1f, 0.55f, 0f); // đỏ hoặc dark-orange

                    // Gọi Setup để random vị trí quanh enemy và set text
                    ui.Setup(enemy.transform, finalDamage, color, isCrit, false);
                }
            }
        }
    }

    public static void SpawnHitbox(GameObject prefab, Transform player,
                                   Transform attackPoint, Transform rPoint,
                                   HitboxType type, float dmg, bool crit, int level,
                                   float lifeTime = 0.2f, Vector3? worldPosOverride = null)
    {
        Vector3 spawnPos;
        Quaternion spawnRot;

        switch (type)
        {
            case HitboxType.Attack:
                spawnPos = attackPoint.position;
                spawnRot = attackPoint.rotation;
                break;

            case HitboxType.E:
                spawnPos = worldPosOverride.HasValue ? worldPosOverride.Value : player.position;
                spawnRot = Quaternion.identity;
                break;

            case HitboxType.R:
                spawnPos = rPoint.position;
                spawnRot = rPoint.rotation;
                break;

            default:
                spawnPos = player.position + player.forward * 1.5f;
                spawnRot = player.rotation;
                break;
        }

        GameObject hitboxObj = GameObject.Instantiate(prefab, spawnPos, spawnRot);
        hitboxObj.GetComponent<HitboxSkills>()?.Init(dmg, crit, level, type, lifeTime);
    }

#if UNITY_EDITOR
    // Debug Gizmo để thấy vùng hitbox trong Scene
    void OnDrawGizmos()
    {
        Gizmos.color = hitboxType switch
        {
            HitboxType.Attack => Color.green, // normal attack màu xanh dương
            HitboxType.E => Color.yellow,    // skill E màu vàng
            HitboxType.R => Color.red,       // skill R màu đỏ
            _ => Color.white
        };

        Collider col = GetComponent<Collider>();
        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireSphere(sphere.center, sphere.radius);
        }
    }
#endif
}
