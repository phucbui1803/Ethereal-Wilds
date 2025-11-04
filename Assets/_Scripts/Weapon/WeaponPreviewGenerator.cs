using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WeaponPreviewGenerator : MonoBehaviour
{
    [Header("Preview Settings")]
    public WeaponPreviewCamera previewCamera;
    public Transform previewHolder;

    public RenderTexture GeneratePreview(GameObject weaponPrefab, int textureSize = 128)
    {
        if (weaponPrefab == null || previewCamera == null || previewHolder == null)
            return null;

        // Xóa các object cũ trong previewHolder
        foreach (Transform child in previewHolder)
            DestroyImmediate(child.gameObject);

        // Spawn prefab tạm trong previewHolder
        GameObject temp = Instantiate(weaponPrefab, previewHolder);
        temp.transform.localPosition = Vector3.zero;
        temp.transform.localRotation = Quaternion.Euler(0, 180, 0);
        temp.transform.localScale = Vector3.one;

        // Đặt layer để camera nhìn thấy
        SetLayerRecursively(temp, LayerMask.NameToLayer("WeaponPreview"));

        // Render prefab thành RenderTexture tạm
        RenderTexture rt = previewCamera.RenderWeaponToTexture(temp, textureSize, textureSize);

        DestroyImmediate(temp); // dọn object tạm
        return rt;
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }
}
