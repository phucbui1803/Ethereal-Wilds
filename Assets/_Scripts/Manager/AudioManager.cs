using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;          // Tên sound, dùng để gọi
    public AudioClip clip;       // Clip âm thanh
    [Range(0f, 1f)]
    public float volume = 1f;    // Âm lượng mặc định
    [Range(0.1f, 3f)]
    public float pitch = 1f;     // Pitch
    public bool loop = false;    // Có lặp lại không
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource; // BGM
    public AudioSource sfxSource;   // SFX ngắn (1-shot)

    [Header("Danh sách âm thanh")]
    public List<Sound> sounds = new List<Sound>();

    private Dictionary<string, Sound> soundDict = new Dictionary<string, Sound>();

    private Dictionary<AudioSource, float> loopBaseVolume = new Dictionary<AudioSource, float>();

    private List<AudioSource> loopSources = new List<AudioSource>(); // lưu các SFX loop đang phát

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tạo dictionary từ list
        foreach (var s in sounds)
        {
            if (s != null && !string.IsNullOrEmpty(s.name))
            {
                if (!soundDict.ContainsKey(s.name))
                    soundDict.Add(s.name, s);
                else
                    Debug.LogWarning($"AudioManager: Sound name '{s.name}' bị trùng!");
            }
        }

        // Load volume
        musicSource.volume = PlayerPrefs.GetFloat("Music Volume", 1f);
        sfxSource.volume = PlayerPrefs.GetFloat("SFX Volume", 1f);
    }

    #region Music
    public void PlayMusic(string name)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning($"AudioManager: Không tìm thấy music '{name}'");
            return;
        }

        // Nếu cùng clip đang chạy → không restart
        if (musicSource.isPlaying && musicSource.clip == s.clip)
            return;

        musicSource.clip = s.clip;
        musicSource.volume = s.volume * PlayerPrefs.GetFloat("Music Volume", 1f);
        musicSource.pitch = s.pitch;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SetMusicVolume(float value)
    {
        musicSource.volume = value;
        PlayerPrefs.SetFloat("Music Volume", value);
    }
    #endregion

    #region SFX
    // 2D / 3D SFX ngắn
    public void PlaySFX(string name, Vector3? position = null)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning($"AudioManager: Không tìm thấy SFX '{name}'");
            return;
        }

        if (position == null)
        {
            // 2D
            sfxSource.PlayOneShot(s.clip, s.volume * sfxSource.volume);
        }
        else
        {
            // 3D
            GameObject obj = new GameObject($"SFX_{s.name}");
            obj.transform.position = position.Value;
            AudioSource source = obj.AddComponent<AudioSource>();
            source.clip = s.clip;
            source.volume = s.volume * sfxSource.volume;
            source.pitch = s.pitch;
            source.loop = s.loop;
            source.spatialBlend = 1f;
            source.minDistance = 10f;
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
            source.Play();

            if (!s.loop)
                Destroy(obj, s.clip.length);
        }
    }

    // SFX loop (ví dụ gió)
    public AudioSource PlaySFXLoop(string name, Vector3? position = null)
    {
        if (!soundDict.TryGetValue(name, out Sound s))
        {
            Debug.LogWarning($"AudioManager: Không tìm thấy SFX '{name}'");
            return null;
        }

        GameObject obj = new GameObject($"SFXLoop_{s.name}");
        obj.transform.position = position ?? Vector3.zero;

        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = s.clip;
        source.volume = s.volume * sfxSource.volume; // tự động nhân với slider
        source.pitch = s.pitch;
        source.loop = true;

        if (position.HasValue)
        {
            source.spatialBlend = 1f; // 3D
            source.minDistance = 5f;
            source.maxDistance = 50f;
            source.rolloffMode = AudioRolloffMode.Logarithmic;
        }
        else
        {
            source.spatialBlend = 0f; // 2D
        }

        source.Play();
        loopSources.Add(source); // lưu lại để update volume realtime
        loopBaseVolume[source] = s.volume; // lưu volume gốc của clip

        return source;
    }

    public void SetSFXVolume(float value)
    {
        sfxSource.volume = value;
        PlayerPrefs.SetFloat("SFX Volume", value);

        // cập nhật tất cả loop SFX đang phát
        foreach (var src in loopSources)
        {
            if (src != null && loopBaseVolume.ContainsKey(src))
                src.volume = loopBaseVolume[src] * value;
        }
    }
    #endregion

    #region Stop
    public void StopAllSounds()
    {
        foreach (var src in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            src.Stop();
            Destroy(src.gameObject);
        }
    }

    public void StopSound(string name)
    {
        foreach (var src in Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None))
        {
            if (src.clip != null && src.clip.name == name)
            {
                src.Stop();
                Destroy(src.gameObject);
            }
        }
    }

    public void StopMusic()
    {
        if (musicSource.isPlaying)
            musicSource.Stop();
    }
    #endregion
}
