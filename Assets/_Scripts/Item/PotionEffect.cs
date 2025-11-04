using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class PotionEffect
{
    // Dictionary lưu buff đang hoạt động cho mỗi player
    private static Dictionary<PlayerStats, Dictionary<PotionData, BuffData>> activeBuffs
        = new Dictionary<PlayerStats, Dictionary<PotionData, BuffData>>();

    private class BuffData
    {
        public Coroutine coroutine;
        public float remainingTime;
        public float atkPercent;
        public float defPercent;
        public float critRate;
        public float critDamage;
    }

    public static void ApplyPotionEffect(PotionData potion, PlayerStats player)
    {
        if (potion == null || player == null) return;

        // Hồi máu ngay lập tức
        if (potion.healAmount > 0)
            player.Heal((int)potion.healAmount);

        // Buff tạm thời nếu duration > 0
        if (potion.duration > 0)
        {
            if (!activeBuffs.ContainsKey(player))
                activeBuffs[player] = new Dictionary<PotionData, BuffData>();

            if (activeBuffs[player].ContainsKey(potion))
            {
                // Buff đang active → cộng thêm thời gian
                BuffData data = activeBuffs[player][potion];
                data.remainingTime += potion.duration;
                // Cập nhật HUD thời gian buff
                BuffHUDManager.Instance?.UpdateBuffTime(potion.icon, data.remainingTime);
            }
            else
            {
                // Tạo buff mới
                BuffData data = new BuffData
                {
                    remainingTime = potion.duration,
                    atkPercent = potion.bonusATKPercent,
                    defPercent = potion.bonusDEFPercent,
                    critRate = potion.bonusCritRate,
                    critDamage = potion.bonusCritDamage
                };

                // Áp buff vào PlayerStats
                player.ApplyAttackBuff(data.atkPercent);
                player.ApplyDefenseBuff(data.defPercent);
                player.ApplyCritRateBuff(data.critRate);
                player.ApplyCritDamageBuff(data.critDamage);

                data.coroutine = player.StartCoroutine(ApplyBuffCoroutine(potion, player, data));
                activeBuffs[player][potion] = data;
                // Hiển thị icon buff trên HUD
                BuffHUDManager.Instance?.ShowBuffIcon(potion.icon, potion.duration);
            }
        }
        else
        {
            // Buff vĩnh viễn → cộng vào % / amount buff
            player.ApplyAttackBuff(potion.bonusATKPercent);
            player.ApplyDefenseBuff(potion.bonusDEFPercent);
            player.ApplyCritRateBuff(potion.bonusCritRate);
            player.ApplyCritDamageBuff(potion.bonusCritDamage);
        }
    }

    private static IEnumerator ApplyBuffCoroutine(PotionData potion, PlayerStats player, BuffData data)
    {
        while (data.remainingTime > 0)
        {
            // Cập nhật HUD timer mỗi frame
            BuffHUDManager.Instance?.UpdateBuffTime(potion.icon, data.remainingTime);

            data.remainingTime -= Time.deltaTime;
            yield return null;
        }

        // Hết thời gian → loại bỏ buff
        player.RemoveAttackBuff(data.atkPercent);
        player.RemoveDefenseBuff(data.defPercent);
        player.RemoveCritRateBuff(data.critRate);
        player.RemoveCritDamageBuff(data.critDamage);

        // Xóa buff khỏi activeBuffs
        if (activeBuffs.ContainsKey(player))
            activeBuffs[player].Remove(potion);

        // Ẩn icon buff trên HUD
        BuffHUDManager.Instance?.HideBuffIcon(potion.icon);
    }
}
