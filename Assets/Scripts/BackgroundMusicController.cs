using UnityEngine;

public class BackgroundMusicController : MonoBehaviour
{
    public AudioClip startMusic;         // 开场音乐
    public AudioClip normalGhostMusic;   // 普通敌人音乐

    private AudioSource musicSource;     // 播放音乐的组件
    private float startMusicTime = 0f;   // 开场计时
    private bool alreadyChangeMusic = false; // 有没有切换音乐

    void Start()
    {
        // 找到声音组件
        musicSource = GetComponent<AudioSource>();

        // 设置开场音乐
        musicSource.clip = startMusic;
        musicSource.loop = false;
        musicSource.volume = 0.35f;

        // 播放开场音乐
        musicSource.Play();
    }

    void Update()
    {
        // 还没有切换音乐
        if (alreadyChangeMusic == false)
        {
            // 每帧增加时间
            startMusicTime = startMusicTime + Time.deltaTime;

            // 开场音乐已经播完
            if (musicSource.isPlaying == false)
            {
                alreadyChangeMusic = true;

                musicSource.Stop();
                musicSource.clip = normalGhostMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
            else
            {
                // 开场音乐达到三秒
                if (startMusicTime >= 3f)
                {
                    alreadyChangeMusic = true;

                    musicSource.Stop();
                    musicSource.clip = normalGhostMusic;
                    musicSource.loop = true;
                    musicSource.Play();
                }
            }
        }
    }
}