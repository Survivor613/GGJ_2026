using UnityEngine;
using UnityEditor;
using TMPro;
using System.IO;

/// <summary>
/// 修复中文显示问题 - 自动创建支持中文的 TextMeshPro 字体
/// 菜单：Tools/Dialogue/Fix Chinese Font (修复中文显示) 🔤
/// </summary>
public class ChineseFontFixer : EditorWindow
{
    // [MenuItem("Tools/Dialogue/Fix Chinese Font (修复中文显示) 🔤")] // 已禁用：TMP字体工具，使用原生Text后不需要
    static void FixChineseFont()
    {
        Debug.Log("<color=cyan>========== 开始修复中文显示问题 ==========</color>");
        
        // 1. 检查是否已有中文字体资源
        TMP_FontAsset chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/ChineseFont SDF.asset");
        
        if (chineseFont == null)
        {
            Debug.Log("未找到中文字体资源，请按以下步骤手动创建：");
            Debug.Log("<color=yellow>========================================</color>");
            Debug.Log("1. 在 Project 窗口右键，选择 Create → TextMeshPro → Font Asset");
            Debug.Log("2. 或者使用菜单：Window → TextMeshPro → Font Asset Creator");
            Debug.Log("3. 选择 Source Font File（推荐使用系统自带的 Arial Unicode MS 或其他中文字体）");
            Debug.Log("4. 在 Character Set 选择 'Unicode Range (Hex)' 或 'Characters from File'");
            Debug.Log("5. 添加常用中文 Unicode 范围：");
            Debug.Log("   - 基本汉字：4E00-9FFF");
            Debug.Log("   - 标点符号：3000-303F");
            Debug.Log("6. 点击 'Generate Font Atlas'");
            Debug.Log("7. 保存为 'Assets/Resources/ChineseFont SDF.asset'");
            Debug.Log("<color=yellow>========================================</color>");
            Debug.Log("");
            Debug.Log("<color=cyan>提示：如果找不到中文字体文件，可以从网上下载免费字体：</color>");
            Debug.Log("- 思源黑体 (Source Han Sans): https://github.com/adobe-fonts/source-han-sans");
            Debug.Log("- 文泉驿微米黑: http://wenq.org/");
            
            // 尝试打开 Font Asset Creator
            EditorApplication.ExecuteMenuItem("Window/TextMeshPro/Font Asset Creator");
            return;
        }
        
        // 2. 应用中文字体到所有 TMP 组件
        ApplyChineseFontToScene(chineseFont);
        
        Debug.Log("<color=green>✓ 中文字体修复完成！</color>");
    }
    
    static void ApplyChineseFontToScene(TMP_FontAsset font)
    {
        // 查找场景中所有 TMP 组件
        TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        
        int count = 0;
        foreach (var text in allTexts)
        {
            text.font = font;
            EditorUtility.SetDirty(text);
            count++;
        }
        
        Debug.Log($"✓ 已为 {count} 个 TMP 组件应用中文字体");
    }
    
    // [MenuItem("Tools/Dialogue/Apply Chinese Font to Scene")] // 已禁用：TMP字体工具，使用原生Text后不需要
    static void ApplyFontToScene()
    {
        TMP_FontAsset chineseFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/ChineseFont SDF.asset");
        
        if (chineseFont == null)
        {
            Debug.LogError("未找到中文字体资源！请先运行 'Fix Chinese Font (修复中文显示)'");
            return;
        }
        
        ApplyChineseFontToScene(chineseFont);
        Debug.Log("<color=green>✓ 中文字体应用完成！</color>");
    }
}
