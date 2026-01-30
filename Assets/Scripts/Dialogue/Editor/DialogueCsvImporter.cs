using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using DialogueSystem.Data;

/// <summary>
/// CSV 导入对话：空白角色列视为旁白
/// 预期格式：speaker,text
/// </summary>
public class DialogueCsvImporter : EditorWindow
{
    [SerializeField] private TextAsset csvFile;
    [SerializeField] private string outputPath = "Assets/Resources/Dialogue_FromCsv.asset";
    [SerializeField] private bool hasHeader = false;
    [SerializeField] private bool trimWhitespace = true;
    [SerializeField] private CsvEncoding csvEncoding = CsvEncoding.Auto;

    private enum CsvEncoding
    {
        Auto,
        Utf8,
        Utf8WithBom,
        Gbk
    }

    [MenuItem("Tools/Dialogue/Import CSV To DialogueScript")]
    public static void ShowWindow()
    {
        GetWindow<DialogueCsvImporter>("CSV 对话导入");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("CSV 导入对话", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV 文件", csvFile, typeof(TextAsset), false);
        outputPath = EditorGUILayout.TextField("输出路径", outputPath);
        hasHeader = EditorGUILayout.Toggle("包含表头", hasHeader);
        trimWhitespace = EditorGUILayout.Toggle("去除首尾空格", trimWhitespace);
        csvEncoding = (CsvEncoding)EditorGUILayout.EnumPopup("CSV 编码", csvEncoding);

        EditorGUILayout.Space(8);
        if (GUILayout.Button("导入生成 DialogueScript"))
        {
            ImportCsv();
        }
    }

    private void ImportCsv()
    {
        if (csvFile == null)
        {
            EditorUtility.DisplayDialog("错误", "请先选择 CSV 文件。", "确定");
            return;
        }

        string path = outputPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            path = "Assets/Resources/Dialogue_FromCsv.asset";
        }

        if (!path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
        {
            path += ".asset";
        }

        string folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder) && !AssetDatabase.IsValidFolder(folder))
        {
            CreateFolders(folder);
        }

        string csvText = ReadCsvText(csvFile, csvEncoding);
        List<LineNode> lines = ParseCsv(csvText);
        if (lines.Count == 0)
        {
            EditorUtility.DisplayDialog("错误", "CSV 中没有可导入的对话行。", "确定");
            return;
        }

        // 生成节点并串联 nextId
        var script = ScriptableObject.CreateInstance<DialogueScriptSO>();
        for (int i = 0; i < lines.Count; i++)
        {
            var node = lines[i];
            node.id = $"line_{i + 1:0000}";
            node.nextId = i < lines.Count - 1 ? $"line_{i + 2:0000}" : "";
            script.nodes.Add(node);
        }

        // 覆盖旧资产
        if (AssetDatabase.LoadAssetAtPath<DialogueScriptSO>(path) != null)
        {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(script, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = script;
        EditorGUIUtility.PingObject(script);

        Debug.Log($"<color=green>✓ CSV 导入完成: {path}</color>");
    }

    private List<LineNode> ParseCsv(string text)
    {
        var result = new List<LineNode>();
        if (string.IsNullOrEmpty(text))
        {
            return result;
        }

        using (var reader = new StringReader(text))
        {
            string line;
            bool skippedHeader = false;
            while ((line = reader.ReadLine()) != null)
            {
                if (!skippedHeader && hasHeader)
                {
                    skippedHeader = true;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                List<string> fields = ParseCsvLine(line);
                if (fields.Count == 0)
                {
                    continue;
                }

                string speaker = fields.Count > 0 ? fields[0] : "";
                string content = fields.Count > 1 ? fields[1] : fields[0];

                if (trimWhitespace)
                {
                    speaker = speaker?.Trim();
                    content = content?.Trim();
                }

                if (string.IsNullOrWhiteSpace(content))
                {
                    continue;
                }

                // 空白 speaker 视为旁白
                var node = new LineNode
                {
                    speakerId = "",
                    speakerName = string.IsNullOrWhiteSpace(speaker) ? "" : speaker,
                    text = content
                };

                result.Add(node);
            }
        }

        return result;
    }

    private List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        fields.Add(sb.ToString());

        if (fields.Count > 0 && fields[0].Length > 0 && fields[0][0] == '\uFEFF')
        {
            fields[0] = fields[0].TrimStart('\uFEFF');
        }

        return fields;
    }

    private void CreateFolders(string folderPath)
    {
        string[] parts = folderPath.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private string ReadCsvText(TextAsset asset, CsvEncoding encoding)
    {
        if (asset == null) return string.Empty;

        string assetPath = AssetDatabase.GetAssetPath(asset);
        if (string.IsNullOrEmpty(assetPath))
        {
            return asset.text;
        }

        byte[] bytes = File.ReadAllBytes(assetPath);
        if (bytes.Length == 0) return string.Empty;

        switch (encoding)
        {
            case CsvEncoding.Utf8:
                return Encoding.UTF8.GetString(bytes);
            case CsvEncoding.Utf8WithBom:
                return Encoding.UTF8.GetString(StripUtf8Bom(bytes));
            case CsvEncoding.Gbk:
                return Encoding.GetEncoding(936).GetString(bytes);
            case CsvEncoding.Auto:
            default:
                // 简单自动：有 UTF-8 BOM 就按 UTF-8，否则尝试 UTF-8，失败再用 GBK
                if (HasUtf8Bom(bytes))
                {
                    return Encoding.UTF8.GetString(StripUtf8Bom(bytes));
                }
                try
                {
                    return Encoding.UTF8.GetString(bytes);
                }
                catch
                {
                    return Encoding.GetEncoding(936).GetString(bytes);
                }
        }
    }

    private bool HasUtf8Bom(byte[] bytes)
    {
        return bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
    }

    private byte[] StripUtf8Bom(byte[] bytes)
    {
        if (!HasUtf8Bom(bytes)) return bytes;
        byte[] result = new byte[bytes.Length - 3];
        Array.Copy(bytes, 3, result, 0, result.Length);
        return result;
    }
}
