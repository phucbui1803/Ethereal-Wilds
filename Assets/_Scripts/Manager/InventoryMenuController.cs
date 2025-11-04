using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryMenuPanel;
    public Button closeButton;

    [Header("HUD Buttons")]
    public Button inventoryIconButton;

    [Header("Sub Panels")]
    public GameObject itemPanel;
    public GameObject potionPanel;

    [Header("Tab Buttons")]
    public Button itemButton;
    public Button potionButton;

    [Header("Glow Overlays")]
    public CanvasGroup itemGlow;
    public CanvasGroup potionGlow;

    public static InventoryMenuController Instance { get; private set; }

    public bool IsMenuOpen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // --- HUD button ---
        if (inventoryIconButton != null)
            inventoryIconButton.onClick.AddListener(() =>
            {
                OpenInventoryFromHUD();
                PlayButtonClickSound();
            });

        // Gắn sự kiện click cho các tab button
        if (itemButton != null)
            itemButton.onClick.AddListener(() =>
            {
                OnTabButtonClicked(MenuType.Item);
                PlayButtonClickSound();
            });

        if (potionButton != null)
            potionButton.onClick.AddListener(() =>
            {
                OnTabButtonClicked(MenuType.Potion);
                PlayButtonClickSound();
            });

        // Gắn sự kiện cho nút Close
        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                CloseInventoryMenu();
                PlayButtonClickSound();
            });

        // Tắt glow ban đầu
        SetGlow(itemGlow, false);
        SetGlow(potionGlow, false);
    }

    private void Update()
    {
        // Nếu đang mở dialogue thì không cho toggle menu
        if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
            return;

        if (Input.GetKeyDown(KeyCode.B))
            ToggleInventoryMenu();
    }

    public void OpenInventoryFromHUD()
    {
        // Nếu đang mở dialogue thì không cho toggle menu
        if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
            return;

        if (!IsMenuOpen)
        {
            IsMenuOpen = true;
            inventoryMenuPanel.SetActive(true);

            StarterAssets.StarterAssetsInputs.ForceUnlockCursor = true;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        ShowItemPanel();
        HighlightGlow(MenuType.Item);
        InventoryUI.Instance?.UpdateUI();
    }

    public void ToggleInventoryMenu()
    {
        IsMenuOpen = !IsMenuOpen;

        // Đảm bảo tắt Player Menu trước khi mở Inventory
        if (IsMenuOpen && PlayerMenuController.Instance != null && PlayerMenuController.Instance.IsMenuOpen)
        {
            PlayerMenuController.Instance.ClosePlayerMenu();
        }

        // Tắt Setting Menu
        if (IsMenuOpen && SettingMenuManager.Instance != null && SettingMenuManager.Instance.IsMenuOpen)
        {
            SettingMenuManager.Instance.CloseSettingMenu();
        }

        // Tắt World Map trước khi mở
        if (IsMenuOpen && WorldMapManager.Instance != null && WorldMapManager.Instance.IsMapOpen)
        {
            WorldMapManager.Instance.CloseWorldMap();
        }

        inventoryMenuPanel.SetActive(IsMenuOpen);

        if (IsMenuOpen)
        {
            ShowItemPanel();
            HighlightGlow(MenuType.Item);
            // Cập nhật UI Inventory khi mở
            InventoryUI.Instance?.UpdateUI();
        }

        StarterAssets.StarterAssetsInputs.ForceUnlockCursor = IsMenuOpen;

        Time.timeScale = IsMenuOpen ? 0f : 1f;
        Cursor.lockState = IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsMenuOpen;
    }

    private void OnTabButtonClicked(MenuType type)
    {
        switch (type)
        {
            case MenuType.Item:
                ShowItemPanel();
                break;
            case MenuType.Potion:
                ShowPotionPanel();
                break;
        }
        HighlightGlow(type);
    }

    private void ShowItemPanel()
    {
        if (itemPanel == null)
        {
            Debug.LogError("itemPanel đã bị destroy hoặc chưa gán!");
            return;
        }

        itemPanel?.SetActive(true);
        potionPanel?.SetActive(false);
    }

    private void ShowPotionPanel()
    {
        itemPanel?.SetActive(false);
        potionPanel?.SetActive(true);
    }

    private void HighlightGlow(MenuType type)
    {
        SetGlow(itemGlow, type == MenuType.Item);
        SetGlow(potionGlow, type == MenuType.Potion);
    }

    private void SetGlow(CanvasGroup glow, bool active)
    {
        if (glow == null) return;
        glow.alpha = active ? 1f : 0f;
        glow.interactable = false;
        glow.blocksRaycasts = false;
    }

    private enum MenuType { Item, Potion, Craft }

    public void CloseInventoryMenu()
    {
        if (!IsMenuOpen) return;

        IsMenuOpen = false;
        inventoryMenuPanel.SetActive(false);

        StarterAssets.StarterAssetsInputs.ForceUnlockCursor = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("Click");
        }
    }
}
