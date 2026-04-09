using System;
using GuildAcademy.Core.Branch;

namespace GuildAcademy.Core.Dialogue
{
    public class DialogueManager
    {
        private readonly DialogueParser _parser;
        private readonly DialogueRunner _runner;
        private readonly IDialogueSource _source;

        public DialogueEntry Current => _runner.Current;
        public bool IsActive => _runner.IsActive;
        public bool HasChoices => _runner.HasChoices;

        public event Action<DialogueEntry> OnDialogueAdvanced
        {
            add => _runner.OnDialogueAdvanced += value;
            remove => _runner.OnDialogueAdvanced -= value;
        }

        public event Action OnDialogueEnded
        {
            add => _runner.OnDialogueEnded += value;
            remove => _runner.OnDialogueEnded -= value;
        }

        public event Action<System.Collections.Generic.List<DialogueChoice>> OnChoicesPresented
        {
            add => _runner.OnChoicesPresented += value;
            remove => _runner.OnChoicesPresented -= value;
        }

        public DialogueManager(IDialogueSource source, FlagSystem flags, TrustSystem trust)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _parser = new DialogueParser();
            _runner = new DialogueRunner(_parser, flags, trust);
        }

        public void LoadFromSource(string sourceKey)
        {
            var entries = _source.LoadEntries(sourceKey);
            _parser.Load(entries);
        }

        public void Start(string entryId)
        {
            if (_parser.EntryCount == 0)
                throw new InvalidOperationException("Dialogue data is not loaded. Call LoadFromSource first.");

            _runner.Start(entryId);
        }

        public void Advance() => _runner.Advance();

        public void SelectChoice(int choiceIndex) => _runner.SelectChoice(choiceIndex);

        public void End() => _runner.End();
    }
}
