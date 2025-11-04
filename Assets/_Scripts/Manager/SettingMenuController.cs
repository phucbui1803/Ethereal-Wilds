using UnityEngine;
using UnityEngine.UI;
using TMPro;
using StarterAssets;
using Cinemachine;

public class SettingMenuController : MonoBehaviour
{
    [Header("Sound Settings")]
    public Slider sliderMusic;
    public Slider sliderSFX;
    public TMP_Text txtMusicValue;
    public TMP_Text txtSFXValue;

    [Header("Shadow Settings")]
    public Button btnShadowLeft;
    public Button btnShadowRight;
    public TMP_Text txtShadowQuality;

    [Header("Gameplay Settings")]
    public Slider sliderSensitivity;
    public Slider sliderCameraDistance;
    public TMP_Text txtSensitivityValue;
    public TMP_Text txtCameraDistanceValue;

    [Header("Log Out")]
    public Button btnQuit;

    // ===== Thêm tham chiếu thực tế =====
    private ThirdPersonController playerController;
    private Transform mainCamera;
    private Vector3 cameraDefaultOffset;
    private float baseCameraDistance = 6f;

    private int currentShadowIndex = 2; // 0 = Off, 1 = Low, 2 = High
    private readonly string[] shadowOptions = { "Off", "Low", "High" };

    private void Start()
    {
        // ===== Tìm player và camera =====
        playerController = Object.FindFirstObjectByType<ThirdPersonController>();
        if (Camera.main != null)
        {
            mainCamera = Camera.main.transform;
            cameraDefaultOffset = mainCamera.localPosition;
            baseCameraDistance = cameraDefaultOffset.magnitude;
        }

        // Load saved settings
        sliderMusic.value = PlayerPrefs.GetFloat("Music Volume", 1f);
        sliderSFX.value = PlayerPrefs.GetFloat("SFX Volume", 1f);
        currentShadowIndex = PlayerPrefs.GetInt("Shadow Quality", 2);

        sliderSensitivity.value = PlayerPrefs.GetFloat("Mouse Sensitivity", 1f);
        sliderCameraDistance.value = PlayerPrefs.GetFloat("Camera Distance", 1f);

        // ===== Add listeners =====
        sliderMusic.onValueChanged.AddListener(SetMusicVolume);
        sliderSFX.onValueChanged.AddListener(SetSFXVolume);

        btnShadowLeft.onClick.AddListener(OnShadowLeft);
        btnShadowRight.onClick.AddListener(OnShadowRight);

        sliderSensitivity.onValueChanged.AddListener(SetSensitivity);
        sliderCameraDistance.onValueChanged.AddListener(SetCameraDistance);

        if (btnQuit != null)
            btnQuit.onClick.AddListener(OnQuitButton);

        // ===== Apply settings on start =====
        UpdateMusicVolumeLabel(sliderMusic.value);
        UpdateSFXVolumeLabel(sliderSFX.value);

        UpdateShadowLabel();
        SetShadowQuality(currentShadowIndex);

        UpdateSensitivityLabel(sliderSensitivity.value);
        UpdateCameraDistanceLabel(sliderCameraDistance.value);

        // Áp dụng giá trị ban đầu
        ApplySensitivity(sliderSensitivity.value);
        ApplyCameraDistance(sliderCameraDistance.value);
    }

    //====================== Volume ======================//
    private void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);

        PlayerPrefs.SetFloat("Music Volume", value);
        PlayerPrefs.Save();

        UpdateMusicVolumeLabel(value);
    }

    private void UpdateMusicVolumeLabel(float value)
    {
        if (txtMusicValue != null)
            txtMusicValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);

        PlayerPrefs.SetFloat("SFX Volume", value);
        PlayerPrefs.Save();

        UpdateSFXVolumeLabel(value);
    }

    private void UpdateSFXVolumeLabel(float value)
    {
        if (txtSFXValue != null)
            txtSFXValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    //====================== Shadow ======================//
    private void OnShadowLeft()
    {
        AudioManager.Instance.PlaySFX("Click");
        currentShadowIndex--;
        if (currentShadowIndex < 0)
            currentShadowIndex = shadowOptions.Length - 1; // quay vòng

        ApplyShadowChange();
    }

    private void OnShadowRight()
    {
        AudioManager.Instance.PlaySFX("Click");
        currentShadowIndex++;
        if (currentShadowIndex >= shadowOptions.Length)
            currentShadowIndex = 0; // quay vòng

        ApplyShadowChange();
    }

    private void ApplyShadowChange()
    {
        UpdateShadowLabel();
        SetShadowQuality(currentShadowIndex);

        PlayerPrefs.SetInt("Shadow Quality", currentShadowIndex);
        PlayerPrefs.Save();
    }

    private void UpdateShadowLabel()
    {
        txtShadowQuality.text = shadowOptions[currentShadowIndex];
    }

    private void SetShadowQuality(int index)
    {
        switch (index)
        {
            case 0: // Off
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.shadowDistance = 0f;
                if (RenderSettings.sun != null)
                    RenderSettings.sun.shadows = LightShadows.None;
                Debug.Log("Shadow: Off");
                break;

            case 1: // Low (Hard Shadows)
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 60f;
                if (RenderSettings.sun != null)
                    RenderSettings.sun.shadows = LightShadows.Hard;
                Debug.Log("Shadow: Low (Hard)");
                break;

            case 2: // High (Soft Shadows)
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadowDistance = 120f;
                if (RenderSettings.sun != null)
                    RenderSettings.sun.shadows = LightShadows.Soft;
                Debug.Log("Shadow: High (Soft)");
                break;
        }
    }

    //====================== Sensitivity ======================//
    private void SetSensitivity(float value)
    {
        PlayerPrefs.SetFloat("Mouse Sensitivity", value);
        PlayerPrefs.Save();

        UpdateSensitivityLabel(value);
        ApplySensitivity(value);

        // Nếu bạn có script điều khiển chuột, có thể cập nhật trực tiếp ở đây:
        // PlayerController.Instance.UpdateMouseSensitivity(value);
    }

    private void ApplySensitivity(float value)
    {
        if (playerController != null)
        {
            // Gán trực tiếp giá trị cho mouseSensitivity
            playerController.mouseSensitivity = Mathf.Lerp(0.2f, 1.0f, value);
        }
    }

    private void UpdateSensitivityLabel(float value)
    {
        if (txtSensitivityValue != null)
            txtSensitivityValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    //====================== Camera Distance ======================//
    private void SetCameraDistance(float value)
    {
        PlayerPrefs.SetFloat("Camera Distance", value);
        PlayerPrefs.Save();

        UpdateCameraDistanceLabel(value);
        ApplyCameraDistance(value);

        // Nếu có CameraFollow script, có thể áp dụng trực tiếp:
        // CameraFollow.Instance.SetDistance(value);
    }

    private void ApplyCameraDistance(float value)
    {
        // Dùng Cinemachine thay vì chỉnh localPosition thủ công
        var virtualCam = FindAnyObjectByType<CinemachineVirtualCamera>();
        if (virtualCam != null)
        {
            var followComp = virtualCam.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (followComp != null)
            {
                // Gán giá trị zoom trong khoảng 2f - 8f
                followComp.CameraDistance = Mathf.Lerp(2f, 6f, value);
            }
        }
    }

    private void UpdateCameraDistanceLabel(float value)
    {
        if (txtCameraDistanceValue != null)
            txtCameraDistanceValue.text = Mathf.RoundToInt(value * 100f).ToString();
    }

    //====================== QUIT ======================//
    private void OnQuitButton()
    {
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // Thoát game khi build
#endif
    }
}
