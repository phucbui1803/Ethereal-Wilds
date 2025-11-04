using UnityEngine;
using System.Collections.Generic;

public class EnemyHitboxSkills : MonoBehaviour
{
    public enum HitboxType { Melee, Ranged, AOE }

    private float damage;
    private float lifeTime = 0.2f;
    private HashSet<Collider> hitPlayers = new HashSet<Collider>();

    public GameObject damageTextPrefab;

    public void Init(float dmg, HitboxType type, float customLifeTime = -1f)
    {
        damage = dmg;
        if (customLifeTime > 0f)
            lifeTime = customLifeTime;

        // Destroy hitbox sau lifeTime
        Destroy(gameObject, lifeTime);

        // đảm bảo collider trigger
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    private void OnEnable()
    {
        hitPlayers.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || hitPlayers.Contains(other)) return;

        hitPlayers.Add(other);

        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps != null)
        {
            float finalDamage = ps.TakeDamage(damage);

            if (damageTextPrefab != null)
            {
                Vector3 pos = other.transform.position + Vector3.up;
                GameObject dmgText = Instantiate(damageTextPrefab, pos, Quaternion.identity);
                var ui = dmgText.GetComponent<FloatDamageUI>();
                if (ui != null)
                    ui.Setup(other.transform, finalDamage, Color.red, false, true);
            }
        }
    }

    public static GameObject SpawnHitbox(GameObject prefab, Transform enemy, Transform point,
                                         HitboxType type, float dmg,
                                         float lifeTime = 0.2f, Vector3? worldPos = null)
    {
        Vector3 pos = point != null ? point.position : (worldPos.HasValue ? worldPos.Value : enemy.position);
        Quaternion rot = point != null ? point.rotation : enemy.rotation;

        GameObject obj = Instantiate(prefab, pos, rot);
        obj.GetComponent<EnemyHitboxSkills>()?.Init(dmg, type, lifeTime);
        return obj;
    }

    private void OnDrawGizmos()
    {
        // Luôn dùng màu đỏ cho tất cả hitbox
        Gizmos.color = Color.red;

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

}
