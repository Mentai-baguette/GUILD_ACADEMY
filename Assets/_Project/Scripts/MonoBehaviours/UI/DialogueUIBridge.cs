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

        /// <summary>
        /// BranchManagerが存在する場合のみDialogueManagerを初期化する。
        /// BranchManager不在では分岐・フラグ・信頼値が機能しないため、会話開始を許可しない。
        /// </summary>
        private bool EnsureInitialized()
        {
            if (_dialogueManager != null) return true;

            var branchManager = BranchManager.Instance;
            if (branchManager == null)
            {
                Debug.LogError("[DialogueUIBridge] BranchManager not found. Cannot start dialogue without flag/trust systems.");
                return false;
            }

            var source = new ResourcesDialogueJsonLoader();
            _dialogueManager = new DialogueManager(
                source, branchManager.Service.Flags, branchManager.Service.Trust);
            _dialogueManager.OnDialogueAdvanced += HandleDialogueAdvanced;
            _dialogueManager.OnChoicesPresented += HandleChoicesPresented;
            _dialogueManager.OnDialogueEnded += HandleDialogueEnded;
            return true;
        }

        public void StartDialogue(string sourceKey, string entryId)
        {
            if (!EnsureInitialized()) return;

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

            // タイプライタ演出中はスキップ（全文即表示）、完了後にAdvance
            if (_dialogueUI != null && !_dialogueUI.IsTypingComplete)
            {
                _dialogueUI.SkipTyping();
                return;
            }

            _dialogueManager.Advance();
        }
    }
}
