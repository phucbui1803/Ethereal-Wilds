using UnityEngine;
using System.Collections;

public class TorchController : MonoBehaviour
{
    [Header("Torch Components")]
    public Light pointLight;                     // Ánh sáng đuốc
    public ParticleSystem[] fireParticles;       // fx_fire, fx_smoke, fx_sparks...

    [Header("Settings")]
    public float lightFadeSpeed = 1f;            // Tốc độ tăng giảm cường độ ánh sáng
    public float nightStartTime = 18f;           // Bắt đầu đêm
    public float dayStartTime = 6f;              // Bắt đầu ngày
    public float maxLightIntensity = 2f;         // Cường độ sáng tối đa

    private bool isActive = false;               // Đuốc đang bật hay tắt
    private Coroutine fadeRoutine;

    void Start()
    {
        // Nếu chưa gán thì tự tìm các thành phần con
        if (pointLight == null)
            pointLight = GetComponentInChildren<Light>();

        if (fireParticles == null || fireParticles.Length == 0)
            fireParticles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update()
    {
        // Lấy giờ hiện tại từ DayNightCycle
        DayNightCycle cycle = FindFirstObjectByType<DayNightCycle>();
        if (cycle == null) return;

        float timeOfDay = cycle.timeOfDay;
        bool isNight = timeOfDay >= nightStartTime || timeOfDay < dayStartTime;

        // Nếu trạng thái thay đổi thì xử lý bật/tắt
        if (isNight && !isActive)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeInTorch());
        }
        else if (!isNight && isActive)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeOutTorch());
        }
    }

    IEnumerator FadeInTorch()
    {
        isActive = true;

        // Bật các particle
        foreach (var ps in fireParticles)
            if (!ps.isPlaying) ps.Play();

        // Tăng dần độ sáng
        if (pointLight != null)
        {
            pointLight.enabled = true; // Bật lại khi bắt đầu fade in
            float startIntensity = 0f;
            float targetIntensity = maxLightIntensity;
            pointLight.intensity = 0f;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * lightFadeSpeed;
                pointLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, t);
                yield return null;
            }
        }
    }

    IEnumerator FadeOutTorch()
    {
        isActive = false;

        // Giảm dần độ sáng
        if (pointLight != null)
        {
            float startIntensity = pointLight.intensity;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * lightFadeSpeed;
                pointLight.intensity = Mathf.Lerp(startIntensity, 0f, t);
                yield return null;
            }
            pointLight.intensity = 0f;
            pointLight.enabled = false; // Tắt component sau khi fade out
        }

        // Tắt các particle
        foreach (var ps in fireParticles)
            if (ps.isPlaying) ps.Stop();
    }
}
