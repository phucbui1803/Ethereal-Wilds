using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MenuController : MonoBehaviour
{
    public GameObject panelMain;
    public GameObject panelButton;
    public GameObject panelSetting;
    public GameObject panelLoading;

    public Button buttonStart;
    public Button buttonQuit;
    public Button buttonSetting;
    public Button buttonAccept;

    public Slider loadingSlider;
    public TMP_Text loadingText;

    void Start()
    {
        panelMain.SetActive(true);
        panelButton.SetActive(true);
        panelSetting.SetActive(false);
        panelLoading.SetActive(false);

        buttonStart.onClick.AddListener(OnStartButton);
        buttonQuit.onClick.AddListener(OnQuitButton);
        buttonSetting.onClick.AddListener(OnSettingButton);
        buttonAccept.onClick.AddListener(OnAcceptButton);
    }

    // Button Start
    public void OnStartButton()
    {
        Debug.Log("StartButton clicked");
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Click");
        else
            Debug.LogWarning("AudioManager missing!");

        StopAllCoroutines();
        panelMain.SetActive(false);
        panelLoading.SetActive(true);
        StartCoroutine(LoadGameSceneAsync());
    }

    // Coroutine load scene
    private IEnumerator LoadGameSceneAsync()
    {
        Debug.Log(">>> Start loading GameScene");

        AsyncOperation operation = SceneManager.LoadSceneAsync("GameScene");

        //Optional: không cho scene active ngay lập tức
        operation.allowSceneActivation = false;

        float fakeProgress = 0f;

        while (!operation.isDone)
        {
            // Unity's load progress maxes at 0.9f until allowSceneActivation = true
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            fakeProgress = Mathf.MoveTowards(fakeProgress, targetProgress, Time.deltaTime);

            if (loadingSlider != null)
                loadingSlider.value = fakeProgress;

            if (loadingText != null)
                loadingText.text = Mathf.RoundToInt(fakeProgress * 100f) + "%";

            // Khi đạt 100%, cho phép kích hoạt scene
            if (fakeProgress >= 1f)
            {
                Debug.Log(">>> Finished loading, activating GameScene");
                operation.allowSceneActivation = true;
            }

            yield return null;
        }

        Debug.Log(">>> Scene loaded successfully!");
    }

    // Button Quit
    public void OnQuitButton()
    {
        AudioManager.Instance.PlaySFX("Click");
        Debug.Log("Quit Game");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // Thoát game khi build
#endif
    }

    // Button Setting
    public void OnSettingButton()
    {
        AudioManager.Instance.PlaySFX("Click");
        panelButton.SetActive(false);
        panelSetting.SetActive(true);
    }

    // Nút Accept trong panel Setting
    public void OnAcceptButton()
    {
        AudioManager.Instance.PlaySFX("Click");
        panelSetting.SetActive(false);
        panelButton.SetActive(true);
    }
}
