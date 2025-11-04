using UnityEngine;
using TMPro;
using System.Collections;

public class NPCNameTag : MonoBehaviour
{
    public TextMeshProUGUI tmpText;

    private Transform target;
    private Vector3 offset = Vector3.zero;
    private CanvasGroup canvasGroup;

    [Header("Settings")]
    public float showDistance = 15f;
    public float fadeSpeed = 10f;
    private float currentAlpha = 0f;

    // -----------------------------
    // 🟢 KHỞI TẠO
    // -----------------------------
    public void Initialize(Transform targetTransform, string name)
    {
        target = targetTransform;

        tmpText = GetComponentInChildren<TextMeshProUGUI>();
        if (tmpText != null)
            tmpText.text = name;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    // -----------------------------
    // 🟢 CẬP NHẬT VỊ TRÍ & FADE
    // -----------------------------
    public void UpdatePositionAndFade(Camera cam)
    {
        if (target == null || cam == null) return;

        // Vị trí + xoay về camera
        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);

        // Fade theo khoảng cách
        float dist = Vector3.Distance(cam.transform.position, target.position);
        float targetAlpha = dist <= showDistance ? 1f : 0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        canvasGroup.alpha = currentAlpha;
    }

    // -----------------------------
    // 🟢 FADE OUT & DESTROY
    // -----------------------------
    public void FadeOutAndDestroy(float duration = 0.5f, System.Action onComplete = null)
    {
        StartCoroutine(FadeOutCoroutine(duration, onComplete));
    }

    private IEnumerator FadeOutCoroutine(float duration, System.Action onComplete)
    {
        float startAlpha = canvasGroup != null ? canvasGroup.alpha : 1f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            if (canvasGroup != null)
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}
