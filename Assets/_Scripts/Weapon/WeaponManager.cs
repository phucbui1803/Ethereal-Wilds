using UnityEngine;
using StarterAssets;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Slots")]
    public Transform weaponGrip;
    public Transform sheathGrip;

    private GameObject weaponInstance;
    private bool weaponInHand = false;

    private Animator animator;
    public ThirdPersonController controller;

    public bool isBusy { get; private set; } = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        if (controller == null)
            controller = GetComponent<ThirdPersonController>();

        if (PlayerInventory.Instance != null)
            weaponInstance = PlayerInventory.Instance.GetCurrentWeaponObject();

        if (weaponInstance != null)
            SheathWeapon();
    }

    public void EquipWeapon(GameObject weapon, bool drawImmediately = false)
    {
        weaponInstance = weapon;

        if (drawImmediately)
            DrawWeapon();
        else
            SheathWeapon();
    }

    public void DrawWeapon()
    {
        if (weaponInstance == null) return;
        weaponInstance.transform.SetParent(weaponGrip);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
        weaponInstance.transform.localScale = Vector3.one;
        weaponInHand = true;
    }

    public void SheathWeapon()
    {
        if (weaponInstance == null) return;
        weaponInstance.transform.SetParent(sheathGrip);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;
        weaponInstance.transform.localScale = Vector3.one;
        weaponInHand = false;
    }

    public bool IsWeaponInHand() => weaponInHand;

    public void OnDrawStart()
    {
        isBusy = true;
        if (controller != null) controller.isDrawWeapon = true;
    }

    public void OnDrawEnd()
    {
        isBusy = false;
        DrawWeapon();
        if (controller != null) controller.isDrawWeapon = false;
    }

    public void OnSheathStart()
    {
        isBusy = true;
        if (controller != null) controller.isDrawWeapon = true;
    }

    public void OnSheathEnd()
    {
        isBusy = false;
        SheathWeapon();
        if (controller != null) controller.isDrawWeapon = false;
    }
}
