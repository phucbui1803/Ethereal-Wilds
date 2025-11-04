using System.Collections.Generic;
using UnityEngine;

public enum EnemySkillType
{
    Melee,
    Ranged,
    AOE
}

[System.Serializable]
public class EnemySkills
{
    public string skillName;
    public EnemySkillType skillType;
    public float damageMultiplier = 1f;

    [Header("Animation & Effect")]
    public string animatorTrigger;          // Trigger animation
    public GameObject effectPrefab;         // Prefab effect
    public Transform spawnPoint;            // Nơi spawn effect
    public bool effectFollowEnemy = true;   // true: gắn parent theo enemy

    [Header("Timing")]
    public float damageDelay = 0.3f;       // Delay trước khi gây damage
    public float cooldown = 3f;

    [Header("Sound")]
    public string soundStart;   // âm thanh lúc bắt đầu skill
    public string soundImpact;  // âm thanh khi gây damage (spawn effect)   


    [HideInInspector] public float lastUsedTime = -Mathf.Infinity;

    public bool CanUse() => Time.time >= lastUsedTime + cooldown;
    public void Use() => lastUsedTime = Time.time;
}
