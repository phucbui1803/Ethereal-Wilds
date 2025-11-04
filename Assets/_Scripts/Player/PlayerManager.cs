using System.Collections;
using UnityEngine;
using TMPro;
using StarterAssets;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [Header("UI Defeat Panel")]
    public GameObject defeatPanel;
    public TMP_Text countdownText;
    public Button respawnButton;

    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public float respawnDelay = 10f;

    private PlayerStats playerStats;
    private Animator animator;
    private ThirdPersonController controller;
    private StarterAssetsInputs input;
    private WeaponManager weaponManager;
    private Coroutine respawnCoroutine;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
        controller = GetComponent<ThirdPersonController>();
        input = GetComponent<StarterAssetsInputs>();
        weaponManager = GetComponent<WeaponManager>();

        if (defeatPanel != null)
            defeatPanel.SetActive(false);

        if (respawnButton != null)
            respawnButton.onClick.AddListener(RespawnImmediately);
    }

    public void OnDeathAnimationFinished()
    {
        if (playerStats.IsDead)
            respawnCoroutine = StartCoroutine(ShowDefeatPanelAndRespawn());
    }

    private IEnumerator ShowDefeatPanelAndRespawn()
    {
        // Hiện panel
        defeatPanel.SetActive(true);

        // Force unlock cursor để UI nhận input
        StarterAssetsInputs.ForceUnlockCursor = true;

        // Tạm khóa di chuyển player
        if (controller != null) controller.enabled = false;

        float timeLeft = respawnDelay;
        while (timeLeft > 0)
        {
            // Vì Time.timeScale = 0, ta dùng unscaledDeltaTime
            countdownText.text = $"{Mathf.Ceil(timeLeft)}s";
            yield return new WaitForSecondsRealtime(1f);
            timeLeft--;
        }

        RespawnPlayer();
    }

    private void RespawnImmediately()
    {
        // Nếu đang chạy countdown thì dừng
        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }

        RespawnPlayer();
    }

    private void RespawnPlayer()
    {
        // Ẩn panel defeat
        defeatPanel.SetActive(false);
        countdownText.text = "";

        // Unlock cursor lại cho gameplay
        StarterAssetsInputs.ForceUnlockCursor = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Khôi phục HP/Stamina
        playerStats.currentHP = playerStats.maxHP;
        playerStats.currentStamina = playerStats.maxStamina;

        // Reset isDead
        typeof(PlayerStats).GetField("isDead",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .SetValue(playerStats, false);

        // Teleport về làng
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        // Reset weapon về sheath ngay lập tức
        var weaponManager = GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.SheathWeapon();
        }

        // Reset animator về Idle
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (controller != null) controller.enabled = true;

        Debug.Log("Player đã hồi sinh!");
    }
}
