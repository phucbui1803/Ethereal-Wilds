using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun & Moon Objects (visuals only)")]
    public Transform sun;   // Sphere + particle
    public Transform moon;  // Sphere only

    [Header("Directional Light")]
    public Light sunLight;  // 1 directional light chính

    [Header("Terrain / Center Point")]
    public Transform worldCenter; // Empty ở giữa map

    [Header("Time Settings")]
    [Range(0, 24)] public float timeOfDay = 0f;
    public float dayDuration = 7200f;
    public float orbitRadius = 600f;             // bán kính quỹ đạo

    [Header("Lighting Settings")]
    public Gradient sunColorGradient;           // màu ánh sáng theo giờ
    public AnimationCurve sunIntensityCurve;    // cường độ ánh sáng theo giờ

    public Material skyboxMaterial;
    public AnimationCurve skyExposureCurve; // 0-1 => ánh sáng theo thời gian

    public float rotationSpeed = 1f; // độ / giây

    public static DayNightCycle Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        // Cập nhật thời gian tự động (nếu game đang chạy)
        timeOfDay += (24f / dayDuration) * Time.deltaTime;
        if (timeOfDay >= 24f) timeOfDay -= 24f;

        UpdateLighting(); // cập nhật ánh sáng & vị trí
    }

    public void UpdateLighting()
    {
        float t = timeOfDay / 24f; // 0–1 theo 24h

        // --- Góc quỹ đạo của mặt trời ---
        float sunAngle = (t - 0.25f) * 360f;

        // --- Vị trí mặt trời ---
        if (sun != null && worldCenter != null)
        {
            Vector3 offset = Quaternion.Euler(0, 0, sunAngle) * Vector3.right * orbitRadius;
            sun.position = worldCenter.position + new Vector3(offset.x, offset.y, 0f);
        }

        // --- Vị trí mặt trăng ---
        if (moon != null && worldCenter != null)
        {
            float moonAngle = sunAngle + 180f;
            Vector3 offsetMoon = Quaternion.Euler(0, 0, moonAngle) * Vector3.right * orbitRadius;
            moon.position = worldCenter.position + new Vector3(offsetMoon.x, offsetMoon.y, 0f);
        }

        // --- Cập nhật ánh sáng ---
        if (sunLight != null && sun != null)
        {
            sunLight.transform.rotation = Quaternion.LookRotation(worldCenter.position - sun.position);
            sunLight.color = sunColorGradient.Evaluate(t);
            sunLight.intensity = sunIntensityCurve.Evaluate(t);
        }

        UpdateSkyboxExposure();
        UpdateSkyboxRotation();
    }

    private void UpdateSkyboxExposure()
    {
        if (skyboxMaterial == null || skyExposureCurve == null) return;

        float t = timeOfDay / 24f;
        float exposure = skyExposureCurve.Evaluate(t);
        skyboxMaterial.SetFloat("_Exposure", exposure);
    }

    private void UpdateSkyboxRotation()
    {
        if (skyboxMaterial == null) return;

        float currentRotation = skyboxMaterial.GetFloat("_Rotation");
        currentRotation += rotationSpeed * Time.deltaTime;
        if (currentRotation >= 360f) currentRotation -= 360f;
        skyboxMaterial.SetFloat("_Rotation", currentRotation);
    }

    // ================== Điều khiển thời gian từ bên ngoài ==================
    public void SetGameTime(float newHour)
    {
        timeOfDay = Mathf.Clamp(newHour, 0f, 24f);
        UpdateLighting();
    }

    public float GetGameTime()
    {
        return timeOfDay;
    }
}
