using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterPanelUI : MonoBehaviour
{
    [Header("Info Panel References")]
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text xpText;
    public TMP_Text hpText;
    public TMP_Text atkText;
    public TMP_Text defText;
    public TMP_Text critRateText;
    public TMP_Text critDamageText;

    [Header("Extra Stats Panel")]
    public GameObject extraStatsPanel; // panel ẩn/hiện khi nhấn More
    public TMP_Text hpRegenText;
    public TMP_Text staminaText;
    public TMP_Text staminaRegenText;

    [Header("Buttons")]
    public Button moreButton;
    public TMP_Text moreButtonLabel; // để đổi text More ↔ Hide

    private bool extraVisible = false;
    private PlayerStats playerStats;

    public static CharacterPanelUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Đảm bảo panel ẩn ngay khi khởi tạo
        if (extraStatsPanel != null)
            extraStatsPanel.SetActive(false);

        if (moreButton != null)
            moreButton.onClick.AddListener(ToggleExtraStats);
    }

    private void OnEnable()
    {
        playerStats = PlayerStats.Instance;
        if (playerStats != null)
            ShowPlayerStats(playerStats);
    }

    public void ShowPlayerStats(PlayerStats stats)
    {
        if (stats == null) return;

        // Phần 1: Chỉ số cơ bản
        nameText.text = $"{stats.characterName}";
        levelText.text = $"{stats.level}";
        xpText.text = $"{stats.currentXP:F0}/{stats.xpToNextLevel:F0}";
        hpText.text = $"{stats.maxHP:F0}";
        atkText.text = $"{stats.atk:F0}";
        defText.text = $"{stats.def:F0}";
        critRateText.text = $"{stats.critRate * 100f:F1}%";
        critDamageText.text = $"{stats.critDamage * 100f:F1}%";

        // Phần 2: Chỉ số mở rộng
        hpRegenText.text = $"{stats.hpRegen:F0}";
        staminaText.text = $"{stats.maxStamina:F0}";
        staminaRegenText.text = $"{stats.staminaRegen:F0}";

        // Đảm bảo trạng thái hiển thị khớp với biến extraVisible
        if (extraStatsPanel != null)
            extraStatsPanel.SetActive(extraVisible);

        UpdateMoreButtonLabel();
    }

    private void ToggleExtraStats()
    {
        extraVisible = !extraVisible;
        extraStatsPanel.SetActive(extraVisible);
        UpdateMoreButtonLabel();

        // Phát âm thanh click
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Click");
    }

    private void UpdateMoreButtonLabel()
    {
        if (moreButtonLabel != null)
            moreButtonLabel.text = extraVisible ? "Hide" : "More";
    }
}
