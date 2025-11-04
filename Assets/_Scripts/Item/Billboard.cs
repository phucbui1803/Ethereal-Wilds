using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCam == null) return;

        Vector3 lookPos = mainCam.transform.position - transform.position;
        lookPos.y = 0; // giữ cho không bị nghiêng
        transform.rotation = Quaternion.LookRotation(-lookPos);
    }
}
