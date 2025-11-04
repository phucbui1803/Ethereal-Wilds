using UnityEngine;
using System.Collections.Generic;

public class WeaponInventoryManager : MonoBehaviour
{
    [Header("References")]
    public WeaponPanelUI weaponPanelUI;
    public WeaponPreviewGenerator previewGenerator;
    public List<WeaponData> ownedWeapons;

    private void Start()
    {
        if (ownedWeapons == null || ownedWeapons.Count == 0)
        {
            Debug.LogWarning("WeaponInventory: Chưa có vũ khí nào trong ownedWeapons!");
            return;
        }

        GenerateWeaponPreviews();
        ShowWeaponInventory();
    }

    private void GenerateWeaponPreviews()
    {
        if (previewGenerator == null)
        {
            Debug.LogWarning("InventoryManager: Chưa gán previewGenerator!");
            return;
        }

        foreach (var weapon in ownedWeapons)
        {
            if (weapon == null)
            {
                Debug.LogWarning("WeaponInventory: Có weapon null trong danh sách!");
                continue;
            }

            if (weapon.weaponPrefab != null)
            {
                weapon.renderTexture = previewGenerator.GeneratePreview(weapon.weaponPrefab);
            }
            else
            {
                Debug.LogWarning($"WeaponInventory: Weapon {weapon.weaponName} chưa gán prefab!");
            }
        }
    }

    public void ShowWeaponInventory()
    {
        if (weaponPanelUI != null)
        {
            weaponPanelUI.ShowWeapons(ownedWeapons);
            Debug.Log("[WeaponInventory] Inventory UI hiển thị thành công!");
        }
        else
        {
            Debug.LogWarning("InventoryManager: Chưa gán WeaponPanelUI!");
        }
    }
}
