using System.Collections;
using StarterAssets;
using UnityEngine;

public class AutoDrawSheath : MonoBehaviour
{
    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.T; // Phím để rút / tra vũ khí
    public float cooldownTime = 1f;

    private Animator animator;
    private ThirdPersonController controller;
    private PlayerSkills playerSkills;
    private bool isOnCooldown = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<ThirdPersonController>();
        playerSkills = GetComponent<PlayerSkills>();

        if (playerSkills == null)
            Debug.LogWarning("AutoDrawSheath: Không tìm thấy PlayerSkills!");
    }

    private void Update()
    {
        // Nếu không có vũ khí thì bỏ qua
        if (PlayerInventory.Instance == null || PlayerInventory.Instance.GetCurrentWeaponObject() == null)
            return;

        // Nhấn phím T để toggle rút / tra kiếm
        if (Input.GetKeyDown(toggleKey))
        {
            if (isOnCooldown) return;

            ToggleWeapon();
            StartCoroutine(CooldownRoutine(cooldownTime));
        }
    }

    private void ToggleWeapon()
    {
        if (!controller.weaponDrawn)
        {
            // Rút kiếm
            animator.SetTrigger("DrawWeapon");
            controller.weaponDrawn = true;
        }
        else
        {
            // Tra kiếm
            animator.SetTrigger("SheathWeapon");
            controller.weaponDrawn = false;

            // Hủy buff Skill Q nếu đang kích hoạt
            playerSkills?.CancelSkillQ();
        }

        // Cập nhật HUD icon
        if (IconDrawSheathUI.Instance != null)
        {
            IconDrawSheathUI.Instance.SetWeaponDrawn(controller.weaponDrawn);
            IconDrawSheathUI.Instance.StartCooldown(cooldownTime);
        }
    }

    private IEnumerator CooldownRoutine(float time)
    {
        isOnCooldown = true;
        yield return new WaitForSeconds(time);
        isOnCooldown = false;
    }
}
