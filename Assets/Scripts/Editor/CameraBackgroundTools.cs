using UnityEditor;
using UnityEngine;

/// <summary>
/// 场景相机背景设置工具
/// </summary>
public static class CameraBackgroundTools
{
    [MenuItem("Tools/Scene/Set Camera Background Black (一键黑底) 🖤")]
    public static void SetCameraBackgroundBlack()
    {
        var cameras = Object.FindObjectsOfType<Camera>(true);
        if (cameras.Length == 0)
        {
            Debug.LogWarning("⚠ 场景中未找到 Camera");
            return;
        }

        foreach (var cam in cameras)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
            EditorUtility.SetDirty(cam);
        }

        Debug.Log("✓ 已将场景相机背景改为黑色");
    }
}
