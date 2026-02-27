using UnityEngine;

public class BgmManager : MonoBehaviour
{
    public static BgmManager Instance;

    private AudioSource bgmSource;
    public string MainMenuBGM = "mainmenu_bgm"; // Change to match your file
    public string Lvl1BGM = "in_game_bgm"; // Change to match your file
    public string winloseBGM = "winlose_bgm"; // Change to match your file

    public float bgmVolumeScale = 1f;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolumeScale;
        bgmSource.loop = true;

        // Auto-play at startup
        AudioClip clip = Resources.Load<AudioClip>(MainMenuBGM);
        if (clip)
        {
            bgmSource.clip = clip;
            bgmSource.Play();
        }
    }

    public void PlayBGM(string clipName,float bgmVolume, bool restartIfAlreadyPlaying = false)
    {
        if (bgmSource.clip && bgmSource.clip.name == clipName && bgmSource.isPlaying && !restartIfAlreadyPlaying)
            return;
        AudioClip clip = Resources.Load<AudioClip>(clipName);
        if (clip)
        {
            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume = bgmVolume;
            bgmSource.Play();
        }
    }

    public void SetVolume(float vol)
    {
        bgmSource.volume = vol;
        bgmVolumeScale = vol;
        PlayerPrefs.SetFloat("bgmvolume", bgmVolumeScale);

    }
}