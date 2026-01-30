using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace DialogueSystem.Effects
{
    public class EffectSpan
    {
        public string type;
        public int startIndex;
        public int endIndex = -1;
        public float strength;
    }

    public class TextEffectController : MonoBehaviour
    {
        private TMP_Text textComponent;
        private List<EffectSpan> activeSpans = new List<EffectSpan>();
        private List<EffectSpan> openSpans = new List<EffectSpan>();

        private void Awake()
        {
            textComponent = GetComponent<TMP_Text>();
        }

        public void ClearSpans()
        {
            activeSpans.Clear();
            openSpans.Clear();
        }

        public void StartSpan(string type, int startIndex, float strength)
        {
            var span = new EffectSpan { type = type.ToLower(), startIndex = startIndex, strength = strength };
            activeSpans.Add(span);
            openSpans.Add(span);
        }

        public void EndSpan(string type, int endIndex)
        {
            string t = type.ToLower();
            var span = openSpans.FindLast(s => s.type == t);
            if (span != null)
            {
                span.endIndex = endIndex;
                openSpans.Remove(span);
            }
        }

        private void LateUpdate()
        {
            if (activeSpans.Count == 0) return;

            textComponent.ForceMeshUpdate();
            var textInfo = textComponent.textInfo;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                Vector3 offset = Vector3.zero;

                foreach (var span in activeSpans)
                {
                    if (i >= span.startIndex && (span.endIndex == -1 || i < span.endIndex))
                    {
                        offset += CalculateOffset(span, i);
                    }
                }

                if (offset != Vector3.zero)
                {
                    int materialIndex = charInfo.materialReferenceIndex;
                    int vertexIndex = charInfo.vertexIndex;
                    Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                    vertices[vertexIndex + 0] += offset;
                    vertices[vertexIndex + 1] += offset;
                    vertices[vertexIndex + 2] += offset;
                    vertices[vertexIndex + 3] += offset;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }

        private Vector3 CalculateOffset(EffectSpan span, int charIndex)
        {
            float s = span.strength > 0 ? span.strength : 1f;
            switch (span.type)
            {
                case "shake":
                    return new Vector3(Random.Range(-s, s), Random.Range(-s, s), 0);
                case "wave":
                    float wave = Mathf.Sin(Time.time * 10f + charIndex * 0.5f) * s;
                    return new Vector3(0, wave, 0);
                default:
                    return Vector3.zero;
            }
        }
    }
}
