using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    public ItemData item;   // Item thưởng
    public int amount = 1;  // Số lượng
}

public enum QuestType { Kill, Collect, Craft, Talk }

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quest System/Quest")]
public class QuestData : ScriptableObject
{
    [Header("Basic Info")]
    public string questName;
    [TextArea] public string description;
    public QuestType type;

    [Header("Assigned NPC")]
    public string npcName;

    [Header("Kill Quest")]
    public string enemyName;
    public int targetCount;

    [Header("Collect Quest")]
    public string itemName;
    public int collectAmount;

    [Header("Craft Quest")]
    public string potionName;
    public int craftAmount;

    [Header("Reward")]
    public int rewardXP;
    public List<QuestReward> rewardItems = new List<QuestReward>();

    [HideInInspector] public int currentProgress = 0; // tự động update
}
