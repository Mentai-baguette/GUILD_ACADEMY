using System.Collections.Generic;
using UnityEngine;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.Core.Branch;
using GuildAcademy.MonoBehaviours.Branch;
using GuildAcademy.MonoBehaviours.Dialogue;

namespace GuildAcademy.UI
{
    public class DialogueUIBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private ChoiceUI _choiceUI;

        [Header("Dialogue Source")]
        [SerializeField] private string _defaultSourceKey = "Dialogues/chapter1_dialogue";

        private DialogueManager _dialogueManager;
        private bool _waitingForChoice;

        public static DialogueUIBridge Instance { get; private set; }
        public bool IsActive => _dialogueManager != null && _dialogueManager.IsActive;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeManager();
        }

        private void InitializeManager()
        {
            var source = new ResourcesDialogueJsonLoader();
            var branchManager = BranchManager.Instance;

            FlagSystem flags = null;
            TrustSystem trust = null;
            if (branchManager != null)
            {
                flags = branchManager.Service.Flags;
                trust = branchManager.Service.Trust;
            }

            _dialogueManager = new DialogueManager(source, flags, trust);
            _dialogueManager.OnDialogueAdvanced += HandleDialogueAdvanced;
            _dialogueManager.OnChoicesPresented += HandleChoicesPresented;
            _dialogueManager.OnDialogueEnded += HandleDialogueEnded;
        }

        public void StartDialogue(string sourceKey, string entryId)
        {
            if (_dialogueManager == null) InitializeManager();

            _dialogueManager.LoadFromSource(sourceKey);
            _dialogueManager.Start(entryId);
        }

        public void StartDialogueFromDefault(string entryId)
        {
            StartDialogue(_defaultSourceKey, entryId);
        }

        private void HandleDialogueAdvanced(DialogueEntry entry)
        {
            _waitingForChoice = false;
            if (_choiceUI != null) _choiceUI.Hide();

            _dialogueUI.ShowLine(entry.Speaker, entry.Text);
        }

        private void HandleChoicesPresented(List<DialogueChoice> choices)
        {
            _waitingForChoice = true;

            if (_choiceUI != null)
            {
                _choiceUI.Show(choices, OnChoiceSelected);
            }
            else
            {
                Debug.LogWarning("[DialogueUIBridge] ChoiceUI not assigned. Auto-selecting first choice.");
                OnChoiceSelected(0);
            }
        }

        private void HandleDialogueEnded()
        {
            _waitingForChoice = false;
            if (_choiceUI != null) _choiceUI.Hide();
            _dialogueUI.HideDialogue();
        }

        private void OnChoiceSelected(int index)
        {
            _waitingForChoice = false;
            if (_choiceUI != null) _choiceUI.Hide();
            _dialogueManager.SelectChoice(index);
        }

        public void OnAdvanceInput()
        {
            if (_dialogueManager == null || !_dialogueManager.IsActive) return;
            if (_waitingForChoice) return;

            _dialogueManager.Advance();
        }
    }
}
