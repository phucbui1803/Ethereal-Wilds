using UnityEngine;
using System.Text;

[CreateAssetMenu(fileName = "NewPotion", menuName = "Inventory/Potion")]
public class PotionData : ScriptableObject
{
    [Header("Basic Info")]
    public string potionName;
    [TextArea(2, 5)] public string description;
    public Sprite icon;
    public int maxStack = 999;
    public bool isConsumable = true;

    [Header("Restore Values")]
    public float healAmount;

    [Header("Buffs (Temporary)")]
    public float duration = 15f;
    public float bonusATKPercent;
    public float bonusDEFPercent;
    public float bonusCritRate;
    public float bonusCritDamage;

    [Header("Crafting Requirements")]
    public ItemData material1;
    public int material1Amount = 1;
    public ItemData material2;
    public int material2Amount = 1;

    public string GetDynamicDescription()
    {
        StringBuilder sb = new StringBuilder();

        // Heal
        if (healAmount > 0)
            sb.AppendLine($"Healing <color=#4DFF4D>{healAmount}</color> HP instantly.");

        // Attack buff/debuff
        if (bonusATKPercent != 0)
        {
            if (bonusATKPercent > 0)
                sb.AppendLine($"Increase <color=#FF4D4D>Attack by {bonusATKPercent * 100:F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
            else
                sb.AppendLine($"Reduce <color=#8C8C8C>Attack by {Mathf.Abs(bonusATKPercent * 100):F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
        }

        // Defense buff/debuff
        if (bonusDEFPercent != 0)
        {
            if (bonusDEFPercent > 0)
                sb.AppendLine($"Increase <color=#4DB8FF>Defense by {bonusDEFPercent * 100:F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
            else
                sb.AppendLine($"Reduce <color=#8C8C8C>Defense by {Mathf.Abs(bonusDEFPercent * 100):F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
        }

        // Crit rate
        if (bonusCritRate != 0)
        {
            if (bonusCritRate > 0)
                sb.AppendLine($"Increase <color=#FFD24D>Critical Rate by {bonusCritRate * 100:F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
            else
                sb.AppendLine($"Reduce <color=#8C8C8C>Critical Rate by {Mathf.Abs(bonusCritRate * 100):F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
        }

        // Crit damage
        if (bonusCritDamage != 0)
        {
            if (bonusCritDamage > 0)
                sb.AppendLine($"Increase <color=#FF8C4D>Critical Damage by {bonusCritDamage * 100:F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
            else
                sb.AppendLine($"Reduce <color=#8C8C8C>Critical Damage by {Mathf.Abs(bonusCritDamage * 100):F0}%</color> for <color=#4DD2FF>{duration:F0}s</color>.");
        }

        if (sb.Length == 0)
            sb.Append("No special effects.");

        return sb.ToString().Trim();
    }
}
