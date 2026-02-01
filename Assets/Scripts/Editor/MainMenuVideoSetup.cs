using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 一键部署主界面 MP4 背景
/// 菜单：Tools/Video/Setup Main Menu Video (一键部署)
/// </summary>
public static class MainMenuVideoSetup
{
    [MenuItem("Tools/Video/Setup Main Menu Video (一键部署) 🎬")]
    public static void SetupMainMenuVideo()
    {
        // 1) 获取或创建 Canvas
        Canvas canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // 2) 创建/复用 RawImage
        var rawImageGO = GameObject.Find("MainMenuVideo");
        if (rawImageGO == null)
        {
            rawImageGO = new GameObject("MainMenuVideo");
            rawImageGO.transform.SetParent(canvas.transform, false);
        }

        var rawImage = rawImageGO.GetComponent<RawImage>();
        if (rawImage == null)
        {
            rawImage = rawImageGO.AddComponent<RawImage>();
        }

        // 拉满屏幕
        var rect = rawImageGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 放到最底层
        rawImageGO.transform.SetAsFirstSibling();

        // 3) 创建或复用 RenderTexture 资源
        const string folderPath = "Assets/Video";
        const string rtPath = folderPath + "/MainMenuVideoRT.renderTexture";

        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Video");
        }

        RenderTexture rt = AssetDatabase.LoadAssetAtPath<RenderTexture>(rtPath);
        if (rt == null)
        {
            rt = new RenderTexture(1920, 1080, 0);
            AssetDatabase.CreateAsset(rt, rtPath);
        }
        rawImage.texture = rt;

        // 4) 创建/复用 VideoPlayer
        var videoGO = GameObject.Find("MainMenuVideoPlayer");
        if (videoGO == null)
        {
            videoGO = new GameObject("MainMenuVideoPlayer");
        }

        var videoPlayer = videoGO.GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = videoGO.AddComponent<VideoPlayer>();
        }

        videoPlayer.playOnAwake = true;
        videoPlayer.isLooping = true;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = rt;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;

        // 如果当前选中了 VideoClip，自动绑定
        if (Selection.activeObject is VideoClip clip)
        {
            videoPlayer.clip = clip;
            Debug.Log($"✓ 已绑定视频：{clip.name}");
        }
        else
        {
            Debug.Log("⚠ 未选择 VideoClip。请在 Project 中选中 MP4 再执行一次，或手动拖到 VideoPlayer.clip。");
        }

        EditorUtility.DisplayDialog("部署完成",
            "已创建主界面视频背景。\n如未绑定视频，请在 Project 中选中 MP4 后再次运行。",
            "OK");
    }
}
