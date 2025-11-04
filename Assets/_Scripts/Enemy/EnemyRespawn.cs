using UnityEngine;
using System;
using System.Collections;

public class EnemyRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public float respawnDelay = 10f;

    [Header("References")]
    public Animator animator;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private bool isDead = false;
    private Action onRespawnCallback;

    private Renderer[] renderers;
    private Collider[] colliders;

    private void Awake()
    {
        // Lưu tất cả renderer và collider con để bật/tắt
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    /// <summary>
    /// Gọi khi enemy chết. callback dùng để reset EnemyStats (HP, level, HP bar)
    /// </summary>
    public void Die(Action callback = null)
    {
        if (isDead) return;
        isDead = true;

        onRespawnCallback = callback;

        // Tắt collider để player không tương tác
        SetColliders(false);

        // Trigger anim Die
        if (animator != null)
            animator.SetTrigger("Die");

        // Bắt đầu respawn coroutine
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // Chờ anim Die kết thúc (mặc định 2-3s)
        float dieAnimLength = 2f;
        if (animator != null)
        {
            AnimatorClipInfo[] clips = animator.GetCurrentAnimatorClipInfo(0);
            if (clips.Length > 0)
                dieAnimLength = clips[0].clip.length;
        }
        yield return new WaitForSeconds(dieAnimLength);

        // Chỉ ẩn renderer, giữ CharacterController active
        SetRenderers(false);

        // Chờ respawnDelay
        yield return new WaitForSeconds(respawnDelay);

        // Reset vị trí + rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;
        isDead = false;

        // Hiện lại renderer
        SetRenderers(true);

        // Reset animator về Idle
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // Bật lại collider
        SetColliders(true);

        // Callback reset EnemyStats
        onRespawnCallback?.Invoke();
    }

    private void SetRenderers(bool active)
    {
        foreach (var r in renderers)
            r.enabled = active;
    }

    private void SetColliders(bool active)
    {
        foreach (var c in colliders)
            c.enabled = active;
    }
}
