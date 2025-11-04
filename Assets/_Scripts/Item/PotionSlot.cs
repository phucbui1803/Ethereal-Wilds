using UnityEngine;

[System.Serializable]
public class PotionSlot
{
    public PotionData potionData;
    public int quantity;

    public PotionSlot(PotionData data, int qty)
    {
        potionData = data;
        quantity = qty;
    }
}
