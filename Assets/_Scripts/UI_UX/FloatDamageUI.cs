using UnityEngine;
using TMPro;

public class FloatDamageUI : MonoBehaviour
{
    public float duration = 2f;              // Thời gian hiển thị tổng
    public float appearMoveDistance = 0.3f;  // Khoảng cách di chuyển lúc spawn
    public float appearMoveTime = 0.1f;      // Thời gian di chuyển nhanh
    private float timer = 0f;

    private TextMeshPro text;
    private Color originalColor;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool hasReachedTarget = false;

    void Awake()
    {
        text = GetComponent<TextMeshPro>();

        if (text == null)
        {
            Debug.LogWarning("Không tìm thấy TextMeshPro trên prefab!");
        }
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Nếu chưa tới vị trí đích, di chuyển nhanh tới đó
        if (!hasReachedTarget)
        {
            float t = Mathf.Clamp01(timer / appearMoveTime);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (t >= 1f)
            {
                hasReachedTarget = true;
                timer = 0f;
            }
        }
        else
        {
            // Sau khi đứng yên đủ lâu thì hủy
            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }

        // Luôn quay mặt về phía camera
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }
    }

    /// <summary>
    /// Setup vị trí spawn: 
    /// - Enemy: lệch trái + ra trước
    /// - Player: lệch phải
    /// </summary>
    public void Setup(Transform targetTransform, float dmg, Color color, bool isCrit = false, bool isPlayer = false)
    {
        if (text != null)
        {
            text.text = isCrit ? $"{dmg:F0}" : dmg.ToString("F0");
            text.color = color;
            originalColor = color;

            if (isCrit)
                text.fontSize *= 1.5f;
        }

        Vector3 basePos = targetTransform.position;

        Vector3 offset;

        if (isPlayer)
        {
            // Player nhận sát thương → lệch sang phải, không ra trước
            offset = targetTransform.right * 0.7f + new Vector3(0, 1.5f, 0);
        }
        else
        {
            // Enemy bị đánh → lệch sang trái và ra trước
            offset = -targetTransform.right * 2.0f + targetTransform.forward * 0.5f + new Vector3(0, 1.5f, 0);
        }

        targetPos = basePos + offset;
        startPos = targetPos - new Vector3(0, appearMoveDistance, 0);

        transform.position = startPos;
    }
}
