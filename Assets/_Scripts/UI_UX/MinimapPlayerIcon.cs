using UnityEngine;
using UnityEngine.UI;

public class MinimapPlayerIcon : MonoBehaviour
{
    public Transform player; // Gán player vào trong Inspector

    void Update()
    {
        // Giữ icon xoay theo hướng player
        Vector3 rot = transform.localEulerAngles;
        rot.z = -player.eulerAngles.y; // Dùng -y để xoay đúng chiều minimap    
        transform.localEulerAngles = rot;
    }
}
