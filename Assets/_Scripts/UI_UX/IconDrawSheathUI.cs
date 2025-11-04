using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IconDrawSheathUI : MonoBehaviour
{
    public static IconDrawSheathUI Instance;

    [Header("Icon Sprites")]
    public Sprite handIcon;      // icon tay trống
    public Sprite weaponIcon;    // icon cầm vũ khí

    [Header("UI Reference")]
    public Image iconImage;
    public Image cooldownMask;
    public TMP_Text cooldownText;

    public bool isOnCooldown { get; private set; }

    private Coroutine cooldownRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SetWeaponDrawn(bool drawn)
    {
        if (iconImage == null) return;

        iconImage.sprite = drawn ? weaponIcon : handIcon;
    }

    public void StartCooldown(float time)
    {
        if (cooldownRoutine != null)
            StopCoroutine(cooldownRoutine);

        cooldownRoutine = StartCoroutine(CooldownRoutine(time));
    }

    private IEnumerator CooldownRoutine(float time)
    {
        isOnCooldown = true;

        float elapsed = 0f;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0, time - elapsed);

            if (cooldownMask != null)
                cooldownMask.fillAmount = 1f - (elapsed / time);

            if (cooldownText != null)
                cooldownText.text = remaining.ToString("F1");

            yield return null;
        }

        // Reset UI
        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;

        if (cooldownText != null)
            cooldownText.text = "";

        isOnCooldown = false;
        cooldownRoutine = null;
    }
}
