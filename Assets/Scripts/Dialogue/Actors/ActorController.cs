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

        private Dictionary<string, ActorView> activeActors = new Dictionary<string, ActorView>();
        private Dictionary<string, ActorDefinitionSO> definitionsMap = new Dictionary<string, ActorDefinitionSO>();

        private void Awake()
        {
            foreach (var def in actorDefinitions)
            {
                definitionsMap[def.actorId] = def;
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
                Sprite s = def.GetPortrait(portraitKey);
                if (s != null) view.SetPortrait(s);
            }

            view.Show(fadeTime);
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
    }
}
