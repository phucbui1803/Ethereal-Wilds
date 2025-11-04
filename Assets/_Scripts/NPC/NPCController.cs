using StarterAssets;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class NPCController : MonoBehaviour, IInteractable
{
    [Header("NPC Info")]
    public string npcName = "Shen";
    public GameObject nameTagPrefab;
    public Transform nameTagAnchor;
    public Canvas worldCanvas;

    [Header("Dialogue & Quest")]
    [TextArea] public string[] dialogues;
    public QuestData[] quests;

    [Header("Idle Settings")]
    public string idleAnimationName = "Idle";
    private Animator animator;

    private NPCNameTag nameTagInstance;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null && !string.IsNullOrEmpty(idleAnimationName))
            animator.Play(idleAnimationName);

        SpawnNameTag();
    }

    void LateUpdate()
    {
        if (nameTagInstance != null && Camera.main != null)
            nameTagInstance.UpdatePositionAndFade(Camera.main);
    }

    private void SpawnNameTag()
    {
        if (nameTagPrefab == null || worldCanvas == null) return;

        if (nameTagAnchor == null)
        {
            GameObject anchor = new GameObject("NameTagAnchor");
            anchor.transform.SetParent(transform);
            anchor.transform.localPosition = new Vector3(0, 2.1f, 0);
            nameTagAnchor = anchor.transform;
        }

        GameObject tagObj = Instantiate(nameTagPrefab, worldCanvas.transform);
        nameTagInstance = tagObj.AddComponent<NPCNameTag>();
        nameTagInstance.Initialize(nameTagAnchor, npcName);
    }

    // -----------------------------
    // 🟢 IInteractable Implementation
    // -----------------------------
    public string GetInteractName() => $"Talk to {npcName}";

    public void Interact()
    {
        // Ẩn panel trước khi mở dialogue
        if (InteractUI.Instance != null)
            InteractUI.Instance.Hide();

        // Gọi UI Dialogue
        if (NPCDialogueUI.Instance != null)
            NPCDialogueUI.Instance.OpenDialogue(this);
    }
}
