using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data
{
    [Serializable]
    public abstract class DialogueNode
    {
    }

    [Serializable]
    public class LineNode : DialogueNode
    {
        public string speakerId;
        public string speakerName;
        [TextArea(3, 10)]
        public string text;
        public string voiceKey;
        public string expressionKey;
    }

    [Serializable]
    public class CommandNode : DialogueNode
    {
        public string command; // e.g., "actor show id=alice"
    }

    [CreateAssetMenu(fileName = "NewDialogueScript", menuName = "Dialogue/Script")]
    public class DialogueScriptSO : ScriptableObject
    {
        [SerializeReference]
        public List<DialogueNode> nodes = new List<DialogueNode>();
    }
}
