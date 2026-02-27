using UnityEngine;
using System.Collections.Generic;

public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    [System.Serializable]
    public class SfxClip
    {
        public string id;
        public AudioClip clip;
        public float volume;
    }

    [Header("SFX List")]
    public List<SfxClip> sfxClips;

    [Header("Pool Settings")]
    public int initialPoolSize = 10;
    public AudioSource audioSourcePrefab;

    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();
    private List<AudioSource> audioPool = new List<AudioSource>();

    [Header("Volume")]
    [Range(0f, 4f)]
    public float sfxVolumeScale = 1f;

    //---- Singleton Setup ----
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            foreach (var entry in sfxClips)
                sfxDict[entry.id] = entry.clip;

            sfxVolumeScale = PlayerPrefs.GetFloat("sfxvolume", 1f);

            // Pool Setup
            for (int i = 0; i < initialPoolSize; i++)
                audioPool.Add(Instantiate(audioSourcePrefab, transform));
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //---- Core Play Method ----
    public void PlaySfx(string id, float? volumeOverride = null, float? pitchOverride = null)
    {
        if (sfxDict.TryGetValue(id, out AudioClip clip))
        {
            SfxClip found = sfxClips.Find(x => x.id == id);

            
            AudioSource freeSource = GetAvailableSource();
            freeSource.clip   = clip;  // IMPORTANT: assign clip
            freeSource.volume = volumeOverride ?? found.volume * sfxVolumeScale;
            freeSource.pitch = pitchOverride ?? Random.Range(0.95f, 1.05f); // Slight random pitch for variety
            freeSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SFX '{id}' not found!");
        }
    }
    
    public bool IsSfxPlaying(string id)
    {
        AudioClip clip;
        if (!sfxDict.TryGetValue(id, out clip))
            return false;
        foreach (var src in audioPool)
        {
            if (src.isPlaying && src.clip == clip)
                return true;
        }
        return false;
    }
    
    public void StopSfx(string id)
    {
        // Look up the clip for this id
        if (!sfxDict.TryGetValue(id, out AudioClip clip))
        {
            Debug.LogWarning($"SFX '{id}' not found for StopSfx!");
            return;
        }

        // Stop all AudioSources currently playing this clip
        foreach (var src in audioPool)
        {
            // For PlayOneShot, src.clip is still the assigned clip on the AudioSource
            // If you use dedicated sources per id, this check is fine
            if (src.isPlaying && src.clip == clip)
            {
                src.Stop();
            }
        }
    }


    public void SetVolume(float vol)
    {
        sfxVolumeScale = vol;
        PlayerPrefs.SetFloat("sfxvolume", sfxVolumeScale);

    }
    

    //---- Simple AudioSource Pool ----
    private AudioSource GetAvailableSource()
    {
        foreach (var src in audioPool)
            if (!src.isPlaying) return src;

        // If all are playing, optionally instantiate a new one
        AudioSource newSrc = Instantiate(audioSourcePrefab, transform);
        audioPool.Add(newSrc);
        return newSrc;
    }
}
