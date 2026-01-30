using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 将对话系统从 TextMeshPro 转换为 Unity 原生 Text
/// 菜单：Tools/Dialogue/Convert to Unity Text (原生Text)
/// </summary>
public class ConvertToUnityText : EditorWindow
{
    [MenuItem("Tools/Dialogue/Convert to Unity Text (原生Text，完美中文) 📝")]
    static void ConvertToUnityTextMode()
    {
        bool confirm = EditorUtility.DisplayDialog("确认转换", 
            "这将把对话系统从 TextMeshPro 转换为 Unity 原生 Text。\n\n优点：\n- 完美支持中文，不需要生成字体\n- 永远不会乱码\n\n缺点：\n- 失去顶点特效（shake/wave）\n- 保留打字机效果\n\n确定要转换吗？", 
            "确定", "取消");
            
        if (!confirm) return;
        
        Debug.Log("<color=cyan>========== 转换到 Unity Text ==========</color>");
        
        // 查找所有 TMP 组件
        TMP_Text[] tmpTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        List<GameObject> converted = new List<GameObject>();
        
        foreach (var tmp in tmpTexts)
        {
            // 只转换对话相关的组件
            if (tmp.gameObject.name == "NameText" || tmp.gameObject.name == "BodyText")
            {
                GameObject go = tmp.gameObject;
                
                // 保存原有信息
                string originalText = tmp.text;
                Color originalColor = tmp.color;
                int originalFontSize = (int)tmp.fontSize;
                TextAlignmentOptions alignment = tmp.alignment;
                
                // 删除 TMP 组件
                DestroyImmediate(tmp);
                
                // 添加 Unity Text
                Text unityText = go.AddComponent<Text>();
                unityText.text = originalText;
                unityText.color = originalColor;
                unityText.fontSize = originalFontSize;
                
                // 转换对齐方式
                switch (alignment)
                {
                    case TextAlignmentOptions.TopLeft:
                    case TextAlignmentOptions.Left:
                    case TextAlignmentOptions.BottomLeft:
                        unityText.alignment = TextAnchor.MiddleLeft;
                        break;
                    case TextAlignmentOptions.Center:
                    case TextAlignmentOptions.Midline:
                        unityText.alignment = TextAnchor.MiddleCenter;
                        break;
                    default:
                        unityText.alignment = TextAnchor.UpperLeft;
                        break;
                }
                
                // 启用富文本和自动换行
                unityText.supportRichText = true;
                unityText.horizontalOverflow = HorizontalWrapMode.Wrap;
                unityText.verticalOverflow = VerticalWrapMode.Truncate;
                
                // 使用系统默认字体（Arial，支持基本中文）
                // 或者设置为用户导入的中文字体
                Font chineseFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (chineseFont != null)
                {
                    unityText.font = chineseFont;
                }
                
                converted.Add(go);
                EditorUtility.SetDirty(go);
                
                Debug.Log($"✓ 转换 {go.name}");
            }
        }
        
        // 禁用 TextEffectController（不再需要）
        var effectControllers = GameObject.FindObjectsOfType<DialogueSystem.Effects.TextEffectController>(true);
        foreach (var controller in effectControllers)
        {
            controller.enabled = false;
            Debug.Log($"✓ 禁用 TextEffectController（原生Text不支持顶点特效）");
        }
        
        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"<color=green>✓ 转换完成！共转换 {converted.Count} 个组件</color>");
        Debug.Log("<color=yellow>注意：shake 和 wave 特效将不再工作（原生Text不支持顶点动画）</color>");
        Debug.Log("<color=yellow>打字机效果仍然正常工作</color>");
        
        EditorUtility.DisplayDialog("转换完成", 
            $"已成功转换 {converted.Count} 个文本组件！\n\n现在可以使用中文模式测试：\nTools → Dialogue → Switch to Chinese Mode\n\n然后按 Play 查看效果！", 
            "确定");
    }
}
