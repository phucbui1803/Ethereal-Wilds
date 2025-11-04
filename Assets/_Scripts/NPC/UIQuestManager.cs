using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIQuestManager : MonoBehaviour
{
    public static UIQuestManager Instance;

    [Header("UI References")]
    public GameObject questPanel;
    public TMP_Text questListText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Khởi đầu ẩn panel
        if (questPanel != null)
            questPanel.SetActive(false);
    }

    /// <summary>
    /// Cập nhật UI hiển thị quest hiện tại
    /// </summary>
    public void UpdateQuestUI()
    {
        var activeQuest = QuestManager.Instance.GetActiveQuest(); // Chỉ 1 quest duy nhất

        if (activeQuest == null)
        {
            // Ẩn panel khi không có quest
            if (questPanel != null)
                questPanel.SetActive(false);
            return;
        }

        // Chỉ bật panel khi thực sự có quest
        if (!questPanel.activeSelf)
            questPanel.SetActive(true);

        string info = $"<b>Quest: {activeQuest.questName}</b>\n";
        info += $"NPC: {activeQuest.npcName}\n";
        info += $"Desc: {activeQuest.description}\n";
        info += $"Progress: {activeQuest.currentProgress}/{GetQuestTargetAmount(activeQuest)}\n";

        // Thông tin phần thưởng
        string rewardInfo = $"Reward: XP +{activeQuest.rewardXP}";

        if (activeQuest.rewardItems != null && activeQuest.rewardItems.Count > 0)
        {
            foreach (var r in activeQuest.rewardItems)
            {
                rewardInfo += $", {r.item.itemName} x{r.amount}";
            }
        }

        info += rewardInfo;
        questListText.text = info;
    }

    /// <summary>
    /// Trả về số lượng mục tiêu của quest tuỳ loại
    /// </summary>
    private int GetQuestTargetAmount(QuestData quest)
    {
        switch (quest.type)
        {
            case QuestType.Collect:
                return quest.collectAmount;
            case QuestType.Kill:
                return quest.targetCount;
            case QuestType.Craft:
                return quest.craftAmount;
            case QuestType.Talk:
                return 1;
            default:
                return 0;
        }
    }

    /// <summary>
    /// Xoá UI khi quest hoàn thành
    /// </summary>
    public void ClearQuestUI()
    {
        if (questPanel != null)
            questPanel.SetActive(false);
    }
}
