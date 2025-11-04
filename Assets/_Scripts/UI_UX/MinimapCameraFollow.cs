using Unity.Jobs;
using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;              // Player để camera follow
    public Vector3 offset = new Vector3(0, 100, 0); // Vị trí lệch so với player

    [Header("Camera Settings")]
    public Camera minimapCamera;          // Camera minimap

    void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        // Cập nhật vị trí camera theo player
        transform.position = player.position + offset;

        // Giữ camera nhìn thẳng xuống
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        //// Nếu có menuController và menu đang mở → khóa zoom
        //if (PlayerMenuController.Instance != null && PlayerMenuController.Instance.IsMenuOpen)
        //    return;

        //if (InventoryMenuController.Instance != null && InventoryMenuController.Instance.IsMenuOpen)
        //    return;
    }
}
