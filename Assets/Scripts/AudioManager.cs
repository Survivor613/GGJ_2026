using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

    [SerializeField] private string bgmParameter;
    [SerializeField] private string sfxParameter;
    public float currentBgmValue { get; private set; } // used for initializing slider
    public float currentSfxValue { get; private set; } // used for initializing slider

    [Header("Audio Database")]
    [SerializeField] private AudioDatabaseSO audioDB;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource; // AudioManager 自己的 sfxSource, 用于全局SFX(如UI)

    [Header("BGM")]
    private AudioClip lastMusicPlayed;
    private string currentBgmGroupName;
    private Coroutine currentBgmCo;
    [SerializeField] private bool bgmShouldPlay;

    private Coroutine playBGMCo;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject, 0.01f);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize Sound Value
        currentBgmValue = 0.2f;
        currentSfxValue = 0.6f;
        SetBGMVolume(currentBgmValue);
        SetSFXVolume(currentSfxValue);
    }

    private void Update()
    {
        if (bgmSource.isPlaying == false && bgmShouldPlay) // bgm 播放完后 isPlay 变为 false，通过 Update()继续播放
        {
            if (string.IsNullOrEmpty(currentBgmGroupName) == false)
                NextBGM(currentBgmGroupName);
        }

        if (bgmSource.isPlaying && bgmShouldPlay == false)
            StopBGM();
    }

    public void PlayBGM(string musicGroup)
    {
        if (playBGMCo != null)
            StopCoroutine(playBGMCo);

        playBGMCo = StartCoroutine(PlayBGMCo(musicGroup));
    }

    private IEnumerator PlayBGMCo(string musicGroup)
    {
        yield return new WaitForSeconds(1f);
        AudioManager.instance.StartBGM(musicGroup);
    }

    public void StartBGM(string musicGroup)   // 在 musicGroup 中随机播放一首音乐
    {
        bgmShouldPlay = true;

        //if (musicGroup == currentBgmGroupName)
        //    return;

        NextBGM(musicGroup);
    }

    public void NextBGM(string musicGroup)   // 在 musicGroup 中换一首音乐
    {
        bgmShouldPlay = true;
        currentBgmGroupName = musicGroup;

        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);

        currentBgmCo = StartCoroutine(SwitchMusicCo(musicGroup));
    }

    public void StopBGM() // 停止音乐
    {
        bgmShouldPlay = false;

        StartCoroutine(FadeVolumeCo(bgmSource, 0, 1));

        if (currentBgmCo != null)
            StopCoroutine(currentBgmCo);
    }

    private IEnumerator SwitchMusicCo(string musicGroup)
    {
        AudioClipData data = audioDB.Get(musicGroup);
        AudioClip nextMusic = data.GetRandomClip();

        if (data == null || data.clips.Count == 0)
        {
            Debug.Log("No audio found for group" + musicGroup);
            yield break;
        }

        if (data.clips.Count > 1)
        {
            while (nextMusic == lastMusicPlayed)
                nextMusic = data.GetRandomClip();
        }

        if (bgmSource.isPlaying)
            yield return FadeVolumeCo(bgmSource, 0, 1);

        lastMusicPlayed = nextMusic;
        bgmSource.clip = nextMusic;
        bgmSource.volume = 0;
        bgmSource.Play();

        StartCoroutine(FadeVolumeCo(bgmSource, 1, 1));
    }


    private IEnumerator FadeVolumeCo(AudioSource source, float targetVolume, float duration)
    {
        float time = 0;
        float startVolume = source.volume;

        while (time < duration)
        {
            time += Time.deltaTime;

            source.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    public void PlaySFX(string soundName, AudioSource sfxSource) // Entity 传入的 sfxSource, 用于局部SFX
    {
        //Debug.Log("Try to get " + soundName);
        var data = audioDB.Get(soundName);
        if (data == null)
        {
            Debug.Log("Attempt to play sound - " + soundName);
            return;
        }

        var clip = data.GetRandomClip();
        if (clip == null) return;

        sfxSource.pitch = UnityEngine.Random.Range(.95f, 1.1f);
        sfxSource.volume = data.volume;
        sfxSource.PlayOneShot(clip); // 播放一次
    }

    public void PlayGlobalSFX(string soundName)
    {
        var data = audioDB.Get(soundName);
        if (data == null) return;

        var clip = data.GetRandomClip();
        if (clip == null) return;

        //Debug.Log("Played audio" + clip.name);
        sfxSource.pitch = UnityEngine.Random.Range(.95f, 1.1f);
        sfxSource.volume = data.volume;
        sfxSource.PlayOneShot(clip);
    }

    public void SetBGMVolume(float value)
    {
        currentBgmValue = value;

        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
        Debug.Log("SetBGMVolume 完毕!");
    }

    public void SetSFXVolume(float value)
    {
        currentSfxValue = value;

        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
    }

    private void OnEnable()
    {
        // 订阅事件
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 取消订阅
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 必须写这两个参数，才能匹配 SceneManager 的要求
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterAllButtons();
    }

    public void RegisterAllButtons()
    {
        // 1. 获取当前活动的场景
        Scene currentScene = SceneManager.GetActiveScene();

        // 2. 获取该场景所有的根物体 (Root GameObjects)
        GameObject[] rootObjects = currentScene.GetRootGameObjects();

        // 3. 遍历每个根物体，寻找其下方所有的 Button（包括隐藏的）
        List<Button> buttonsInScene = new List<Button>();
        foreach (GameObject root in rootObjects)
        {
            // GetComponentsInChildren<T>(true) 这里的 true 表示包含隐藏物体
            Button[] btns = root.GetComponentsInChildren<Button>(true);
            buttonsInScene.AddRange(btns);
        }

        // 4. 执行注册逻辑
        foreach (var btn in buttonsInScene)
        {
            btn.onClick.RemoveListener(PlayDefaultBtnSound);
            btn.onClick.AddListener(PlayDefaultBtnSound);
        }
    }

    private void PlayDefaultBtnSound() => PlayGlobalSFX("button_click");
}
