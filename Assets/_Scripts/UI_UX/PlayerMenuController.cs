    using StarterAssets;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    public class PlayerMenuController : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject playerMenuPanel;
        public Button closeButton;

        [Header("HUD Buttons")]
        public Button characterIconButton;

        [Header("Sub Panels")]
        public GameObject characterPanel;
        public GameObject weaponPanel;
        public GameObject skillPanel;

        [Header("Tab Buttons")]
        public Button characterButton;
        public Button weaponButton;
        public Button skillButton;

        [Header("Glow Overlays")]
        public CanvasGroup characterGlow;
        public CanvasGroup weaponGlow;
        public CanvasGroup skillGlow;

        public static PlayerMenuController Instance { get; private set; }

        public bool IsMenuOpen { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            // Gắn sự kiện click cho icon button Character
            if (characterIconButton != null)
                characterIconButton.onClick.AddListener(() =>
                {
                    OpenCharacterPanelFromHUD();
                    PlayButtonClickSound();
                });

            // Gắn sự kiện click cho các button
            if (characterButton != null)
                characterButton.onClick.AddListener(() =>
                {
                    OnTabButtonClicked(MenuType.Character);
                    PlayButtonClickSound();
                });

            if (weaponButton != null)
                weaponButton.onClick.AddListener(() =>
                {
                    OnTabButtonClicked(MenuType.Weapon);
                    PlayButtonClickSound();
                });

            if (skillButton != null)
                skillButton.onClick.AddListener(() =>
                {
                    OnTabButtonClicked(MenuType.Skill);
                    PlayButtonClickSound();
                });

            // Gắn sự kiện cho nút Close
            if (closeButton != null)
                closeButton.onClick.AddListener(() =>
                {
                    ClosePlayerMenu();
                    PlayButtonClickSound();
                });

            // Đảm bảo glow ban đầu tắt
            SetGlow(characterGlow, false);
            SetGlow(weaponGlow, false);
            SetGlow(skillGlow, false);
        }

        private void Update()
        {
            // Nếu đang mở dialogue thì không cho toggle menu
            if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
                return;

            if (Input.GetKeyDown(KeyCode.C))
                TogglePlayerMenu();
        }

        public void OpenCharacterPanelFromHUD()
        {
            // Kiểm tra nếu đang dialogue thì không mở menu
            if (StarterAssetsInputs.ForceUnlockCursor && StarterAssetsInputs.Instance != null && StarterAssetsInputs.Instance.dialogueActive)
                return;

            if (!IsMenuOpen)
            {
                IsMenuOpen = true;
                playerMenuPanel.SetActive(true);

                StarterAssets.StarterAssetsInputs.ForceUnlockCursor = true;

                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            ShowCharacterPanel();
            HighlightGlow(MenuType.Character);
        }

        public void TogglePlayerMenu()
        {
            IsMenuOpen = !IsMenuOpen;

            // Tắt Setting trước khi mở
            if (IsMenuOpen && SettingMenuManager.Instance != null && SettingMenuManager.Instance.IsMenuOpen)
            {
                SettingMenuManager.Instance.CloseSettingMenu();
            }

            // Đảm bảo tắt Inventory trước khi mở Player Menu
            if (IsMenuOpen && InventoryMenuController.Instance != null && InventoryMenuController.Instance.IsMenuOpen)
            {
                InventoryMenuController.Instance.CloseInventoryMenu();
            }

            // Tắt World Map trước khi mở
            if (IsMenuOpen && WorldMapManager.Instance != null && WorldMapManager.Instance.IsMapOpen)
            {
                WorldMapManager.Instance.CloseWorldMap();
            }

            playerMenuPanel.SetActive(IsMenuOpen);

            if (IsMenuOpen)
            {
                ShowCharacterPanel();
                HighlightGlow(MenuType.Character);
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
                case MenuType.Character:
                    ShowCharacterPanel();
                    break;
                case MenuType.Weapon:
                    ShowWeaponPanel();
                    break;
                case MenuType.Skill:
                    ShowSkillPanel();
                    break;
            }
            HighlightGlow(type);
        }

        private void ShowCharacterPanel()
        {
            characterPanel?.SetActive(true);
            weaponPanel?.SetActive(false);
            skillPanel?.SetActive(false);
        }

        private void ShowWeaponPanel()
        {
            characterPanel?.SetActive(false);
            weaponPanel?.SetActive(true);
            skillPanel?.SetActive(false);

            // Gọi generate preview ở đây
            //WeaponPanelUI.Instance?.RefreshPanel();
        }

        private void ShowSkillPanel()
        {
            characterPanel?.SetActive(false);
            weaponPanel?.SetActive(false);
            skillPanel?.SetActive(true);
        }

        private void HighlightGlow(MenuType type)
        {
            SetGlow(characterGlow, type == MenuType.Character);
            SetGlow(weaponGlow, type == MenuType.Weapon);
            SetGlow(skillGlow, type == MenuType.Skill);
        }

        private void SetGlow(CanvasGroup glow, bool active)
        {
            if (glow == null) return;
            glow.alpha = active ? 1f : 0f;
            glow.interactable = false;
            glow.blocksRaycasts = false; // không chặn click
        }

        private enum MenuType { Character, Weapon, Skill }

        public void ClosePlayerMenu()
        {
            if (!IsMenuOpen) return;

            IsMenuOpen = false;
            playerMenuPanel.SetActive(false);

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
