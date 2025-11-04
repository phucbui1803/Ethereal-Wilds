using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    [Header("References")]
    public PlayerStats playerStats;
    public WeaponManager weaponManager;

    [Header("Weapon")]
    public WeaponData defaultWeapon;
    private WeaponData equippedWeapon;
    private GameObject currentWeaponObj;

    [Header("Weapon Holder")]
    public Transform weaponHolder;

    private bool hasEquippedWeapon = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (weaponManager == null)
            weaponManager = FindAnyObjectByType<WeaponManager>();
    }

    private void Start()
    {
        if (defaultWeapon != null)
        {
            EquipWeapon(defaultWeapon, true); // ✅ Lần đầu: equip vũ khí mặc định
        }
    }

    /// <summary>
    /// Equip weapon mới
    /// </summary>
    public void EquipWeapon(WeaponData weapon, bool isDefault = false)
    {
        if (weapon == null)
        {
            Debug.LogWarning("[PlayerInventory] EquipWeapon thất bại: weapon = null");
            return;
        }

        // ✅ Nếu vũ khí mới giống vũ khí hiện tại thì bỏ qua
        if (hasEquippedWeapon && equippedWeapon == weapon)
        {
            Debug.Log($"[PlayerInventory] Vũ khí {weapon.weaponName} đã được trang bị. Bỏ qua.");
            return;
        }

        // --- Trước khi gỡ weapon cũ ---
        if (weaponManager != null)
        {
            weaponManager.EquipWeapon(null, false);
        }

        // ✅ Gỡ vũ khí cũ trước khi gán cái mới
        if (currentWeaponObj != null)
        {
            Destroy(currentWeaponObj);
            currentWeaponObj = null;
        }

        // ✅ Lưu vũ khí hiện tại
        equippedWeapon = weapon;
        hasEquippedWeapon = true;

        // Spawn vũ khí mới
        if (weapon.weaponPrefab != null && weaponHolder != null)
        {
            currentWeaponObj = Instantiate(weapon.weaponPrefab, weaponHolder);
            currentWeaponObj.transform.localPosition = Vector3.zero;
            currentWeaponObj.transform.localRotation = Quaternion.identity;
            currentWeaponObj.transform.localScale = Vector3.one;

            Debug.Log($"[PlayerInventory] Spawn vũ khí: {weapon.weaponPrefab.name}");
        }
        else
        {
            Debug.LogError($"[PlayerInventory] Không thể spawn vũ khí {weapon.weaponName}. Kiểm tra weaponPrefab hoặc weaponHolder!");
        }

        // Cập nhật stats
        if (playerStats != null)
            playerStats.EquipWeapon(weapon);
        else
            Debug.LogError("[PlayerInventory] playerStats = null");

        // Đặt weapon vào tay hoặc sheath
        if (weaponManager != null && currentWeaponObj != null)
        {
            bool isDrawn = weaponManager.IsWeaponInHand();
            weaponManager.EquipWeapon(currentWeaponObj, isDrawn);
        }

        Debug.Log($"Đã trang bị vũ khí: {weapon.weaponName}");
    }

    /// <summary>
    /// Gỡ vũ khí hiện có
    /// </summary>
    public void UnequipWeapon()
    {
        if (!hasEquippedWeapon) return;

        equippedWeapon = null;
        hasEquippedWeapon = false;

        // Cập nhật stats
        if (playerStats != null)
            playerStats.EquipWeapon(null);

        // Xóa prefab
        if (currentWeaponObj != null)
            Destroy(currentWeaponObj);
        currentWeaponObj = null;

        Debug.Log("Đã gỡ vũ khí.");
    }

    // Getter tiện lợi
    public WeaponData GetEquippedWeapon() => equippedWeapon;
    public GameObject GetCurrentWeaponObject() => currentWeaponObj;
}
