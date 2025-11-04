using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class InteractUI : MonoBehaviour
{
    public static InteractUI Instance;

    [Header("References")]
    public Button panelButton;        // Thay Image thành Button
    public TMP_Text interactText;

    private System.Action onClickAction; // Hành động khi nhấn button

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        panelButton.gameObject.SetActive(false);

        // Gán listener cho button
        panelButton.onClick.RemoveAllListeners();
        panelButton.onClick.AddListener(() =>
        {
            onClickAction?.Invoke();
        });
    }

    public void Show(string interactName, System.Action onClick)
    {
        panelButton.gameObject.SetActive(true);
        interactText.text = $"[F] {interactName}";
        onClickAction = onClick;
    }

    /// <summary>
    /// Ẩn panel button
    /// </summary>
    public void Hide()
    {
        panelButton.gameObject.SetActive(false);
        onClickAction = null;
    }

    /// <summary>
    /// Cho phép gọi tương tác từ F key
    /// </summary>
    public void Press()
    {
        onClickAction?.Invoke();
        Hide();
    }
}
