using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LootNotificationSlot : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public TMP_Text itemNameText;
    public TMP_Text quantityText;

    [Header("Timing")]
    public float displayTime = 2f;
    public float fadeTime = 0.3f;

    private System.Action<LootNotificationSlot> onFinished;

    public void Setup(ItemData item, int quantity, System.Action<LootNotificationSlot> onFinished)
    {
        this.onFinished = onFinished;

        icon.sprite = item.icon;
        itemNameText.text = item.itemName;
        quantityText.text = "x" + quantity;

        StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        // Fade In
        CanvasGroup cg = GetComponent<CanvasGroup>();
        cg.alpha = 0;
        float t = 0;
        while (t < fadeTime)
        {
            cg.alpha = Mathf.Lerp(0, 1, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 1;

        // Đợi hiển thị
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        t = 0;
        while (t < fadeTime)
        {
            cg.alpha = Mathf.Lerp(1, 0, t / fadeTime);
            t += Time.deltaTime;
            yield return null;
        }
        cg.alpha = 0;

        // Gọi callback để thông báo slot đã xong
        onFinished?.Invoke(this);
    }
}
