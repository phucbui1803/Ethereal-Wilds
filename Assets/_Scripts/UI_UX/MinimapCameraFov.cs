using UnityEngine;

public class MinimapCameraFOV : MonoBehaviour
{
    public Transform mainCam;  // Main Camera (camera chính trong game)

    void Update()
    {
        if (mainCam == null)
        {
            return;
        }

        // Xoay vùng sáng theo hướng camera chính
        Vector3 rot = transform.localEulerAngles;
        rot.z = -mainCam.eulerAngles.y;  // ngược trục y để xoay đúng minimap
        transform.localEulerAngles = rot;
    }
}
