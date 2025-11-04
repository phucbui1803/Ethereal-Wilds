using UnityEngine;
using UnityEngine.UI;
using StarterAssets;


public class SettingMenuManager : MonoBehaviour
{
    [Header("References")]
    public GameObject settingPanel;       // Gán panel Setting ở đây
    public Button closeButton;            // Nút "Accept" hoặc "Close"

    [Header("HUD Buttons")]
    public Button settingIconButton;   // Nút Setting trên HUD

    public static SettingMenuManager Instance { get; private set; }

    public bool IsMenuOpen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Gắn sự kiện click cho icon Setting (HUD)
        if (settingIconButton != null)
            settingIconButton.onClick.AddListener(() =>
            {
                OpenSettingFromHUD();
                PlayButtonClickSound();
            });

        // Gán sự kiện cho nút Close
        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                CloseSettingMenu();
                PlayButtonClickSound();
            });

        // Đảm bảo panel bị ẩn khi bắt đầu
        if (settingPanel != null)
            settingPanel.SetActive(false);
    }

    private void Update()
    {
        // Nếu đang mở dialogue thì không cho toggle menu
        if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
            return;

        // Nhấn ESC để mở / đóng menu Setting
        if (Input.GetKeyDown(KeyCode.Escape))
            ToggleSettingMenu();
    }

    public void OpenSettingFromHUD()
    {
        // Nếu đang mở dialogue thì không cho toggle menu
        if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
            return;

        if (!IsMenuOpen)
        {
            IsMenuOpen = true;
            settingPanel.SetActive(true);

            StarterAssets.StarterAssetsInputs.ForceUnlockCursor = true;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ToggleSettingMenu()
    {
        IsMenuOpen = !IsMenuOpen;

        // Nếu mở Setting mà PlayerMenu đang mở thì tắt nó
        if (IsMenuOpen && PlayerMenuController.Instance != null && PlayerMenuController.Instance.IsMenuOpen)
            PlayerMenuController.Instance.ClosePlayerMenu();

        // Nếu mở Setting mà Inventory đang mở thì tắt nó
        if (IsMenuOpen && InventoryMenuController.Instance != null && InventoryMenuController.Instance.IsMenuOpen)
            InventoryMenuController.Instance.CloseInventoryMenu();

        // Nếu mở Setting mà World Map đang mở thì tắt nó
        if (IsMenuOpen && WorldMapManager.Instance != null && WorldMapManager.Instance.IsMapOpen)
            WorldMapManager.Instance.CloseWorldMap();

        settingPanel.SetActive(IsMenuOpen);

        StarterAssets.StarterAssetsInputs.ForceUnlockCursor = IsMenuOpen;

        // Dừng game và hiển thị chuột khi mở
        Time.timeScale = IsMenuOpen ? 0f : 1f;
        Cursor.lockState = IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsMenuOpen;
    }

    public void CloseSettingMenu()
    {
        if (!IsMenuOpen) return;

        IsMenuOpen = false;
        settingPanel.SetActive(false);

        StarterAssets.StarterAssetsInputs.ForceUnlockCursor = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Click");
    }
}
