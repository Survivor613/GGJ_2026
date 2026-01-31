using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace DialogueSystem.UI
{
    /// <summary>
    /// 通用打字机效果 - 支持 Unity Text
    /// </summary>
    public class TypewriterEffectUniversal : MonoBehaviour
    {
        [SerializeField] private Text textComponent;
        [SerializeField] private float defaultSpeed = 0.05f;
        
        public bool IsPlaying { get; private set; }
        public Action OnComplete;
        
        private Coroutine typewriterCoroutine;
        private float currentSpeed;
        private string currentRawText;
        
        // Simple tag regex
        private static readonly Regex tagRegex = new Regex(@"\[(/?[a-zA-Z]+)(?:=([^\]]+))?\]");

        public void Play(string rawText)
        {
            Stop();
            currentRawText = rawText;
            typewriterCoroutine = StartCoroutine(TypewriterRoutine(rawText));
        }

        public void Stop()
        {
            if (typewriterCoroutine != null)
            {
                StopCoroutine(typewriterCoroutine);
                typewriterCoroutine = null;
            }
            IsPlaying = false;
        }

        public void Skip()
        {
            if (!IsPlaying) return;
            
            Stop();
            
            // Remove control tags but keep color tags
            string cleanText = tagRegex.Replace(currentRawText ?? string.Empty, match =>
            {
                string tagName = match.Groups[1].Value.ToLower();
                // Keep color tags for Unity Text
                if (tagName.StartsWith("color") || tagName == "/color")
                {
                    return match.Value.Replace("[", "<").Replace("]", ">");
                }
                return "";
            });
            
            textComponent.text = cleanText;
            IsPlaying = false;
            OnComplete?.Invoke();
        }

        private IEnumerator TypewriterRoutine(string rawText)
        {
            IsPlaying = true;
            textComponent.text = "";
            currentSpeed = defaultSpeed;
            
            StringBuilder displayedText = new StringBuilder();
            int charIndex = 0;
            
            foreach (Match match in tagRegex.Matches(rawText))
            {
                // Text before tag
                if (match.Index > charIndex)
                {
                    string textSegment = rawText.Substring(charIndex, match.Index - charIndex);
                    foreach (char c in textSegment)
                    {
                        displayedText.Append(c);
                        textComponent.text = displayedText.ToString();
                        yield return new WaitForSeconds(currentSpeed);
                    }
                }
                
                // Process tag
                string tagName = match.Groups[1].Value.ToLower();
                string tagValue = match.Groups[2].Value;
                
                if (tagName == "pause")
                {
                    float.TryParse(tagValue, out float pauseTime);
                    yield return new WaitForSeconds(pauseTime);
                }
                else if (tagName == "spd")
                {
                    float.TryParse(tagValue, out float speed);
                    currentSpeed = speed > 0 ? speed : defaultSpeed;
                }
                else if (tagName == "sfx")
                {
                    if (AudioManager.instance != null) AudioManager.instance.PlayGlobalSFX(tagValue);
                }
                else if (tagName.StartsWith("color") || tagName == "/color")
                {
                    // Convert to Unity rich text format
                    string richTag = match.Value.Replace("[", "<").Replace("]", ">");
                    displayedText.Append(richTag);
                    textComponent.text = displayedText.ToString();
                }
                // Ignore other tags (shake, wave not supported by Unity Text)
                
                charIndex = match.Index + match.Length;
            }
            
            // Remaining text
            if (charIndex < rawText.Length)
            {
                string remaining = rawText.Substring(charIndex);
                foreach (char c in remaining)
                {
                    displayedText.Append(c);
                    textComponent.text = displayedText.ToString();
                    yield return new WaitForSeconds(currentSpeed);
                }
            }
            
            IsPlaying = false;
            OnComplete?.Invoke();
            typewriterCoroutine = null;
        }
    }
}
