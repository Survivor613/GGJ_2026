using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem.Data
{
    public enum ActorSlot
    {
        Left,
        Right,
        Center
    }

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
        public ActorSlot defaultSlot = ActorSlot.Left;
        public string defaultPortraitKey;
        public List<PortraitEntry> portraits = new List<PortraitEntry>();
        
        public Sprite GetPortrait(string key)
        {
            var entry = portraits.Find(p => p.key == key);
            return entry?.sprite;
        }

        public Sprite GetDefaultPortrait()
        {
            if (!string.IsNullOrEmpty(defaultPortraitKey))
            {
                var sprite = GetPortrait(defaultPortraitKey);
                if (sprite != null) return sprite;
            }
            return portraits.Count > 0 ? portraits[0].sprite : null;
        }
    }
}
