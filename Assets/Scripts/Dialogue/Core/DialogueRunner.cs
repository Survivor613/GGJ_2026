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
        [Header("Actor Auto Display")]
        [SerializeField] private bool autoShowActors = true;
        [SerializeField] private float commandOverrideSeconds = 1.5f;
        [Header("UI Control")]
        [SerializeField] private bool hideDialogueRootObject = true;
        
        private IDialogueView dialogueView;
        private GameObject dialogueRoot;

        private DialogueScriptSO currentScript;
        private DialogueNode currentNode;
        private int currentIndex = -1;
        private RunnerState state = RunnerState.Idle;
        private float manualOverrideUntil;

        private void Awake()
        {
            Instance = this;
            
            // 初始化 dialogueView 接口
            if (dialogueViewComponent != null)
            {
                dialogueView = dialogueViewComponent as IDialogueView;
                dialogueRoot = (dialogueViewComponent as Component)?.gameObject;
                if (dialogueView == null)
                {
                    Debug.LogError("DialogueView 组件必须实现 IDialogueView 接口！");
                }
            }
        }

        public void StartDialogue(DialogueScriptSO script, string startNodeId = "")
        {
            currentScript = script;
            if (!string.IsNullOrEmpty(startNodeId))
            {
                Debug.LogWarning("已启用按列表顺序播放，对话脚本不再使用 id/nextId。startNodeId 将被忽略。");
            }
            currentIndex = 0;
            PlayNodeAtIndex(currentIndex);
        }

        public void PlayNodeAtIndex(int index)
        {
            if (currentScript == null || currentScript.nodes == null || currentScript.nodes.Count == 0)
            {
                EndDialogue();
                return;
            }

            if (index < 0 || index >= currentScript.nodes.Count)
            {
                EndDialogue();
                return;
            }

            currentNode = currentScript.nodes[index];

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
            
            bool allowAuto = autoShowActors && Time.time >= manualOverrideUntil;
            string speakerName = node.speakerName;
            string resolvedActorId = actorController != null ? actorController.GetActorIdByDisplayName(speakerName) : string.Empty;

            if (allowAuto && !string.IsNullOrEmpty(resolvedActorId))
            {
                if (!actorController.IsActorShown(resolvedActorId))
                {
                    actorController.ShowActorAuto(resolvedActorId);
                }
                actorController.SetFocus(resolvedActorId);
            }
            else if (allowAuto && string.IsNullOrEmpty(resolvedActorId))
            {
                actorController.SetFocus(null); // Dim all
            }

            // Update UI
            string displayName = !string.IsNullOrEmpty(node.speakerName) ? node.speakerName : string.Empty;
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
                        RegisterManualOverride();
                        break;
                    case "wait":
                        if (parts.Length > 1 && float.TryParse(parts[1], out float t))
                        {
                            StartCoroutine(WaitRoutine(t, currentIndex + 1));
                            return;
                        }
                        break;
                    case "scene":
                        HandleSceneCommand(parts);
                        break;
                }
            }

            // Default: go to next node immediately
            PlayNodeAtIndex(++currentIndex);
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
            if (string.IsNullOrEmpty(id))
            {
                string displayName = paramsMap.GetValueOrDefault("name");
                if (!string.IsNullOrEmpty(displayName))
                {
                    id = actorController != null ? actorController.GetActorIdByDisplayName(displayName) : string.Empty;
                }
            }
            
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

        private IEnumerator WaitRoutine(float time, int nextIndex)
        {
            yield return new WaitForSeconds(time);
            currentIndex = nextIndex;
            PlayNodeAtIndex(currentIndex);
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
                PlayNodeAtIndex(++currentIndex);
            }
        }

        private void EndDialogue()
        {
            state = RunnerState.Idle;
            dialogueView.Hide();
            Debug.Log("Dialogue Ended");
        }

        public void HideUI()
        {
            dialogueView?.Hide();
            if (historyView != null)
            {
                historyView.Toggle(false);
            }
            if (hideDialogueRootObject && dialogueRoot != null)
            {
                dialogueRoot.SetActive(false);
            }
        }

        public void ShowUI()
        {
            if (hideDialogueRootObject && dialogueRoot != null)
            {
                dialogueRoot.SetActive(true);
            }
            if (dialogueViewComponent != null)
            {
                dialogueView = dialogueViewComponent as IDialogueView;
            }
        }

        private void RegisterManualOverride()
        {
            manualOverrideUntil = Time.time + Mathf.Max(0f, commandOverrideSeconds);
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
