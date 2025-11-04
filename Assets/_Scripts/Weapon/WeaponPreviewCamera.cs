using UnityEngine;

[RequireComponent(typeof(Camera))]
public class WeaponPreviewCamera : MonoBehaviour
{   
    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null) 
        { 
            Debug.LogError("[WeaponPreviewCamera] Camera missing!"); 
            return; 
        }

        cam.cullingMask = LayerMask.GetMask("WeaponPreview");
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.clear;
        cam.enabled = false;
    }

    public RenderTexture RenderWeaponToTexture(GameObject prefab, int width = 128, int height = 128)
    {
        if (prefab == null) return null;

        // Tạo RenderTexture cho slot
        RenderTexture rt = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 1;
        rt.Create();

        // Spawn prefab tạm
        GameObject temp = Instantiate(prefab);
        temp.transform.position = Vector3.zero;
        temp.transform.rotation = Quaternion.Euler(0, 180, 0);
        temp.transform.localScale = Vector3.one;

        // Đặt layer để camera nhìn thấy
        SetLayerRecursively(temp, LayerMask.NameToLayer("WeaponPreview"));

        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = null;

        DestroyImmediate(temp);
        return rt;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
