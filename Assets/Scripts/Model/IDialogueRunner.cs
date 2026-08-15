namespace Model
{
    public interface IDialogueRunner
    {
        public bool IsDialogueRunning { get; }
        public void StartDialogue(string nodeName);
    }
}