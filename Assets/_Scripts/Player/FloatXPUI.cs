using UnityEngine;
using TMPro;

public class FloatXPUI : MonoBehaviour
{
    public float duration = 1.5f;        // Thời gian tồn tại
    public float moveUpDistance = 0.5f;  // Di chuyển lên trên
    public float moveSpeed = 1f;

    private TextMeshPro textMesh;
    private Vector3 startPos;
    private Vector3 endPos;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void Setup(Vector3 playerPos, int xpAmount, Color color)
    {
        textMesh.text = $"+{xpAmount} XP";
        textMesh.color = color;

        // Vị trí spawn: bên phải player một chút, hơi cao lên
        startPos = playerPos + new Vector3(0.5f, 2f, 0);
        endPos = startPos + new Vector3(0, moveUpDistance, 0);

        transform.position = startPos;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Di chuyển lên
        float t = timer / duration;
        transform.position = Vector3.Lerp(startPos, endPos, t);

        // Quay về phía camera
        if (Camera.main != null)
        {
            transform.forward = Camera.main.transform.forward;
        }

        // Hủy khi hết thời gian
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}
