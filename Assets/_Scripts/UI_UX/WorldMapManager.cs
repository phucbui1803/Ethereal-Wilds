using StarterAssets;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapManager : MonoBehaviour
{
    [Header("References")]
    public GameObject worldMapPanel;
    public Button closeButton;

    [Header("HUD Buttons")]
    public Button worldMapIconButton;

    public static WorldMapManager Instance { get; private set; }
    public bool IsMapOpen { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Gắn sự kiện click cho icon Setting (HUD)
        if (worldMapIconButton != null)
            worldMapIconButton.onClick.AddListener(() =>
            {
                OpenWorldMapFromHUD();
                PlayClickSound();
            });

        if (closeButton != null)
            closeButton.onClick.AddListener(() =>
            {
                CloseWorldMap();
                PlayClickSound();
            });

        if (worldMapPanel != null)
            worldMapPanel.SetActive(false);
    }

    private void Update()
    {
        // Nếu đang mở dialogue thì không cho toggle menu
        if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
            return;

        if (Input.GetKeyDown(KeyCode.M))
            ToggleWorldMap();
    }

    public void OpenWorldMapFromHUD()
    {
        // Nếu đang mở dialogue thì không cho toggle menu
        if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
            return;

        if (!IsMapOpen)
        {
            IsMapOpen = true;
            worldMapPanel.SetActive(true);

            StarterAssets.StarterAssetsInputs.ForceUnlockCursor = true;

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ToggleWorldMap()
    {
        IsMapOpen = !IsMapOpen;

        if (IsMapOpen)
        {
            if (SettingMenuManager.Instance?.IsMenuOpen == true)
                SettingMenuManager.Instance.CloseSettingMenu();

            if (PlayerMenuController.Instance?.IsMenuOpen == true)
                PlayerMenuController.Instance.ClosePlayerMenu();
            
            if (InventoryMenuController.Instance?.IsMenuOpen == true)
                InventoryMenuController.Instance.CloseInventoryMenu();

            // Khi mở map, chỉ đồng bộ hiển thị, không ghi thời gian ra world
            WorldMapController.Instance?.SyncTimeFromWorld();
        }

        worldMapPanel.SetActive(IsMapOpen);

        StarterAssets.StarterAssetsInputs.ForceUnlockCursor = IsMapOpen;

        Time.timeScale = IsMapOpen ? 0f : 1f;
        Cursor.lockState = IsMapOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = IsMapOpen;
    }

    public void CloseWorldMap()
    {
        if (!IsMapOpen) return;

        IsMapOpen = false;
        worldMapPanel.SetActive(false);

        StarterAssets.StarterAssetsInputs.ForceUnlockCursor = false;

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void PlayClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Click");
    }
}
