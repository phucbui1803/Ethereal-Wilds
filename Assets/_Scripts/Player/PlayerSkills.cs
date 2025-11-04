using UnityEngine;
using System.Collections;

public class PlayerSkills : MonoBehaviour
{
    private Animator animator;
    private WeaponManager weaponManager;
    private PlayerStats playerStats;

    [Header("Skill Data (Assign in Inspector)")]
    public SkillData normalAttackData;
    public SkillData skillQData;
    public SkillData skillEData;
    public SkillData skillRData;

    [Header("Combo Settings")]
    [SerializeField] private int maxCombo = 3;
    private int comboStep = 0;
    private bool isAttacking = false;
    private bool queueNextAttack = false;
    private float lastClickTime;

    [Header("Attack Settings")]
    [SerializeField] private LayerMask enemyLayers;

    [Header("Skills Cooldown")]
    public float skillQCooldown;
    public float skillECooldown;
    public float skillRCooldown;

    [Header("Skills Damage Multipliers")]
    public float normalAttackDamageMultiplier;
    public float skillQAtkBuffPercent;
    public float skillQCritRateBuffPercent;
    public float skillEDamageMultiplier;
    public float skillRDamageMultiplier;

    private bool canUseQ = true;
    private bool canUseE = true;
    private bool canUseR = true;

    [Header("Time Buff Skill")]
    public float skillQTimeBuff;

    [Header("Skill R Energy")]
    public float skillRMaxEnergy;
    private float skillRCurrentEnergy;

    [Header("Energy Gain Values")]
    public float energyPerAttack;
    public float energyPerQ;
    public float energyPerE;

    [Header("Effects / Prefabs")]
    public GameObject skillQEffect;
    public GameObject skillEEffect;
    public GameObject skillREffect;

    [Header("UI Skill Icon")]
    public IconSkillUI iconAttack;
    public IconSkillUI iconSkillQ;
    public IconSkillUI iconSkillE;
    public IconSkillUI iconSkillR;

    [Header("Skill Q Flame")]
    public Transform skillQPoint;
    private GameObject fireInstance;

    [Header("Fireball Settings")]
    public Transform skillEPoint;
    public Transform skillRPoint;
    public float fireballRSpacing = 3f;

    [Header("Hitbox Settings")]
    public GameObject hitboxAttackPrefab;
    public GameObject hitboxEPrefab;
    public GameObject hitboxRPrefab;
    public Transform hitboxAttackPoint;
    public Transform hitboxRPoint;

    private bool qBuffActive = false;

    [Header("Menu References")]
    public PlayerMenuController menuController;

    private void Start()
    {
        animator = GetComponent<Animator>();
        weaponManager = GetComponent<WeaponManager>();
        playerStats = GetComponent<PlayerStats>();

        if (menuController == null)
        {
            menuController = FindFirstObjectByType<PlayerMenuController>();
            if (menuController == null)
                Debug.LogWarning("PlayerSkills: Không tìm thấy PlayerMenuController trong scene!");
        }

        // =======================
        // Gán giá trị từ SkillData
        if (normalAttackData != null)
        {
            //normalAttackDamageMultiplier = normalAttackData.skillMultiplier;
        }

        if (skillQData != null)
        {
            skillQCooldown = skillQData.cooldown;
            //skillQAtkBuffPercent = skillQData.skillMultiplier;
            //skillQCritRateBuffPercent = skillQData.skillMultiplier;
            skillQTimeBuff = skillQData.timeBuff;
            energyPerQ = skillQData.energyRegen;
        }

        if (skillEData != null)
        {
            skillECooldown = skillEData.cooldown;
            //skillEDamageMultiplier = skillEData.skillMultiplier;
            energyPerE = skillEData.energyRegen;
        }

        if (skillRData != null)
        {
            skillRCooldown = skillRData.cooldown;
            //skillRDamageMultiplier = skillRData.skillMultiplier;
            skillRMaxEnergy = skillRData.energyMax;  // R max energy

            skillRCurrentEnergy = skillRData.currentEnergy;

        }
        // =======================

        // Khởi tạo năng lượng R và UI
        iconSkillR?.SetEnergy(skillRCurrentEnergy, skillRMaxEnergy);

        if (skillQEffect != null && skillQPoint != null)
        {
            fireInstance = Instantiate(skillQEffect, skillQPoint.position, skillQPoint.rotation, skillQPoint);
            fireInstance.SetActive(false);
        }
    }


    private void Update()
    {
        // check player menu 
        if (PlayerMenuController.Instance != null && PlayerMenuController.Instance.IsMenuOpen)
            return;

        if (weaponManager == null || !weaponManager.controller.weaponDrawn)
            return;

        HandleNormalAttack();
        HandleSkillInput();
    }

    #region NORMAL COMBO
    private void HandleNormalAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastClickTime = Time.time;

            if (isAttacking &&
               (animator.GetCurrentAnimatorStateInfo(0).IsName("SkillE") ||
                animator.GetCurrentAnimatorStateInfo(0).IsName("SkillR")))
            {
                queueNextAttack = true;
                return;
            }

            if (isAttacking)
            {
                if (!queueNextAttack)
                {
                    queueNextAttack = true;
                }
            }
            else
            {
                comboStep = 1;
                animator.SetInteger("ComboStep", comboStep);
                animator.SetTrigger("Attack");
                PlayComboAttackSound(comboStep);
            }

            // Tăng năng lượng R khi đánh thường
            AddEnergyToR(energyPerAttack);
        }
    }

    public void OnAttackStart()
    {
        isAttacking = true;
    }

    public void OnAttackTransition()
    {
        if (queueNextAttack)
        {
            comboStep++;

            if (comboStep > maxCombo)
            {
                comboStep = 1;
            }

            animator.SetInteger("ComboStep", comboStep);
            animator.SetTrigger("Attack");
            PlayComboAttackSound(comboStep);
            queueNextAttack = false;
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;

        if (!queueNextAttack)
        {
            comboStep = 0;
            animator.SetInteger("ComboStep", comboStep);
        }
    }

    public void SpawnHitboxForAttack()
    {
        if (hitboxAttackPrefab == null || hitboxAttackPoint == null)
        {
            return;
        }

        var dmgInfo = playerStats.CalculateDamage();
        int playerLevel = playerStats.level;
        float skillDamage = dmgInfo.damage * normalAttackData.skillMultiplier;

        // Spawn hitbox Attack tại vị trí cố định trước mặt player
        HitboxSkills.SpawnHitbox(hitboxAttackPrefab, transform,  // parent
                                 hitboxAttackPoint, null,
                                 HitboxSkills.HitboxType.Attack,
                                 skillDamage, dmgInfo.isCrit, playerLevel,
                                 0.2f,                            // lifetime hitbox
                                 hitboxAttackPoint.position);     // vị trí spawn
    }

    private void PlayComboAttackSound(int step)
    {
        if (AudioManager.Instance == null) return;

        switch (step)
        {
            case 1: AudioManager.Instance.PlaySFX("Normal Attack 1"); break;
            case 2: AudioManager.Instance.PlaySFX("Normal Attack 2"); break;
            case 3: AudioManager.Instance.PlaySFX("Normal Attack 3"); break;
        }
    }
    #endregion

    #region SKILL INPUT
    private void HandleSkillInput()
    {
        if (Input.GetKeyDown(KeyCode.Q) && canUseQ)
        {
            UseSkill("Q");
            AddEnergyToR(energyPerQ); // Skill Q hồi R
        }

        if (Input.GetKeyDown(KeyCode.E) && canUseE)
        {
            UseSkill("E");
            AddEnergyToR(energyPerE); // Skill E hồi R
        }

        if (Input.GetKeyDown(KeyCode.R) && canUseR && skillRCurrentEnergy >= skillRMaxEnergy)
        {
            skillRCurrentEnergy = 0;
            iconSkillR.SetEnergy(skillRCurrentEnergy, skillRMaxEnergy);

            UseSkill("R");
        }
    }

    private void UseSkill(string skill)
    {
        animator.SetTrigger("Skill" + skill);

        switch (skill)
        {
            case "Q":
                canUseQ = false;
                SkillQ();
                iconSkillQ?.StartCooldown(skillQCooldown);
                Invoke(nameof(ResetQ), skillQCooldown);
                break;
            case "E":
                canUseE = false;
                SkillE();
                iconSkillE?.StartCooldown(skillECooldown);
                Invoke(nameof(ResetE), skillECooldown);
                break;
            case "R":
                canUseR = false;
                SkillR();
                iconSkillR?.StartCooldown(skillRCooldown);
                Invoke(nameof(ResetR), skillRCooldown);
                break;
        }
    }

    private void AddEnergyToR(float amount)
    {
        skillRCurrentEnergy += amount;
        if (skillRCurrentEnergy > skillRMaxEnergy)
            skillRCurrentEnergy = skillRMaxEnergy;

        iconSkillR?.SetEnergy(skillRCurrentEnergy, skillRMaxEnergy);
    }
    #endregion

    #region SKILL Q
    private void SkillQ()
    {
        if (fireInstance != null)
        {
            fireInstance.SetActive(true);
        }

        PlaySkillQSound();

        if (!qBuffActive)
        {
            qBuffActive = true;
            playerStats.ApplyAttackBuff(skillQData.skillMultiplier);
            playerStats.ApplyCritRateBuff(skillQData.skillMultiplier);
            Invoke(nameof(RemoveQBuff), skillQTimeBuff);
        }
    }

    private void RemoveQBuff()
    {
        qBuffActive = false;
        playerStats.RemoveAttackBuff(skillQData.skillMultiplier);
        playerStats.RemoveCritRateBuff(skillQData.skillMultiplier);

        if (fireInstance != null)
        {
            fireInstance.SetActive(false);
        }
    }

    public void DisableFlameFromWeaponManager()
    {
        DisableFlame();
        CancelInvoke(nameof(DisableFlame));
    } 
    private void DisableFlame()
    {
        if (fireInstance != null)
        {
            fireInstance.SetActive(false);
        }
    }

    public void OnSkillStart() 
    { 
        isAttacking = true;

        if (weaponManager != null && weaponManager.controller != null)
        { 
            var controllerMB = weaponManager.controller as MonoBehaviour;

            if (controllerMB != null)
            { 
                controllerMB.enabled = false;
            } 
        } 
    } 

    public void OnSkillEnd() 
    { 
        if (weaponManager != null && weaponManager.controller != null) 
        { 
            var controllerMB = weaponManager.controller as MonoBehaviour; 

            if (controllerMB != null) 
            { 
                controllerMB.enabled = true;
            } 
        } 

        isAttacking = false; 

        if (queueNextAttack) 
        { 
            comboStep = 1; 
            animator.SetInteger("ComboStep", comboStep); 
            animator.SetTrigger("Attack"); 
            queueNextAttack = false; 
        } 
        else 
        { 
            comboStep = 0; 
            animator.SetInteger("ComboStep", comboStep); 
        } 
    }

    private void PlaySkillQSound()
    {
        if (AudioManager.Instance == null) 
            return;

        AudioManager.Instance.PlaySFX("Fire Buff");
    }
    #endregion

    #region CANCEL SKILL Q
    public void CancelSkillQ()
    {
        if (!qBuffActive)
            return; // Nếu buff chưa active thì thôi

        // Hủy timer RemoveQBuff nếu đang chạy
        CancelInvoke(nameof(RemoveQBuff));

        playerStats.RemoveAttackBuff(skillQData.skillMultiplier);
        playerStats.RemoveCritRateBuff(skillQData.skillMultiplier);

        // Tắt hiệu ứng Q
        if (fireInstance != null)
        {
            fireInstance.SetActive(false);
        }

        qBuffActive = false;

        Debug.Log("Skill Q đã bị hủy!");
    }
    #endregion


    #region SKILL E
    private void SkillE()
    {
        animator.SetBool("Grounded", true);
        animator.SetBool("IsWeaponInHand", true);
    }

    public void SpawnFireballForSkillE()
    {
        if (skillEEffect == null || skillEPoint == null)
        {
            return;
        }

        GameObject fireball = Instantiate(skillEEffect, skillEPoint.position, skillEPoint.rotation);
        Fireball fb = fireball.GetComponent<Fireball>();

        if (fb != null)
        {
            fb.mode = Fireball.FireballMode.Forward;
        }
    }

    public void SpawnHitboxForSkillE(Vector3 spawnPosition)
    {
        if (hitboxEPrefab == null || skillEPoint == null)
        {
            return;
        }

        var dmgInfo = playerStats.CalculateDamage();
        int playerLevel = playerStats.level;
        float skillDamage = dmgInfo.damage * skillEData.skillMultiplier;

        // Spawn hitbox E tại vị trí fireball va chạm (truyền từ Fireball.cs)
        HitboxSkills.SpawnHitbox(hitboxEPrefab, transform,    // parent transform nếu muốn
                                 null, null,
                                 HitboxSkills.HitboxType.E,
                                 skillDamage, dmgInfo.isCrit, playerLevel,
                                 0.2f,                       // lifetime
                                 spawnPosition);       // vị trí spawn hitbox khi va chạm
    }
    #endregion

    #region SKILL R
    private void SkillR()
    {
        animator.SetBool("Grounded", true);
        animator.SetBool("IsWeaponInHand", true);
    }

    public void SpawnFireballForSkillR(int index)
    {
        if (skillREffect == null || skillRPoint == null)
        {
            return;
        }

        Vector3 forward = skillRPoint.forward;
        Vector3 spawnPos = skillRPoint.position + forward * (index * fireballRSpacing);

        GameObject fireball = Instantiate(skillREffect, spawnPos, Quaternion.LookRotation(Vector3.down));
        Fireball fb = fireball.GetComponent<Fireball>();

        if (fb != null)
        {
            fb.mode = Fireball.FireballMode.Downward;
        }
    }

    public void SpawnHitboxForSkillR()
    {
        if (hitboxRPrefab == null || skillRPoint == null)
        {
            return;
        }

        var dmgInfo = playerStats.CalculateDamage();
        int playerLevel = playerStats.level;
        float skillDamage = dmgInfo.damage * skillRData.skillMultiplier;

        // Spawn tại hitboxRPoint cố định trước mặt player
        HitboxSkills.SpawnHitbox(hitboxRPrefab, transform,
                                 null, hitboxRPoint,
                                 HitboxSkills.HitboxType.R,
                                 skillDamage, dmgInfo.isCrit, playerLevel,
                                 0.2f);
    }
    #endregion

    private void ResetQ() => canUseQ = true;
    private void ResetE() => canUseE = true;
    private void ResetR() => canUseR = true;
}
