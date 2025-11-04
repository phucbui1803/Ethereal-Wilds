using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneMusicController : MonoBehaviour
{
    [Header("List Music In Scene")]
    public List<string> musicNames; // tên các bài có trong AudioManager

    private int currentIndex = 0;
    private bool isPausedByFocus = false;

    private Coroutine musicRoutine;

    void Start()
    {
        // Dừng nhạc trước đó nếu có
        AudioManager.Instance.musicSource.Stop();

        if (musicNames.Count == 0)
        {
            Debug.LogWarning($"SceneMusicController ({SceneManager.GetActiveScene().name}): Chưa có bài nhạc nào!");
            return;
        }

        // Nếu chỉ có 1 bài
        if (musicNames.Count == 1)
        {
            PlaySingleMusic();
        }
        else
        {
            PlayRandomMusic();
        }
    }

    void PlaySingleMusic()
    {
        AudioManager.Instance.PlayMusic(musicNames[0]);
    }

    void PlayRandomMusic()
    {
        currentIndex = Random.Range(0, musicNames.Count);
        AudioManager.Instance.PlayMusic(musicNames[currentIndex]);

        if (musicRoutine != null)
            StopCoroutine(musicRoutine);

        musicRoutine = StartCoroutine(PlayNextWhenFinished());
    }

    IEnumerator PlayNextWhenFinished()
    {
        var source = AudioManager.Instance.musicSource;

        while (true)
        {
            // ⚠️ Chỉ kiểm tra khi không bị pause bởi focus
            if (!isPausedByFocus && source != null && !source.isPlaying)
            {
                // chỉ đổi bài nếu clip thực sự đã kết thúc
                if (source.time == 0f || Mathf.Approximately(source.time, 0f))
                {
                    currentIndex = (currentIndex + 1) % musicNames.Count;
                    AudioManager.Instance.PlayMusic(musicNames[currentIndex]);
                }
            }
            yield return null;
        }
    }

    // Lưu vị trí phát nhạc để resume lại đúng chỗ
    private float resumeTime = 0f;

    private void OnApplicationFocus(bool hasFocus)
    {
        HandleFocusOrPause(!hasFocus);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        HandleFocusOrPause(pauseStatus);
    }

    private void HandleFocusOrPause(bool isPaused)
    {
        var source = AudioManager.Instance.musicSource;
        if (source == null) return;

        if (isPaused)
        {
            if (source.isPlaying)
            {
                resumeTime = source.time; // lưu lại vị trí
                source.Pause();
                isPausedByFocus = true;
            }
        }
        else
        {
            if (isPausedByFocus)
            {
                // Resume lại đúng vị trí trước khi mất focus
                source.time = resumeTime;
                source.UnPause();
                isPausedByFocus = false;
            }
        }
    }
}
