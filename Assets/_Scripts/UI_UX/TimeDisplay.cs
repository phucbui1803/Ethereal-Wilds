using UnityEngine;
using TMPro;  // nếu bạn dùng TextMeshPro

public class TimeDisplay : MonoBehaviour
{
    public DayNightCycle dayNightCycle; // tham chiếu tới script DayNightCycle
    public TextMeshProUGUI timeText;    // Text UI hiển thị thời gian

    void Update()
    {
        if (dayNightCycle == null || timeText == null) return;

        // Lấy timeOfDay từ DayNightCycle
        float time = dayNightCycle.timeOfDay;

        // Chuyển sang giờ và phút
        int hours = Mathf.FloorToInt(time);
        int minutes = Mathf.FloorToInt((time - hours) * 60f);

        // Định dạng chuỗi "HH:MM"
        string formattedTime = string.Format("{0:00}:{1:00}", hours, minutes);

        // Cập nhật lên UI
        timeText.text = formattedTime;
    }
}
