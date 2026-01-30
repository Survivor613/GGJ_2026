namespace DialogueSystem.UI
{
    /// <summary>
    /// 对话视图接口 - 同时支持 TMP 和原生 Text 版本
    /// </summary>
    public interface IDialogueView
    {
        bool IsTypewriterPlaying { get; }
        void ShowLine(string speaker, string text);
        void SkipTypewriter();
        void ShowContinueIcon(bool show);
        void Hide();
    }
}
