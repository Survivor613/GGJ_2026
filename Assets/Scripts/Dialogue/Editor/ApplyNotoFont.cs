using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// 自动查找并应用 Noto Sans CJK 字体
/// 菜单：Tools/Dialogue/Apply Noto Font
/// </summary>
public class ApplyNotoFont : EditorWindow
{
    // [MenuItem("Tools/Dialogue/Apply Noto Font (应用Noto字体) 🎨")] // 已禁用：使用原生Text后不需要
    static void ApplyNotoFontToScene()
    {
        Debug.Log("<color=cyan>========== 应用 Noto Sans CJK 字体 ==========</color>");
        
        // 搜索 Noto 字体资源
        string[] guids = AssetDatabase.FindAssets("NotoSansCJK t:TMP_FontAsset");
        
        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog("未找到字体", 
                "未找到 NotoSansCJK 字体资源！\n\n请确保已将 NotoSansCJK-Regular SDF.asset 导入到项目中。", 
                "确定");
            return;
        }
        
        // 加载第一个找到的 Noto 字体
        string path = AssetDatabase.GUIDToAssetPath(guids[0]);
        TMP_FontAsset notoFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        
        Debug.Log($"✓ 找到字体资源: {path}");
        
        // 应用到所有对话相关的 TMP 组件
        TMP_Text[] allTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        int count = 0;
        
        foreach (var text in allTexts)
        {
            if (text.gameObject.name == "NameText" || text.gameObject.name == "BodyText")
            {
                text.font = notoFont;
                EditorUtility.SetDirty(text);
                count++;
                Debug.Log($"  → 为 {text.gameObject.name} 应用 Noto 字体");
            }
        }
        
        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"<color=green>✓ 完成！已为 {count} 个组件应用 Noto Sans CJK 字体</color>");
        
        EditorUtility.DisplayDialog("应用成功", 
            $"已成功为 {count} 个文本组件应用 Noto Sans CJK 字体！\n\n现在切换到中文模式：\nTools → Dialogue → Switch to Chinese Mode\n\n然后按 Play 测试！", 
            "确定");
    }
}
