using UnityEngine;

[RequireComponent(typeof(WindZone))]
public class WindSoundController : MonoBehaviour
{
    public string windSoundName = "Wind"; // tên sound trong AudioManager
    private AudioSource windSource;
    private WindZone windZone;

    void Start()
    {
        windZone = GetComponent<WindZone>();

        if (AudioManager.Instance != null)
        {
            windSource = AudioManager.Instance.PlaySFXLoop(windSoundName);
        }
    }

    void Update()
    {
        if (windSource != null && windZone != null)
        {
            float targetVolume = Mathf.Clamp01(windZone.windMain / 1f) * AudioManager.Instance.sfxSource.volume;
            windSource.volume = Mathf.Lerp(windSource.volume, targetVolume, Time.deltaTime * 5f);
        }
    }

    void OnDestroy()
    {
        if (windSource != null)
            Destroy(windSource.gameObject);
    }
}
