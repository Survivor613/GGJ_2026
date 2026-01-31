using System.Collections.Generic;
using UnityEngine;
using DialogueSystem.Data;

namespace DialogueSystem.Actors
{
    public class ActorController : MonoBehaviour
    {
        [SerializeField] private GameObject actorPrefab;
        [SerializeField] private Transform actorLayer;
        [SerializeField] private List<ActorDefinitionSO> actorDefinitions;
        [Header("Slot Positions")]
        [SerializeField] private Vector2 leftSlot = new Vector2(-300, 0);
        [SerializeField] private Vector2 rightSlot = new Vector2(300, 0);
        [SerializeField] private Vector2 centerSlot = new Vector2(0, 0);

        private Dictionary<string, ActorView> activeActors = new Dictionary<string, ActorView>();
        private Dictionary<string, ActorDefinitionSO> definitionsMap = new Dictionary<string, ActorDefinitionSO>();
        private Dictionary<string, string> displayNameToId = new Dictionary<string, string>();

        private void Awake()
        {
            foreach (var def in actorDefinitions)
            {
                definitionsMap[def.actorId] = def;
                if (!string.IsNullOrWhiteSpace(def.displayName))
                {
                    displayNameToId[def.displayName] = def.actorId;
                }
            }
        }

        public void ShowActor(string actorId, string portraitKey, Vector2 position, float fadeTime = 0.2f)
        {
            if (!activeActors.TryGetValue(actorId, out ActorView view))
            {
                var go = Instantiate(actorPrefab, actorLayer);
                view = go.GetComponent<ActorView>();
                view.Init(actorId);
                activeActors[actorId] = view;
            }

            view.gameObject.SetActive(true);
            view.transform.localPosition = new Vector3(position.x, position.y, 0);
            
            if (definitionsMap.TryGetValue(actorId, out var def))
            {
                Sprite s = string.IsNullOrEmpty(portraitKey) ? def.GetDefaultPortrait() : def.GetPortrait(portraitKey);
                if (s != null) view.SetPortrait(s);
            }

            view.Show(fadeTime);
        }

        public bool IsActorShown(string actorId)
        {
            return activeActors.TryGetValue(actorId, out ActorView view) && view.gameObject.activeSelf;
        }

        public void ShowActorAuto(string actorId, float fadeTime = 0.2f)
        {
            if (!definitionsMap.TryGetValue(actorId, out var def))
            {
                return;
            }

            Vector2 position = GetSlotPosition(def.defaultSlot);
            string portraitKey = def.defaultPortraitKey;
            ShowActor(actorId, portraitKey, position, fadeTime);
        }

        public void HideActor(string actorId, float fadeTime = 0.2f)
        {
            if (activeActors.TryGetValue(actorId, out ActorView view))
            {
                view.Hide(fadeTime);
            }
        }

        public void SetFocus(string focusedActorId, float duration = 0.2f)
        {
            foreach (var kvp in activeActors)
            {
                kvp.Value.SetFocus(kvp.Key == focusedActorId, duration);
            }
        }
        
        public string GetDisplayName(string actorId)
        {
            if (definitionsMap.TryGetValue(actorId, out var def)) return def.displayName;
            return actorId;
        }

        public string GetActorIdByDisplayName(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName)) return string.Empty;
            return displayNameToId.TryGetValue(displayName, out var id) ? id : string.Empty;
        }

        private Vector2 GetSlotPosition(ActorSlot slot)
        {
            switch (slot)
            {
                case ActorSlot.Left:
                    return leftSlot;
                case ActorSlot.Right:
                    return rightSlot;
                case ActorSlot.Center:
                default:
                    return centerSlot;
            }
        }
    }
}
