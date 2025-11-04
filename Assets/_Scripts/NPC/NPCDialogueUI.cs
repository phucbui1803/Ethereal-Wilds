using StarterAssets;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogueOption
{
    public string text;                   // Text hiển thị trên button
    public System.Action onSelect;        // Hành động khi chọn
}

public class NPCDialogueUI : MonoBehaviour
{
    public static NPCDialogueUI Instance;

    [Header("UI Panels")]
    public GameObject dialoguePanel;
    public TMP_Text npcNameText;
    public TMP_Text dialogueText;
    public Transform optionsParent;
    public Button optionButtonPrefab;   

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        dialoguePanel.SetActive(false);
    }

    /// <summary>
    /// Mở hội thoại với NPC
    /// </summary>
    public void OpenDialogue(NPCController npc)
    {
        dialoguePanel.SetActive(true);
        npcNameText.text = npc.npcName;

        // Khóa di chuyển và hiện chuột
        if (StarterAssetsInputs.ForceUnlockCursor == false)
            StarterAssetsInputs.ForceUnlockCursor = true;

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerInput = player.GetComponent<StarterAssetsInputs>();
            if (playerInput != null) playerInput.dialogueActive = true;
        }

        string playerName = PlayerStats.Instance != null ? PlayerStats.Instance.characterName : "Kachujin";
        // Step 1: NPC chào dựa theo thời gian
        string greeting = GetGreeting() + " " + playerName + "!";
        ShowOptions(greeting, new List<DialogueOption>()
        {
            new DialogueOption(){ text="Hi", onSelect=()=> GiveQuest(npc) },
            new DialogueOption(){ text="Bye" + " " + npc.npcName + "!", onSelect=()=> CloseDialogue() }
        });
    }

    private string GetGreeting()
    {
        // Lấy giờ từ DayNightCycle
        float hour = DayNightCycle.Instance != null ? DayNightCycle.Instance.GetGameTime() : 12f;

        if (hour < 12f) return "Good morning";
        else if (hour < 18f) return "Good afternoon";
        else return "Good evening";
    }

    /// <summary>
    /// Hiển thị text và các lựa chọn
    /// </summary>
    private void ShowOptions(string text, List<DialogueOption> options)
    {
        dialogueText.text = text;

        // Xoá nút cũ
        foreach (Transform t in optionsParent) Destroy(t.gameObject);

        // Tạo nút mới
        foreach (var option in options)
        {
            Button btn = Instantiate(optionButtonPrefab, optionsParent);
            btn.GetComponentInChildren<TMP_Text>().text = option.text;
            btn.onClick.AddListener(() => option.onSelect.Invoke());
        }
    }

    /// <summary>
    /// Step 2: NPC đưa quest
    /// </summary>
    private void GiveQuest(NPCController npc)
    {
        ShowOptions("I have a quest for you. Do you want to accept it?", new List<DialogueOption>()
        {
            new DialogueOption(){ text="Yes", onSelect=()=> { AcceptQuest(npc); CloseDialogue(); } },
            new DialogueOption(){ text="Bye" + " " + npc.npcName + "!", onSelect=()=> CloseDialogue() }
        });
    }

    private void AcceptQuest(NPCController npc)
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("QuestManager.Instance is null! Hãy thêm QuestManager vào scene.");
            return;
        }

        if (npc.quests != null && npc.quests.Length > 0)
        {
            // Chọn 1 quest ngẫu nhiên
            QuestData questToGive = npc.quests[Random.Range(0, npc.quests.Length)];

            QuestManager.Instance.AddQuest(questToGive);  // Khi này UIQuestManager mới bật panel

            Debug.Log($"Quest nhận: {questToGive.questName}");
        }
        else
        {
            Debug.LogWarning("NPC này chưa có quest để giao.");
        }
    }

    private void CloseDialogue()
    {
        dialoguePanel.SetActive(false);

        // Bật lại di chuyển và khóa chuột
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            var playerInput = player.GetComponent<StarterAssetsInputs>();
            if (playerInput != null) playerInput.dialogueActive = false;
        }

        // Thả chuột và mở lại camera rotation
        StarterAssetsInputs.ForceUnlockCursor = false;
    }
}
