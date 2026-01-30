using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using DialogueSystem.Core;

namespace DialogueSystem.UI
{
    public class TypewriterEffect : MonoBehaviour
    {
        [SerializeField] private TMP_Text textComponent;
        [SerializeField] private float defaultSpeed = 0.05f;
        
        public bool IsPlaying { get; private set; }
        public Action OnComplete;
        
        private Coroutine typewriterCoroutine;
        private List<DialogueToken> currentTokens;
        private float currentSpeed;
        
        // Reference to effect controller
        private Effects.TextEffectController effectController;

        private void Awake()
        {
            effectController = GetComponent<Effects.TextEffectController>();
        }

        public void Play(string rawText)
        {
            Stop();
            currentTokens = DialogueParser.Parse(rawText);
            typewriterCoroutine = StartCoroutine(TypewriterRoutine());
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
            
            StringBuilder fullText = new StringBuilder();
            if (effectController != null) effectController.ClearSpans();
            
            int charIndex = 0;
            foreach (var token in currentTokens)
            {
                if (token.type == TokenType.Text)
                {
                    fullText.Append(token.value);
                    charIndex += token.value.Length;
                }
                else if (token.type == TokenType.EffectStart)
                {
                    if (effectController != null) effectController.StartSpan(token.value, charIndex, token.floatValue);
                }
                else if (token.type == TokenType.EffectEnd)
                {
                    if (effectController != null) effectController.EndSpan(token.value, charIndex);
                }
            }
            
            textComponent.text = fullText.ToString();
            textComponent.maxVisibleCharacters = 9999;
            IsPlaying = false;
            OnComplete?.Invoke();
        }

        private IEnumerator TypewriterRoutine()
        {
            IsPlaying = true;
            textComponent.text = "";
            textComponent.maxVisibleCharacters = 0;
            currentSpeed = defaultSpeed;
            
            StringBuilder displayedText = new StringBuilder();
            if (effectController != null) effectController.ClearSpans();
            
            int charCounter = 0;
            
            foreach (var token in currentTokens)
            {
                switch (token.type)
                {
                    case TokenType.Text:
                        foreach (char c in token.value)
                        {
                            displayedText.Append(c);
                            textComponent.text = displayedText.ToString();
                            charCounter++;
                            textComponent.maxVisibleCharacters = charCounter;
                            
                            // Only wait if it's not a whitespace (optional UX choice)
                            yield return new WaitForSeconds(currentSpeed);
                        }
                        break;
                        
                    case TokenType.Pause:
                        yield return new WaitForSeconds(token.floatValue);
                        break;
                        
                    case TokenType.Speed:
                        currentSpeed = token.floatValue > 0 ? token.floatValue : defaultSpeed;
                        break;
                        
                    case TokenType.Sound:
                        // Call AudioManager if exists
                        if (AudioManager.instance != null) AudioManager.instance.PlayGlobalSFX(token.value);
                        break;
                        
                    case TokenType.EffectStart:
                        if (effectController != null) effectController.StartSpan(token.value, charCounter, token.floatValue);
                        break;
                        
                    case TokenType.EffectEnd:
                        if (effectController != null) effectController.EndSpan(token.value, charCounter);
                        break;
                }
            }
            
            IsPlaying = false;
            OnComplete?.Invoke();
            typewriterCoroutine = null;
        }
    }
}
