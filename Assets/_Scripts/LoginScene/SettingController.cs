using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingController : MonoBehaviour
{
    public Slider sliderMusic;
    public Slider sliderSFX;

    [Header("Shadow Settings")]
    public Button btnShadowLeft;
    public Button btnShadowRight;
    public TMP_Text txtShadowQuality;

    private int currentShadowIndex = 2; // 0 = Off, 1 = Low, 2 = High
    private readonly string[] shadowOptions = { "Off", "Low", "High" };

    private void Start()
    {
        // Load saved settings
        sliderMusic.value = PlayerPrefs.GetFloat("Music Volume", 1f);
        sliderSFX.value = PlayerPrefs.GetFloat("SFX Volume", 1f);
        currentShadowIndex = PlayerPrefs.GetInt("Shadow Quality", 2);

        // ===== Add listeners =====
        sliderMusic.onValueChanged.AddListener(SetMusicVolume);
        sliderSFX.onValueChanged.AddListener(SetSFXVolume);

        btnShadowLeft.onClick.AddListener(OnShadowLeft);
        btnShadowRight.onClick.AddListener(OnShadowRight);

        // ===== Apply settings on start =====
        UpdateShadowLabel();
        SetShadowQuality(currentShadowIndex);
    }

    //====================== Volume ======================//
    private void SetMusicVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);

        PlayerPrefs.SetFloat("Music Volume", value);
        PlayerPrefs.Save();
    }

    private void SetSFXVolume(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);

        PlayerPrefs.SetFloat("SFX Volume", value);
        PlayerPrefs.Save();
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
}
