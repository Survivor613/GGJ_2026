using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// UI视频背景管理器
/// 当UI显示时自动播放视频背景，隐藏时停止播放
/// </summary>
[RequireComponent(typeof(RawImage))]
public class UI_VideoBackground : MonoBehaviour
{
    [Header("Video Settings")]
    [Tooltip("视频文件（可选，也可以使用URL）")]
    [SerializeField] private VideoClip videoClip;
    
    [Tooltip("视频URL（如果没有VideoClip，可以使用URL）")]
    [SerializeField] private string videoURL;
    
    [Tooltip("是否循环播放")]
    [SerializeField] private bool isLooping = true;
    
    [Tooltip("是否播放音频")]
    [SerializeField] private bool playAudio = false;
    
    [Tooltip("RenderTexture分辨率宽度")]
    [SerializeField] private int renderTextureWidth = 1920;
    
    [Tooltip("RenderTexture分辨率高度")]
    [SerializeField] private int renderTextureHeight = 1080;

    [Header("Auto Reference")]
    [Tooltip("自动引用现有的RenderTexture（如MainMenuVideoRT）")]
    [SerializeField] private RenderTexture existingRenderTexture;

    // 私有变量
    private RawImage rawImage;
    private VideoPlayer videoPlayer;
    private RenderTexture renderTexture;
    private GameObject videoPlayerObject;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        SetupVideoPlayer();
    }

    private void OnEnable()
    {
        // UI显示时播放视频
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }

    private void OnDisable()
    {
        // UI隐藏时停止视频
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }

    private void OnDestroy()
    {
        // 清理资源
        if (videoPlayerObject != null)
        {
            Destroy(videoPlayerObject);
        }

        // 如果是运行时创建的RenderTexture，销毁它
        if (renderTexture != null && existingRenderTexture == null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }

    /// <summary>
    /// 设置视频播放器
    /// </summary>
    private void SetupVideoPlayer()
    {
        // 1. 获取或创建RenderTexture
        if (existingRenderTexture != null)
        {
            renderTexture = existingRenderTexture;
        }
        else
        {
            renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 0);
        }
        
        rawImage.texture = renderTexture;

        // 2. 创建VideoPlayer GameObject
        videoPlayerObject = new GameObject("UI_VideoPlayer");
        videoPlayerObject.transform.SetParent(transform, false);
        
        videoPlayer = videoPlayerObject.AddComponent<VideoPlayer>();
        
        // 3. 配置VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = isLooping;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        
        // 4. 设置视频源
        if (videoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = videoClip;
        }
        else if (!string.IsNullOrEmpty(videoURL))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = videoURL;
        }
        else
        {
            Debug.LogWarning("UI_VideoBackground: 没有设置视频源（VideoClip或URL）");
        }

        // 5. 音频设置
        if (playAudio)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            AudioSource audioSource = videoPlayerObject.AddComponent<AudioSource>();
            videoPlayer.SetTargetAudioSource(0, audioSource);
        }
        else
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }
    }

    /// <summary>
    /// 运行时更换视频
    /// </summary>
    public void SetVideoClip(VideoClip clip)
    {
        if (videoPlayer != null && clip != null)
        {
            videoPlayer.clip = clip;
            if (gameObject.activeInHierarchy)
            {
                videoPlayer.Play();
            }
        }
    }

    /// <summary>
    /// 运行时更换视频URL
    /// </summary>
    public void SetVideoURL(string url)
    {
        if (videoPlayer != null && !string.IsNullOrEmpty(url))
        {
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = url;
            if (gameObject.activeInHierarchy)
            {
                videoPlayer.Play();
            }
        }
    }
}
