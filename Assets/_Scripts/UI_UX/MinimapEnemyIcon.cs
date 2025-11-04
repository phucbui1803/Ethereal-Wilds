using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MinimapEnemyIcon : MonoBehaviour
{
    [Header("References")]
    public Camera minimapCamera;         // Camera minimap (orthographic, từ trên nhìn xuống)
    public RectTransform minimapUIRoot;  // Panel minimap trên HUD (ví dụ RawImage)
    public Image enemyIconPrefab;        // Prefab icon enemy (UI Image)

    private Dictionary<Transform, Image> enemyIcons = new Dictionary<Transform, Image>();

    void Start()
    {
        // Lấy tất cả enemy (tag "Enemy")
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            AddEnemy(enemy.transform);
        }
    }

    void Update()
    {
        List<Transform> toRemove = new List<Transform>();

        foreach (var kvp in enemyIcons)
        {
            Transform enemy = kvp.Key;
            Image icon = kvp.Value;

            if (enemy == null)
            {
                Destroy(icon.gameObject);
                toRemove.Add(enemy);
                continue;
            }

            // Lấy vị trí enemy trong viewport (0..1)
            Vector3 viewportPos = minimapCamera.WorldToViewportPoint(enemy.position);

            if (viewportPos.z > 0) // chỉ hiển thị khi enemy nằm trong phía trước camera minimap
            {
                icon.enabled = true;

                // Chuyển viewport (0..1) thành local anchoredPosition trên minimapUIRoot
                float x = (viewportPos.x - 0.5f) * minimapUIRoot.rect.width;
                float y = (viewportPos.y - 0.5f) * minimapUIRoot.rect.height;
                icon.rectTransform.anchoredPosition = new Vector2(x, y);
            }
            else
            {
                icon.enabled = false;
            }
        }

        // Xoá icon của enemy đã chết
        foreach (var e in toRemove)
        {
            enemyIcons.Remove(e);
        }
    }

    public void AddEnemy(Transform enemy)
    {
        if (!enemyIcons.ContainsKey(enemy))
        {
            Image newIcon = Instantiate(enemyIconPrefab, minimapUIRoot);
            enemyIcons.Add(enemy, newIcon);
        }
    }

    public void RemoveEnemy(Transform enemy)
    {
        if (enemyIcons.ContainsKey(enemy))
        {
            Destroy(enemyIcons[enemy].gameObject);
            enemyIcons.Remove(enemy);
        }
    }
}
