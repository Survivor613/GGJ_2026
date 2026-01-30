using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DialogueSystem.Data;
using DialogueSystem.UI;
using DialogueSystem.Actors;

namespace DialogueSystem.Core
{
    public enum RunnerState
    {
        Idle,
        PlayingLine,
        WaitingInput,
        ExecutingCommand
    }

    public class DialogueRunner : MonoBehaviour
    {
        public static DialogueRunner Instance { get; private set; }

        [Header("References")]
        [SerializeField] private MonoBehaviour dialogueViewComponent; // 可以是 DialogueView 或 DialogueViewUniversal
        [SerializeField] private ActorController actorController;
        [SerializeField] private HistoryView historyView;
        
        private IDialogueView dialogueView;

        private DialogueScriptSO currentScript;
        private DialogueNode currentNode;
        private RunnerState state = RunnerState.Idle;

        private void Awake()
        {
            Instance = this;
            
            // 初始化 dialogueView 接口
            if (dialogueViewComponent != null)
            {
                dialogueView = dialogueViewComponent as IDialogueView;
                if (dialogueView == null)
                {
                    Debug.LogError("DialogueView 组件必须实现 IDialogueView 接口！");
                }
            }
        }

        public void StartDialogue(DialogueScriptSO script, string startNodeId = "")
        {
            currentScript = script;
            string id = string.IsNullOrEmpty(startNodeId) && script.nodes.Count > 0 ? script.nodes[0].id : startNodeId;
            PlayNode(id);
        }

        public void PlayNode(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                EndDialogue();
                return;
            }

            currentNode = currentScript.GetNode(id);
            if (currentNode == null)
            {
                Debug.LogError($"Node {id} not found in script.");
                EndDialogue();
                return;
            }

            if (currentNode is LineNode lineNode)
            {
                StartCoroutine(PlayLineRoutine(lineNode));
            }
            else if (currentNode is CommandNode cmdNode)
            {
                ExecuteCommand(cmdNode);
            }
        }

        private IEnumerator PlayLineRoutine(LineNode node)
        {
            state = RunnerState.PlayingLine;
            
            // Focus actor
            if (!string.IsNullOrEmpty(node.speakerId))
            {
                actorController.SetFocus(node.speakerId);
            }
            else
            {
                actorController.SetFocus(null); // Dim all
            }

            // Update UI
            string displayName = !string.IsNullOrEmpty(node.speakerName) ? node.speakerName : actorController.GetDisplayName(node.speakerId);
            dialogueView.ShowLine(displayName, node.text);

            // Wait for typewriter
            while (dialogueView.IsTypewriterPlaying)
            {
                yield return null;
            }

            // Log to history
            if (historyView != null)
            {
                historyView.AddEntry(displayName, node.text);
            }

            state = RunnerState.WaitingInput;
            dialogueView.ShowContinueIcon(true);
        }

        private void ExecuteCommand(CommandNode node)
        {
            state = RunnerState.ExecutingCommand;
            
            // Simple command parser: "actor show id=alice portrait=smile x=0 y=0"
            string cmd = node.command;
            string[] parts = cmd.Split(' ');
            
            if (parts.Length > 0)
            {
                string action = parts[0].ToLower();
                switch (action)
                {
                    case "actor":
                        HandleActorCommand(parts);
                        break;
                    case "wait":
                        if (parts.Length > 1 && float.TryParse(parts[1], out float t))
                        {
                            StartCoroutine(WaitRoutine(t, node.nextId));
                            return;
                        }
                        break;
                    case "scene":
                        // --- 修改后的逻辑 ---
                        if (parts.Length > 1)
                        {
                            // 兼容两种格式：
                            // 1. scene LevelName
                            // 2. scene load name=LevelName
                            string sceneName = "";

                            if (parts[1].ToLower() == "load")
                            {
                                var paramsMap = new Dictionary<string, string>();
                                for (int i = 2; i < parts.Length; i++)
                                {
                                    string[] kv = parts[i].Split('=');
                                    if (kv.Length == 2) paramsMap[kv[0]] = kv[1];
                                }
                                sceneName = paramsMap.GetValueOrDefault("name");
                            }
                            else
                            {
                                sceneName = parts[1];
                            }

                            if (!string.IsNullOrEmpty(sceneName))
                            {
                                // 使用你的 GameManager 执行带转场的切换
                                GameManager.instance.ChangeSceneTo(sceneName);
                                // 注意：场景切换后当前对话会自动中断，所以这里直接返回即可
                                return;
                            }
                        }
                        break;
                }
            }

            // Default: go to next node immediately
            PlayNode(node.nextId);
        }

        private void HandleActorCommand(string[] parts)
        {
            if (parts.Length < 2) return;
            string subAction = parts[1].ToLower();
            
            var paramsMap = new Dictionary<string, string>();
            for (int i = 2; i < parts.Length; i++)
            {
                string[] kv = parts[i].Split('=');
                if (kv.Length == 2) paramsMap[kv[0]] = kv[1];
            }

            string id = paramsMap.GetValueOrDefault("id");
            
            if (subAction == "show")
            {
                string portrait = paramsMap.GetValueOrDefault("portrait");
                float x = float.Parse(paramsMap.GetValueOrDefault("x", "0"));
                float y = float.Parse(paramsMap.GetValueOrDefault("y", "0"));
                actorController.ShowActor(id, portrait, new Vector2(x, y));
            }
            else if (subAction == "hide")
            {
                actorController.HideActor(id);
            }
            else if (subAction == "focus")
            {
                actorController.SetFocus(id);
            }
        }

        private void HandleSceneCommand(string[] parts)
        {
            if (parts.Length < 2) return;
            string subAction = parts[1].ToLower();

            var paramsMap = new Dictionary<string, string>();
            for (int i = 2; i < parts.Length; i++)
            {
                string[] kv = parts[i].Split('=');
                if (kv.Length == 2) paramsMap[kv[0]] = kv[1];
            }

            if (subAction == "load")
            {
                string sceneName = paramsMap.GetValueOrDefault("name");
                string sceneIndex = paramsMap.GetValueOrDefault("index");
                string modeValue = paramsMap.GetValueOrDefault("mode", "single");
                LoadSceneMode mode = modeValue.ToLower() == "additive" ? LoadSceneMode.Additive : LoadSceneMode.Single;

                if (!string.IsNullOrEmpty(sceneName))
                {
                    SceneManager.LoadScene(sceneName, mode);
                }
                else if (int.TryParse(sceneIndex, out int index))
                {
                    SceneManager.LoadScene(index, mode);
                }
                else
                {
                    Debug.LogWarning("Scene command missing name or index. Example: scene load name=YourScene");
                }
            }
            else if (subAction == "reload")
            {
                var activeScene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(activeScene.buildIndex);
            }
        }

        private IEnumerator WaitRoutine(float time, string nextId)
        {
            yield return new WaitForSeconds(time);
            PlayNode(nextId);
        }

        public void OnClickArea()
        {
            if (state == RunnerState.PlayingLine)
            {
                dialogueView.SkipTypewriter();
            }
            else if (state == RunnerState.WaitingInput)
            {
                dialogueView.ShowContinueIcon(false);
                PlayNode(currentNode.nextId);
            }
        }

        private void EndDialogue()
        {
            state = RunnerState.Idle;
            dialogueView.Hide();
            Debug.Log("Dialogue Ended");
        }
    }

    public static class DictionaryExtensions
    {
        public static string GetValueOrDefault(this Dictionary<string, string> dict, string key, string defaultValue = "")
        {
            return dict.TryGetValue(key, out string val) ? val : defaultValue;
        }
    }
}
