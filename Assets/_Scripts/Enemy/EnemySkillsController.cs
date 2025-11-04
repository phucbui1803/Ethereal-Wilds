using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyStats))]
[RequireComponent(typeof(EnemyController))]
public class EnemySkillController : MonoBehaviour
{
    [Header("Skill Settings")]
    public List<EnemySkills> skills = new List<EnemySkills>();
    public float attackDistance = 5f;

    [Header("References")]
    public Transform player;
    private EnemyController enemyController;
    private EnemyStats enemyStats;
    private Animator animator;

    [Header("Hitbox Prefabs")]
    public GameObject SwipingHitboxPrefab;
    public GameObject roaringHitboxPrefab;
    public GameObject jumpAttackHitboxPrefab;

    [Header("Hitbox Points")]
    public Transform SwipingPoint;
    public Transform roaringPoint;
    public Transform jumpAttackPoint;

    private Coroutine attackRoutine;

    void Start()
    {
        animator = GetComponent<Animator>();
        enemyStats = GetComponent<EnemyStats>();
        enemyController = GetComponent<EnemyController>();

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (player == null || !enemyStats.IsAlive())
        {
            StopAttackRoutine();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackDistance)
        {
            enemyController._isAttacking = true;
            if (attackRoutine == null)
                attackRoutine = StartCoroutine(AttackLoop());
        }
        else
        {
            enemyController._isAttacking = false;
            StopAttackRoutine();
        }
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }
    }

    private IEnumerator AttackLoop()
    {
        while (enemyStats.IsAlive())
        {
            EnemySkills skill = GetRandomAvailableSkill();

            if (skill != null)
            {
                yield return StartCoroutine(PerformAttack(skill));
            }
            else
            {
                yield return new WaitForSeconds(0.2f); // chờ khi không có skill
            }
        }
    }

    private EnemySkills GetRandomAvailableSkill()
    {
        List<EnemySkills> available = new List<EnemySkills>();
        foreach (var s in skills)
        {
            if (s.CanUse()) available.Add(s);
        }

        if (available.Count == 0) return null;
        return available[Random.Range(0, available.Count)];
    }

    private IEnumerator PerformAttack(EnemySkills skill)
    {
        enemyController._isUsingSkill = true;

        // ✳️ Tạm khóa di chuyển
        enemyController.canMove = false;

        skill.Use(); // ghi nhận thời gian dùng skill

        // Xoay về player
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(dir);

        // Trigger animation
        if (!string.IsNullOrEmpty(skill.animatorTrigger))
            animator.SetTrigger(skill.animatorTrigger);

        // Phát âm thanh khi bắt đầu skill
        if (!string.IsNullOrEmpty(skill.soundStart) && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(skill.soundStart, transform.position);

        float animLength = GetAnimationLength(skill.animatorTrigger);
        yield return new WaitForSeconds(animLength);

        // ✅ Reset trạng thái sau khi skill xong
        enemyController._isUsingSkill = false;
        enemyController._isAttacking = false;
        enemyController.canMove = true;

        // ✅ Cho phép AI trở lại patrol hoặc chase nếu cần
        if (Vector3.Distance(transform.position, player.position) <= enemyController.ChaseDistance)
        {
            enemyController.IsChasing = true;
        }
        else
        {
            enemyController.ChooseNewState(false);
        }


        yield return new WaitForSeconds(0.2f); // delay nhỏ trước skill tiếp theo
    }

    // ========================= Effects =========================
    public void SpawnEffectEvent(string skillName)
    {
        EnemySkills skill = skills.Find(s => s.skillName == skillName);
        if (skill == null || skill.effectPrefab == null) return;

        Transform spawnPoint = skill.spawnPoint != null ? skill.spawnPoint : transform;
        GameObject effect = Instantiate(skill.effectPrefab, spawnPoint.position, spawnPoint.rotation);

        if (skill.effectFollowEnemy)
            effect.transform.SetParent(spawnPoint);

        // Phát âm thanh impact tại vị trí spawn effect
        if (!string.IsNullOrEmpty(skill.soundImpact) && AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(skill.soundImpact, spawnPoint.position);

        Destroy(effect, 3f);
    }

    // ========================= Hitboxes =========================
    public void SpawnSwipingHitbox()
    {
        if (SwipingHitboxPrefab == null || SwipingPoint == null) return;
        float damage = enemyStats.CalculateDamage() * GetSkillMultiplier("Swiping");

        EnemyHitboxSkills.SpawnHitbox(SwipingHitboxPrefab, transform, SwipingPoint,
                                      EnemyHitboxSkills.HitboxType.Melee, damage, 0.2f);
    }

    public void SpawnRoaringHitbox()
    {
        if (roaringHitboxPrefab == null || roaringPoint == null) return;
        float damage = enemyStats.CalculateDamage() * GetSkillMultiplier("Roaring");
        float duration = GetAnimationLength("Roaring");

        StartCoroutine(SpawnContinuousHitbox(roaringHitboxPrefab, roaringPoint.position, damage, duration));
    }

    public void SpawnJumpAttackHitbox()
    {
        if (jumpAttackHitboxPrefab == null || jumpAttackPoint == null) return;
        float damage = enemyStats.CalculateDamage() * GetSkillMultiplier("JumpAttack");

        EnemyHitboxSkills.SpawnHitbox(jumpAttackHitboxPrefab, transform, jumpAttackPoint,
                                      EnemyHitboxSkills.HitboxType.AOE, damage, 0.2f);
    }

    private float GetSkillMultiplier(string skillName)
    {
        EnemySkills skill = skills.Find(s => s.skillName == skillName);
        return skill != null ? skill.damageMultiplier : 1f;
    }

    private float GetAnimationLength(string trigger)
    {
        if (animator == null || string.IsNullOrEmpty(trigger)) return 1f;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == trigger)
                return clip.length;
        }

        Debug.LogWarning($"Animation clip '{trigger}' not found!");
        return 1f;
    }

    private IEnumerator SpawnContinuousHitbox(GameObject prefab, Vector3 pos, float damage, float duration)
    {
        float timer = 0f;
        float interval = 2f;

        while (timer < duration)
        {
            EnemyHitboxSkills.SpawnHitbox(prefab, transform, null,
                                          EnemyHitboxSkills.HitboxType.AOE, damage, interval, pos);
            yield return new WaitForSeconds(interval);
            timer += interval;
        }
    }
}
