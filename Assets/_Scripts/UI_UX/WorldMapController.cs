using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class WorldMapController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IScrollHandler
{
    [Header("UI References")]
    public RawImage worldMapImage;
    public Button leftButton;
    public Button rightButton;
    public Button acceptButton;
    public TMP_Text timeText;

    [Header("Camera Settings")]
    public Camera minimapCamera;
    public float minZoom = 20f;
    public float maxZoom = 200f;
    public float zoomSpeed = 2f;
    public float panSpeed = 1f;
    public float focusSmoothTime = 0.4f;

    [Header("Game Time Settings")]
    public float currentHour = 0f;
    public float dayLength = 24f;
    private float pendingHour = 0f;

    [Header("World Bounds")]
    public float mapSize = 1000f;
    public float panLimit = 900f;

    private bool isDragging = false;
    private Vector2 lastMousePos;
    private Vector3 cameraVelocity = Vector3.zero;

    public static WorldMapController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        leftButton?.onClick.AddListener(() => ChangePendingTime(-1));
        rightButton?.onClick.AddListener(() => ChangePendingTime(1));
        acceptButton?.onClick.AddListener(AcceptTimeChange);

        SyncTimeFromWorld();
        UpdateTimeText();
    }

    private void ChangePendingTime(int delta)
    {
        pendingHour += delta;
        if (pendingHour < 0) pendingHour = dayLength - 1;
        else if (pendingHour >= dayLength) pendingHour = 0;
        UpdateTimeText();
        PlayButtonClickSound();
    }

    private void AcceptTimeChange()
    {
        currentHour = pendingHour;
        UpdateTimeText();
        if (DayNightCycle.Instance != null)
            DayNightCycle.Instance.SetGameTime(currentHour);
        PlayButtonClickSound();
    }

    private void UpdateTimeText()
    {
        int hour = Mathf.FloorToInt(pendingHour);
        if (timeText != null) timeText.text = $"{hour:00}:00";
    }

    public void SyncTimeFromWorld()
    {
        if (DayNightCycle.Instance != null)
        {
            currentHour = DayNightCycle.Instance.GetGameTime();
            pendingHour = currentHour;
        }
        UpdateTimeText();
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (minimapCamera == null) return;

        float newSize = minimapCamera.orthographicSize - eventData.scrollDelta.y * zoomSpeed;
        minimapCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        lastMousePos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if ((eventData.position - lastMousePos).sqrMagnitude < 10f)
            FocusMapOnClick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || minimapCamera == null) return;

        Vector2 delta = eventData.delta;
        Vector3 move = new Vector3(-delta.x, 0, -delta.y) * (panSpeed * Time.unscaledDeltaTime * minimapCamera.orthographicSize);
        minimapCamera.transform.position += move;
        ClampCameraPosition();
    }

    private void FocusMapOnClick(PointerEventData eventData)
    {
        if (worldMapImage == null || minimapCamera == null) return;

        // Nếu canvas overlay -> truyền null, nếu screen space camera -> truyền canvas camera
        Camera cam = eventData.pressEventCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(worldMapImage.rectTransform, eventData.position, cam, out Vector2 localPoint))
            return;

        Rect rect = worldMapImage.rectTransform.rect;

        // Chuẩn hóa 0..1 dựa trên pivot center
        float normalizedX = (localPoint.x / rect.width) + 0.5f;
        float normalizedY = (localPoint.y / rect.height) + 0.5f;

        // Tính vị trí world
        Vector3 targetPos = new Vector3(
            (normalizedX - 0.5f) * mapSize,
            minimapCamera.transform.position.y,
            (normalizedY - 0.5f) * mapSize
        );

        targetPos.x = Mathf.Clamp(targetPos.x, -panLimit / 2f, panLimit / 2f);
        targetPos.z = Mathf.Clamp(targetPos.z, -panLimit / 2f, panLimit / 2f);

        StopAllCoroutines();
        StartCoroutine(SmoothFocus(targetPos));

    }

    private IEnumerator SmoothFocus(Vector3 targetPos)
    {
        while (Vector3.Distance(minimapCamera.transform.position, targetPos) > 0.1f)
        {
            minimapCamera.transform.position = Vector3.SmoothDamp(minimapCamera.transform.position, targetPos, ref cameraVelocity, focusSmoothTime);
            yield return null;
        }
        minimapCamera.transform.position = targetPos;
    }

    private void ClampCameraPosition()
    {
        if (minimapCamera == null) return;

        Vector3 pos = minimapCamera.transform.position;
        float halfLimit = panLimit / 2f;
        pos.x = Mathf.Clamp(pos.x, -halfLimit, halfLimit);
        pos.z = Mathf.Clamp(pos.z, -halfLimit, halfLimit);
        minimapCamera.transform.position = pos;
    }

    private void PlayButtonClickSound()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX("Click");
    }
}
