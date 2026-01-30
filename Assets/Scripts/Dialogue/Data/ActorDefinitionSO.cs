using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data
{
    [Serializable]
    public class PortraitEntry
    {
        public string key;
        public Sprite sprite;
    }

    [CreateAssetMenu(fileName = "NewActorDefinition", menuName = "Dialogue/Actor")]
    public class ActorDefinitionSO : ScriptableObject
    {
        public string actorId;
        public string displayName;
        public List<PortraitEntry> portraits = new List<PortraitEntry>();
        
        public Sprite GetPortrait(string key)
        {
            var entry = portraits.Find(p => p.key == key);
            return entry?.sprite;
        }
    }
}
