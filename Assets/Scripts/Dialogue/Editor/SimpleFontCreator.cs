using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 简易中文字体生成器 - 只包含对话中实际使用的字符
/// 菜单：Tools/Dialogue/Create Custom Font (简易版)
/// </summary>
public class SimpleFontCreator : EditorWindow
{
    private Font sourceFont;
    private string characterString = "你好欢迎来到对话系统测试可以说话很快或者得慢这是一段旁白文字没有角色高亮危险冲鸭两人踏上了旅途爱丽丝";
    
    // [MenuItem("Tools/Dialogue/Create Custom Font (简易版) 🎨")] // 已禁用：TMP字体工具，使用原生Text后不需要
    static void ShowWindow()
    {
        GetWindow<SimpleFontCreator>("简易字体生成器");
    }
    
    void OnGUI()
    {
        GUILayout.Label("简易中文字体生成器", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox("这个工具会创建一个只包含你指定字符的小型字体文件，生成速度快！", MessageType.Info);
        GUILayout.Space(10);
        
        sourceFont = (Font)EditorGUILayout.ObjectField("源字体文件", sourceFont, typeof(Font), false);
        
        if (sourceFont == null)
        {
            EditorGUILayout.HelpBox("请选择一个中文字体文件（如 Arial Unicode MS）", MessageType.Warning);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("包含的字符：");
        characterString = EditorGUILayout.TextArea(characterString, GUILayout.Height(100));
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox($"当前包含 {GetUniqueCharCount()} 个不同的字符", MessageType.None);
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("生成字体资源", GUILayout.Height(40)))
        {
            if (sourceFont == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择源字体文件！", "确定");
                return;
            }
            
            CreateFontAsset();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("添加测试对话中的所有字符"))
        {
            characterString = "你好欢迎来到对话系统测试我可以说话很快或者得慢这是一段旁白文字没有角色高亮危险冲鸭两人踏上了旅途爱丽丝" +
                            "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz" +
                            "！？。，、；：（）【】《》…—";
        }
    }
    
    int GetUniqueCharCount()
    {
        HashSet<char> uniqueChars = new HashSet<char>(characterString);
        return uniqueChars.Count;
    }
    
    void CreateFontAsset()
    {
        Debug.Log("<color=cyan>========== 开始创建简易中文字体 ==========</color>");
        
        // 使用 TMP Font Asset Creator
        try
        {
            // 创建字体资源
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(sourceFont);
            
            if (fontAsset != null)
            {
                // 保存资源
                string path = "Assets/Resources/ChineseFont SDF.asset";
                AssetDatabase.CreateAsset(fontAsset, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                Debug.Log($"<color=green>✓ 字体资源创建成功：{path}</color>");
                Debug.Log($"<color=yellow>注意：这个方法创建的是基础字体，如果需要更多字符，请使用 Font Asset Creator 手动创建。</color>");
                
                // 选中创建的资源
                Selection.activeObject = fontAsset;
                EditorGUIUtility.PingObject(fontAsset);
                
                EditorUtility.DisplayDialog("成功", 
                    "字体资源创建成功！\n\n现在运行：\nTools → Dialogue → Apply Chinese Font to Scene", 
                    "确定");
            }
            else
            {
                EditorUtility.DisplayDialog("失败", "字体资源创建失败，请检查源字体文件是否正确。", "确定");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"创建字体资源时出错：{e.Message}");
            EditorUtility.DisplayDialog("错误", $"创建失败：{e.Message}", "确定");
        }
    }
}
