using UnityEngine;
using UnityEngine.Rendering;

public class Rain : MonoBehaviour
{
    public Transform player;
    public Vector3 offset = new Vector3(0, 50, 0);

    public ParticleSystem rainParticle;

    public float minRainDuration = 38f;
    public float maxRainDuration = 39f;
    public float minRainInterval = 180f;
    public float maxRainInterval = 300f;

    private float rainTimer = 0f;
    private float currentRainDuration = 0f;
    private bool isRaining = false;

    private AudioSource rainAudioSource; // lưu audio loop riêng cho mưa

    private void Start()
    {
        if (rainParticle == null)
        {
            rainParticle = GetComponent<ParticleSystem>();
        }

        if (rainParticle != null)
        {
            rainParticle.Stop();
        }

        rainTimer = Random.Range(minRainInterval, maxRainInterval);
    }

    void Update()
    {
        if (player != null)
        {
            transform.position = player.position + offset;
        }

        if (!isRaining)
        {
            rainTimer -= Time.deltaTime;

            if (rainTimer <= 0f)
            {
                StartRain();
            }
        }
        else
        {
            currentRainDuration -= Time.deltaTime;

            if (currentRainDuration <= 0f)
            {
                StopRain();
            }
        }
    }

    private void StartRain()
    {
        isRaining = true;
        currentRainDuration = Random.Range(minRainDuration, maxRainDuration);

        if (rainParticle != null)
        {
            rainParticle.Play();
        }

        // Phát âm thanh loop mưa
        if (AudioManager.Instance != null)
        {
            // Nếu đang có âm thanh mưa cũ thì thôi
            if (rainAudioSource == null)
            {
                rainAudioSource = AudioManager.Instance.PlaySFXLoop("Rain");
            }
        }
    }

    private void StopRain()
    {
        isRaining = false;

        if (rainParticle != null)
        {
            rainParticle.Stop();
        }

        // Tắt âm thanh mưa
        if (rainAudioSource != null)
        {
            rainAudioSource.Stop();
            Destroy(rainAudioSource.gameObject); // xóa object chứa source
            rainAudioSource = null;
        }

        rainTimer = Random.Range(minRainInterval, maxRainInterval);
    }
}
