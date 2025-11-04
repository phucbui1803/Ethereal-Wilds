using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("Current Active Quest")]
    public QuestData activeQuest; // Chỉ nhận 1 quest tại một thời điểm

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Nhận quest mới
    /// </summary>
    public void AddQuest(QuestData quest)
    {
        if (activeQuest != null)
        {
            Debug.LogWarning("Bạn đang có quest khác chưa hoàn thành!");
            return;
        }

        activeQuest = quest;
        activeQuest.currentProgress = 0;
        Debug.Log($"Quest nhận: {activeQuest.questName}");
        UIQuestManager.Instance?.UpdateQuestUI();
    }

    // Nhặt item
    public void UpdateCollectProgress(string itemName)
    {
        if (activeQuest == null) return;

        if (activeQuest.type == QuestType.Collect && activeQuest.itemName == itemName)
        {
            activeQuest.currentProgress++;
            Debug.Log($"[{activeQuest.questName}] Progress: {activeQuest.currentProgress}/{activeQuest.collectAmount}");

            UIQuestManager.Instance?.UpdateQuestUI();

            if (activeQuest.currentProgress >= activeQuest.collectAmount)
            {
                CompleteQuest();
            }
        }
    }

    // Chế tạo potion
    public void UpdateCraftProgress(string potionName)
    {
        if (activeQuest == null) return;

        if (activeQuest.type == QuestType.Craft && activeQuest.potionName == potionName)
        {
            activeQuest.currentProgress++;
            Debug.Log($"[{activeQuest.questName}] Progress: {activeQuest.currentProgress}/{activeQuest.craftAmount}");
            UIQuestManager.Instance?.UpdateQuestUI();

            if (activeQuest.currentProgress >= activeQuest.craftAmount)
                CompleteQuest();
        }
    }

    // Kill enemy
    public void UpdateKillProgress(string enemyName)
    {
        if (activeQuest == null) return;

        if (activeQuest.type == QuestType.Kill && activeQuest.enemyName == enemyName)
        {
            activeQuest.currentProgress++;
            Debug.Log($"[{activeQuest.questName}] Progress: {activeQuest.currentProgress}/{activeQuest.targetCount}");

            UIQuestManager.Instance?.UpdateQuestUI();

            if (activeQuest.currentProgress >= activeQuest.targetCount)
                CompleteQuest();
        }
    }

    /// <summary>
    /// Hoàn thành quest hiện tại
    /// </summary>
    public void CompleteQuest()
    {
        if (activeQuest == null) return;

        Debug.Log($"Hoàn thành nhiệm vụ: {activeQuest.questName}");

        // Trao XP
        if (activeQuest.rewardXP > 0)
            PlayerStats.Instance?.AddXP(activeQuest.rewardXP);

        // Trao Item (hỗ trợ nhiều item)
        if (activeQuest.rewardItems != null && activeQuest.rewardItems.Count > 0)
        {
            foreach (var r in activeQuest.rewardItems)
            {
                InventoryManager.Instance?.AddItem(r.item, r.amount);
            }
        }

        activeQuest = null;

        UIQuestManager.Instance?.UpdateQuestUI();
    }

    /// <summary>
    /// Lấy quest hiện tại
    /// </summary>
    public QuestData GetActiveQuest() => activeQuest;
}
