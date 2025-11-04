using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class EnemyHPBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider hpSlider;
    public TextMeshProUGUI enemyNameText;

    [Header("Target Settings")]
    public Transform target;                     // Tự tìm nếu chưa gán
    public string anchorName = "HPBarAnchor";    // Tên object trong prefab enemy
    public Vector3 offset = new Vector3(0, 0, 0);
    public float showDistance = 15f;

    [Header("Fade Settings")]
    public float fadeSpeed = 10f;
    public float defaultDestroyFadeTime = 0.5f;

    private Camera mainCam;
    private CanvasGroup canvasGroup;
    private float currentAlpha = 0f;
    private bool isBeingDestroyed = false;

    private EnemyStats enemyStats;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Start()
    {
        mainCam = Camera.main;
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        // Lấy EnemyStats tự động
        enemyStats = target != null ? target.GetComponentInParent<EnemyStats>() : null;
    }

    void LateUpdate()
    {
        if (target == null || mainCam == null || isBeingDestroyed) return;

        transform.position = target.position + offset;
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);

        // Hiệu ứng fade dựa theo khoảng cách
        float dist = Vector3.Distance(mainCam.transform.position, target.position);
        float targetAlpha = dist <= showDistance ? 1f : 0f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);
        canvasGroup.alpha = currentAlpha;

        // Cập nhật tên và level tự động
        if (enemyStats != null)
            SetName($"Lv. {enemyStats.GetLevel()} {enemyStats.enemyName}");

        // Cập nhật thanh HP
        if (enemyStats != null && hpSlider != null)
            hpSlider.value = Mathf.Clamp01(enemyStats.currentHP / enemyStats.maxHP);
    }

    /// <summary>
    /// Gán enemy, tìm anchor nếu chưa gán
    /// </summary>
    public void Initialize(Transform enemyRoot, string enemyName, float currentHP, float maxHP)
    {
        if (enemyRoot != null)
        {
            Transform anchor = enemyRoot.Find(anchorName);
            target = anchor != null ? anchor : enemyRoot;
        }

        SetName($"Lv. {enemyStats?.GetLevel() ?? 1} {enemyName}");
        UpdateHP(currentHP, maxHP);

        enemyStats = enemyRoot != null ? enemyRoot.GetComponent<EnemyStats>() : null;
    }

    public void SetName(string name)
    {
        if (enemyNameText != null)
            enemyNameText.text = name;
    }

    public void UpdateHP(float currentHP, float maxHP)
    {
        if (hpSlider != null)
            hpSlider.value = Mathf.Clamp01(currentHP / maxHP);
    }

    public void FadeOutAndDestroy(float duration = -1f, System.Action onComplete = null)
    {
        if (!isBeingDestroyed)
        {
            if (duration < 0) duration = defaultDestroyFadeTime;
            StartCoroutine(FadeOutCoroutine(duration, onComplete));
        }
    }

    private IEnumerator FadeOutCoroutine(float duration, System.Action onComplete)
    {
        isBeingDestroyed = true;
        float startAlpha = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, normalized);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        onComplete?.Invoke();
        Destroy(gameObject);
    }
}
