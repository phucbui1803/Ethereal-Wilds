using UnityEngine;
using UnityEngine.Rendering;

public class Fireball : MonoBehaviour
{
    public enum FireballMode { Forward, Downward } // thêm enum để chọn chế độ
    public FireballMode mode = FireballMode.Forward;

    [Header("Stats")]
    [SerializeField] private float speed = 20f; // tốc độ bắn thẳng
    [SerializeField] private float gravity = 20f; // tốc độ rơi (Skill R)
    private float damage;
    private int playerLevel;

    [Header("Prefab")]
    public GameObject explosionPrefab;
    public GameObject hitboxEPrefab;
    public GameObject hitboxRPrefab;

    private Rigidbody rb;
    private bool hasHit = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (mode == FireballMode.Forward)
            {
                rb.linearVelocity = transform.forward * speed;
            }
        }

        // Phát âm thanh fireball 3D
        Play3DSound("Fireball", transform.position);

        // Destroy tự động sau 5s nếu không va chạm
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (mode == FireballMode.Downward && !hasHit)
        {
            // Rơi thẳng xuống (Skill R)
            transform.position += Vector3.down * gravity * Time.deltaTime;
        }
    }

    public void SetDamage(float value)
    {
        damage = value;
    }

    public void SetPlayerLevel(int level)
    {
        playerLevel = level;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
        {
            return; // tránh double hit
        }

        hasHit = true;

        // Spawn VFX nổ
        SpawnExplosion(transform.position);

        // Nếu là skill E, chỉ spawn hitbox tại vị trí va chạm
        if (mode == FireballMode.Forward && hitboxEPrefab != null)
        {
            // Gọi PlayerSkills để spawn hitbox E
            PlayerSkills playerSkills = FindFirstObjectByType<PlayerSkills>();
            if (playerSkills != null)
            {   
                playerSkills.SpawnHitboxForSkillE(transform.position);
            }
        }

        // Nếu là skill R, chỉ spawn hitbox tại vị trí cố định
        if (mode == FireballMode.Downward)
        {
            // Gọi PlayerSkills để spawn hitbox R tại vị trí cố định
            PlayerSkills playerSkills = FindFirstObjectByType<PlayerSkills>();
            if (playerSkills != null)
            {
                playerSkills.SpawnHitboxForSkillR();
            }
        }

        Destroy(gameObject);
    }

    private void SpawnExplosion(Vector3 position)
    {
        if (explosionPrefab == null)
        {
            return;
        }

        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);

        // Phát âm thanh explosion 3D
        Play3DSound("Explosion", position);

        ParticleSystem ps = explosion.GetComponent<ParticleSystem>();

        if (ps != null)
        {
            Destroy(explosion, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(explosion, 2f);
        }
    }

    private void Play3DSound(string name, Vector3 position)
    {
        if (AudioManager.Instance == null) return;

        // Gọi AudioManager phát 3D sound
        AudioManager.Instance.PlaySFX(name, position);
    }
}
