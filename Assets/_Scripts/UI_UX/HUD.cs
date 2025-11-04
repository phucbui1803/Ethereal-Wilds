using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    [Header("Player Stats")]
    public PlayerStats player;

    [Header("UI Elements")]
    public Slider hpSlider;
    public Slider staminaSlider;
    public TMP_Text hpText;

    [Header("HP Text Colors")]
    public Color hpFullColor = Color.black;
    public Color hpLowColor = Color.white;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("HUD: PlayerStats not assigned!");
            enabled = false;
            return;
        }

        // Setup sliders max value
        hpSlider.maxValue = player.maxHP;
        hpSlider.value = player.currentHP;

        staminaSlider.maxValue = player.maxStamina;
        staminaSlider.value = player.currentStamina;

        UpdateHUD();
    }

    private void Update()
    {
        if (player == null) return;

        // Cập nhật giá trị slider theo PlayerStats
        hpSlider.maxValue = player.maxHP;
        hpSlider.value = player.currentHP;

        staminaSlider.maxValue = player.maxStamina;
        staminaSlider.value = player.currentStamina;

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (hpText != null)
        {
            hpText.text = $"{player.currentHP:F0}/{player.maxHP:F0}";
            float fillRatio = player.currentHP / player.maxHP;
            hpText.color = fillRatio > 0.5f ? hpFullColor : hpLowColor;
        }
    }
}
