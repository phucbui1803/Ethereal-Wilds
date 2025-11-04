using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class IconSkillUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image icon;             // Icon của skill
    public Image overlay;          // Hiệu ứng sáng khi sẵn sàng
    public Image cooldownMask;     // Mask vòng tròn che khi cooldown
    public TMP_Text cooldownText;  // Text hiển thị số giây còn lại

    [Header("Settings")]
    public bool blinkWhenReady = true;  // Nhấp nháy khi hồi xong
    public float blinkSpeed = 2f;       // Tốc độ nhấp nháy overlay
    public float blinkDuration = 2f;    // Thời gian nhấp nháy (giây)

    private Coroutine cooldownRoutine;
    private Coroutine blinkRoutine;
    public bool isOnCooldown { get; private set; }

    private float currentEnergy;
    private float maxEnergy;

    private void Awake()
    {
        ResetUI();
    }

    #region ENERGY (Skill R)
    public void SetEnergy(float current, float max)
    {
        currentEnergy = current;
        maxEnergy = max;

        if (overlay != null)
        {
            overlay.fillAmount = currentEnergy / maxEnergy; // overlay biểu thị năng lượng
        }

        // Nếu đầy năng lượng, overlay nhấp nháy 2s
        if (currentEnergy >= maxEnergy && blinkWhenReady)
        {
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
            }

            blinkRoutine = StartCoroutine(BlinkOverlayOnce(blinkDuration));
        }
    }
    #endregion

    #region COOLDOWN
    public void StartCooldown(float cooldownTime)
    {
        if (cooldownTime <= 0 || !gameObject.activeInHierarchy || isOnCooldown)
        {
            return;
        }

        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        cooldownRoutine = StartCoroutine(CooldownRoutine(cooldownTime));
    }

    private IEnumerator CooldownRoutine(float time)
    {
        isOnCooldown = true;

        if (overlay != null)
        {
            overlay.enabled = true;

            // Dừng blink nếu đang chạy
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
                blinkRoutine = null;
            }
        }

        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0, time - elapsed);

            if (cooldownMask != null)
            {
                cooldownMask.fillAmount = 1 - (elapsed / time);
            }

            if (cooldownText != null)
            {
                cooldownText.text = remaining.ToString("F1");
            }

            yield return null;
        }

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 0;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        isOnCooldown = false;

        if (overlay != null)
        {
            overlay.enabled = true;
        }

        // Nếu Skill R đầy, nhấp nháy overlay 2s
        if (currentEnergy >= maxEnergy && blinkWhenReady)
        {
            if (blinkRoutine != null)
            {
                StopCoroutine(blinkRoutine);
            }

            blinkRoutine = StartCoroutine(BlinkOverlayOnce(blinkDuration));
        }
    }

    private IEnumerator BlinkOverlayOnce(float duration)
    {
        float timer = 0f;
        float alpha = 1f;
        bool fadingOut = true;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            alpha += (fadingOut ? -1 : 1) * blinkSpeed * Time.deltaTime;

            if (alpha <= 0.3f)
            {
                alpha = 0.3f;
                fadingOut = false;
            }
            else if (alpha >= 1f)
            {
                alpha = 1f;
                fadingOut = true;
            }

            if (overlay != null)
            {
                Color c = overlay.color;
                c.a = alpha;
                overlay.color = c;
            }

            yield return null;
        }

        if (overlay != null)
        {
            Color c = overlay.color;
            c.a = 1f;
            overlay.color = c;
        }
    }
    #endregion

    private void ResetUI()
    {
        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 0;
        }

        if (cooldownText != null)
        {
            cooldownText.text = "";
        }

        if (overlay != null)
        {
            overlay.enabled = true;
        }
    }
}
