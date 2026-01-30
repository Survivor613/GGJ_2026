using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using DialogueSystem.UI;
using DialogueSystem.Core;

/// <summary>
/// 清理 TMP 残留并迁移到原生 Text 系统
/// 菜单：Tools/Dialogue/Cleanup and Migrate (清理迁移) 🧹
/// </summary>
public class CleanupAndMigrate : EditorWindow
{
    [MenuItem("Tools/Dialogue/Cleanup and Migrate (清理迁移) 🧹")]
    static void Cleanup()
    {
        bool confirm = EditorUtility.DisplayDialog("确认清理", 
            "此操作将：\n\n1. 检查并报告所有 TMP 残留\n2. 将 DialogueView 替换为 Universal 版本\n3. 将 TypewriterEffect 替换为 Universal 版本\n4. 禁用所有 TextEffectController\n\n这将确保系统完全使用原生 Text。\n\n确定继续？", 
            "确定", "取消");
            
        if (!confirm) return;
        
        Debug.Log("<color=cyan>========== 开始清理和迁移 ==========</color>");
        
        int fixedCount = 0;
        
        // 1. 检查 TMP 组件残留
        TMP_Text[] tmpTexts = GameObject.FindObjectsOfType<TMP_Text>(true);
        if (tmpTexts.Length > 0)
        {
            Debug.LogWarning($"⚠ 发现 {tmpTexts.Length} 个 TMP_Text 组件残留：");
            foreach (var tmp in tmpTexts)
            {
                Debug.LogWarning($"  - {tmp.gameObject.name} ({tmp.GetType().Name})");
            }
        }
        
        // 2. 替换 DialogueView 为 DialogueViewUniversal
        DialogueView[] oldViews = GameObject.FindObjectsOfType<DialogueView>(true);
        foreach (var oldView in oldViews)
        {
            GameObject go = oldView.gameObject;
            
            // 获取旧组件的引用
            SerializedObject so = new SerializedObject(oldView);
            GameObject panel = so.FindProperty("panel").objectReferenceValue as GameObject;
            GameObject continueIcon = so.FindProperty("continueIcon").objectReferenceValue as GameObject;
            
            // 删除旧组件
            DestroyImmediate(oldView);
            
            // 添加新组件
            DialogueViewUniversal newView = go.AddComponent<DialogueViewUniversal>();
            
            // 设置引用
            SerializedObject newSO = new SerializedObject(newView);
            newSO.FindProperty("panel").objectReferenceValue = panel;
            newSO.FindProperty("continueIcon").objectReferenceValue = continueIcon;
            
            // 查找 Unity Text 组件
            Text[] texts = go.GetComponentsInChildren<Text>(true);
            foreach (var text in texts)
            {
                if (text.gameObject.name == "NameText")
                    newSO.FindProperty("nameText").objectReferenceValue = text;
                if (text.gameObject.name == "BodyText")
                {
                    newSO.FindProperty("bodyText").objectReferenceValue = text;
                    
                    // 查找或添加 TypewriterEffectUniversal
                    TypewriterEffectUniversal typewriter = text.GetComponent<TypewriterEffectUniversal>();
                    if (typewriter == null)
                    {
                        typewriter = text.gameObject.AddComponent<TypewriterEffectUniversal>();
                        SerializedObject twSO = new SerializedObject(typewriter);
                        twSO.FindProperty("textComponent").objectReferenceValue = text;
                        twSO.ApplyModifiedProperties();
                    }
                    newSO.FindProperty("typewriter").objectReferenceValue = typewriter;
                }
            }
            
            newSO.ApplyModifiedProperties();
            
            EditorUtility.SetDirty(go);
            fixedCount++;
            Debug.Log($"✓ 替换 DialogueView 为 DialogueViewUniversal 在 {go.name}");
        }
        
        // 3. 更新 DialogueRunner 引用
        DialogueRunner runner = GameObject.FindObjectOfType<DialogueRunner>();
        if (runner != null)
        {
            SerializedObject runnerSO = new SerializedObject(runner);
            var dialogueView = GameObject.FindObjectOfType<DialogueViewUniversal>();
            if (dialogueView != null)
            {
                runnerSO.FindProperty("dialogueViewComponent").objectReferenceValue = dialogueView;
                runnerSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(runner);
                Debug.Log("✓ 更新 DialogueRunner 引用到 DialogueViewUniversal");
            }
        }
        
        // 4. 禁用 TextEffectController
        var effectControllers = GameObject.FindObjectsOfType<DialogueSystem.Effects.TextEffectController>(true);
        foreach (var controller in effectControllers)
        {
            if (controller.enabled)
            {
                controller.enabled = false;
                EditorUtility.SetDirty(controller);
                fixedCount++;
                Debug.Log($"✓ 禁用 TextEffectController 在 {controller.gameObject.name}");
            }
        }
        
        // 5. 删除旧的 TypewriterEffect（TMP版本）
        TypewriterEffect[] oldTypewriters = GameObject.FindObjectsOfType<TypewriterEffect>(true);
        foreach (var oldTw in oldTypewriters)
        {
            if (oldTw != null)
            {
                GameObject go = oldTw.gameObject;
                DestroyImmediate(oldTw);
                fixedCount++;
                Debug.Log($"✓ 删除旧 TypewriterEffect 从 {go.name}");
            }
        }
        
        // 保存场景
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log($"<color=green>========== ✓ 清理迁移完成！共处理 {fixedCount} 项 ==========</color>");
        
        string message = $"清理迁移完成！\n\n共处理 {fixedCount} 项。\n\n";
        if (tmpTexts.Length > 0)
        {
            message += $"⚠ 仍有 {tmpTexts.Length} 个 TMP 组件残留，建议手动删除或重新搭建场景。\n\n";
        }
        message += "现在可以测试对话系统了！";
        
        EditorUtility.DisplayDialog("清理迁移完成", message, "确定");
    }
}
