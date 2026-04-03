using System;
using System.Collections.Generic;
using GuildAcademy.Core.Branch;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Dialogue
{
    public class DialogueRunner
    {
        private readonly DialogueParser _parser;
        private readonly FlagSystem _flags;
        private readonly TrustSystem _trust;
        private DialogueEntry _current;

        public DialogueEntry Current => _current;
        public bool IsActive => _current != null;
        public bool HasChoices => _current?.Choices != null && _current.Choices.Count > 0;

        public event Action<DialogueEntry> OnDialogueAdvanced;
        public event Action OnDialogueEnded;
        public event Action<List<DialogueChoice>> OnChoicesPresented;

        public DialogueRunner(DialogueParser parser, FlagSystem flags, TrustSystem trust)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _flags = flags;
            _trust = trust;
        }

        public void Start(string entryId)
        {
            _current = _parser.GetById(entryId);
            OnDialogueAdvanced?.Invoke(_current);

            if (HasChoices)
                OnChoicesPresented?.Invoke(_current.Choices);
        }

        public void Advance()
        {
            if (_current == null) return;
            if (HasChoices) return;

            if (string.IsNullOrEmpty(_current.Next))
            {
                End();
                return;
            }

            _current = _parser.GetById(_current.Next);
            OnDialogueAdvanced?.Invoke(_current);

            if (HasChoices)
                OnChoicesPresented?.Invoke(_current.Choices);
        }

        public void SelectChoice(int index)
        {
            if (_current == null)
                throw new InvalidOperationException("No active dialogue.");

            if (_current.Choices == null || index < 0 || index >= _current.Choices.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var choice = _current.Choices[index];

            if (_flags != null && !string.IsNullOrEmpty(choice.Flag))
                _flags.Set(choice.Flag, true);

            if (_trust != null && choice.TrustEffects != null)
            {
                foreach (var effect in choice.TrustEffects)
                {
                    if (Enum.TryParse<CharacterId>(effect.Key, out var charId))
                        _trust.AddTrust(charId, effect.Value);
                }
            }

            if (string.IsNullOrEmpty(choice.Next))
            {
                End();
                return;
            }

            _current = _parser.GetById(choice.Next);
            OnDialogueAdvanced?.Invoke(_current);

            if (HasChoices)
                OnChoicesPresented?.Invoke(_current.Choices);
        }

        public void End()
        {
            _current = null;
            OnDialogueEnded?.Invoke();
        }
    }
}
