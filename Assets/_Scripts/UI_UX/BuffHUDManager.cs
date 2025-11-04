using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class BuffHUDManager : MonoBehaviour
{
    public static BuffHUDManager Instance;

    [Header("References")]
    public Transform buffPanel;        // Panel chứa buff icon
    public GameObject buffIconPrefab;  // Prefab: icon + glow image + TMP_Text thời gian

    private class ActiveBuff
    {
        public GameObject iconObj;
        public Image glowImage;
        public TMP_Text timeText;     // Text hiển thị thời gian
        public float duration;        // Thời gian gốc (để glow fill)
        public float timeRemaining;   // Thời gian còn lại
        public Sprite potionIcon;     // Để check trùng
    }

    private readonly List<ActiveBuff> activeBuffs = new();

    private void Awake() => Instance = this;

    private void Update()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuff buff = activeBuffs[i];
            buff.timeRemaining -= Time.deltaTime;

            // Update glow fill
            if (buff.glowImage != null)
                buff.glowImage.fillAmount = Mathf.Clamp01(buff.timeRemaining / buff.duration);

            // Update thời gian hiển thị
            if (buff.timeText != null)
                buff.timeText.text = Mathf.Ceil(buff.timeRemaining).ToString();

            // Hết thời gian → xóa icon
            if (buff.timeRemaining <= 0)
            {
                Destroy(buff.iconObj);
                activeBuffs.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Hiển thị icon buff hoặc cộng dồn thời gian nếu đã tồn tại
    /// </summary>
    public void ShowBuffIcon(Sprite icon, float duration)
    {
        if (icon == null || buffIconPrefab == null || buffPanel == null) return;

        // Kiểm tra buff đã tồn tại
        ActiveBuff existingBuff = activeBuffs.Find(b => b.potionIcon == icon);
        if (existingBuff != null)
        {
            existingBuff.timeRemaining += duration; // cộng dồn thời gian còn lại
            return;
        }

        // Tạo icon mới
        GameObject newIcon = Instantiate(buffIconPrefab, buffPanel);
        Image[] images = newIcon.GetComponentsInChildren<Image>();
        TMP_Text timeText = newIcon.GetComponentInChildren<TMP_Text>();

        if (images.Length < 2 || timeText == null)
        {
            Debug.LogWarning("BuffIconPrefab phải có: icon + glow + TMP_Text thời gian.");
            Destroy(newIcon);
            return;
        }

        images[0].sprite = icon;      // icon chính
        images[1].fillAmount = 1f;    // glow fill

        activeBuffs.Add(new ActiveBuff
        {
            iconObj = newIcon,
            glowImage = images[1],
            timeText = timeText,
            duration = duration,
            timeRemaining = duration,
            potionIcon = icon
        });
    }

    /// <summary>
    /// Cập nhật thời gian buff trên HUD (nếu cần gọi từ PotionEffect)
    /// </summary>
    public void UpdateBuffTime(Sprite icon, float timeRemaining)
    {
        ActiveBuff buff = activeBuffs.Find(b => b.potionIcon == icon);
        if (buff != null)
            buff.timeRemaining = timeRemaining;
    }

    /// <summary>
    /// Ẩn icon khi buff kết thúc
    /// </summary>
    public void HideBuffIcon(Sprite icon)
    {
        ActiveBuff buff = activeBuffs.Find(b => b.potionIcon == icon);
        if (buff != null)
        {
            Destroy(buff.iconObj);
            activeBuffs.Remove(buff);
        }
    }
}
