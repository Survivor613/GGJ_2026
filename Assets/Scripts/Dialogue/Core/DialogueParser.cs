using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace DialogueSystem.Core
{
    public enum TokenType
    {
        Text,
        Pause,
        Speed,
        EffectStart,
        EffectEnd,
        Sound
    }

    public class DialogueToken
    {
        public TokenType type;
        public string value;
        public float floatValue;
    }

    public static class DialogueParser
    {
        // Simple tag format: [tag=value] or [tag] or [/tag]
        private static readonly Regex tagRegex = new Regex(@"\[(/?[a-zA-Z]+)(?:=([^\]]+))?\]");

        public static List<DialogueToken> Parse(string text)
        {
            var tokens = new List<DialogueToken>();
            int lastIndex = 0;

            foreach (Match match in tagRegex.Matches(text))
            {
                // Text before the tag
                if (match.Index > lastIndex)
                {
                    tokens.Add(new DialogueToken 
                    { 
                        type = TokenType.Text, 
                        value = text.Substring(lastIndex, match.Index - lastIndex) 
                    });
                }

                string tagName = match.Groups[1].Value;
                string tagValue = match.Groups[2].Value;

                if (tagName.StartsWith("/"))
                {
                    tokens.Add(new DialogueToken 
                    { 
                        type = TokenType.EffectEnd, 
                        value = tagName.Substring(1) 
                    });
                }
                else
                {
                    switch (tagName.ToLower())
                    {
                        case "pause":
                            float.TryParse(tagValue, out float p);
                            tokens.Add(new DialogueToken { type = TokenType.Pause, floatValue = p });
                            break;
                        case "spd":
                            float.TryParse(tagValue, out float s);
                            tokens.Add(new DialogueToken { type = TokenType.Speed, floatValue = s });
                            break;
                        case "sfx":
                            tokens.Add(new DialogueToken { type = TokenType.Sound, value = tagValue });
                            break;
                        default:
                            tokens.Add(new DialogueToken { type = TokenType.EffectStart, value = tagName, floatValue = ParseFloat(tagValue) });
                            break;
                    }
                }

                lastIndex = match.Index + match.Length;
            }

            if (lastIndex < text.Length)
            {
                tokens.Add(new DialogueToken 
                { 
                    type = TokenType.Text, 
                    value = text.Substring(lastIndex) 
                });
            }

            return tokens;
        }

        private static float ParseFloat(string val)
        {
            float.TryParse(val, out float res);
            return res;
        }
    }
}
