using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 查找并列出所有可用的 TMP 字体，让用户选择
/// 菜单：Tools/Dialogue/Find All TMP Fonts
/// </summary>
public class FindAndApplyFont : EditorWindow
{
    private List<TMP_FontAsset> allFonts = new List<TMP_FontAsset>();
    private Vector2 scrollPos;
    private TMP_FontAsset selectedFont;

    // [MenuItem("Tools/Dialogue/Find All TMP Fonts (查找所有字体) 🔎")] // 已禁用：使用原生Text后不需要
    static void ShowWindow()
    {
        GetWindow<FindAndApplyFont>("选择字体");
    }

    void OnEnable()
    {
        RefreshFontList();
    }

    void RefreshFontList()
    {
        allFonts.Clear();
        
        // 查找所有 TMP_FontAsset
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            if (font != null)
            {
                allFonts.Add(font);
            }
        }
        
        Debug.Log($"找到 {allFonts.Count} 个 TMP 字体资源");
    }

    void OnGUI()
    {
        GUILayout.Label("项目中的所有 TMP 字体：", EditorStyles.boldLabel);
        
        if (GUILayout.Button("刷新列表"))
        {
            RefreshFontList();
        }
        
        GUILayout.Space(10);
        
        if (allFonts.Count == 0)
        {
            EditorGUILayout.HelpBox("未找到任何 TMP 字体资源！\n\n请确保：\n1. 已将字体文件拖入 Unity\n2. 文件类型是 .asset\n3. 是 TextMeshPro 的字体资源", MessageType.Warning);
            return;
        }
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        
        foreach (var font in allFonts)
        {
            EditorGUILayout.BeginHorizontal();
            
            // 显示字体信息
            string fontInfo = $"{font.name}";
            if (font.name.ToLower().Contains("chinese") || 
                font.name.ToLower().Contains("noto") || 
                font.name.ToLower().Contains("cjk") ||
                font.name.ToLower().Contains("源"))
            {
                fontInfo += " ⭐ (推荐用于中文)";
            }
            
            EditorGUILayout.LabelField(fontInfo);
            
            if (GUILayout.Button("应用此字体", GUILayout.Width(100)))
            {
                ApplyFont(font);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.EndScrollView();
        
        GUILayout.Space(20);
        EditorGUILayout.HelpBox("点击【应用此字体】按钮，将会把该字体应用到对话系统的所有文本组件。", MessageType.Info);
    }

    void ApplyFont(TMP_FontAsset font)
    {
        Debug.Log($"<color=cyan>应用字体: {font.name}</color>");
        
        TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        int count = 0;
        
        foreach (var text in allTexts)
        {
            if (text.gameObject.name == "NameText" || text.gameObject.name == "BodyText")
            {
                text.font = font;
                EditorUtility.SetDirty(text);
                count++;
            }
        }
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"<color=green>✓ 完成！已为 {count} 个组件应用字体: {font.name}</color>");
        
        EditorUtility.DisplayDialog("应用成功", 
            $"已成功应用字体：{font.name}\n\n共更新 {count} 个文本组件。\n\n现在可以测试对话系统了！", 
            "确定");
    }
}
