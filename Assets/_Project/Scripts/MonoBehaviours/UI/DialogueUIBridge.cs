using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.Core.Branch;
using GuildAcademy.MonoBehaviours.Branch;
using GuildAcademy.MonoBehaviours.Dialogue;

namespace GuildAcademy.UI
{
    /// <summary>
    /// DialogueManager（ロジック）と各UI（表示）をつなぐブリッジ。
    ///
    /// 仕様書セクション5「会話画面」準拠:
    /// - 会話テキスト表示（DialogueUI）
    /// - 選択肢表示（ChoiceUI）
    /// - 立ち絵制御（PortraitController）
    /// - 入力: 決定ボタン（テキスト送り/選択肢決定）
    ///         キャンセルボタン（テキスト早送り → DialogueUI側で処理）
    ///         上下キー（選択肢カーソル移動 → ChoiceUI側で処理）
    ///         特殊ボタン（オートモード切替）
    /// </summary>
    public class DialogueUIBridge : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private DialogueUI _dialogueUI;
        [SerializeField] private ChoiceUI _choiceUI;
        [SerializeField] private PortraitController _portraitController;

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

            // 同じオブジェクト or 子オブジェクトから自動取得（未設定の場合）
            if (_dialogueUI == null)
                _dialogueUI = GetComponentInChildren<DialogueUI>();
            if (_choiceUI == null)
                _choiceUI = GetComponentInChildren<ChoiceUI>();
            if (_portraitController == null)
                _portraitController = GetComponentInChildren<PortraitController>();
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

        // ============================================================
        // 会話開始API
        // ============================================================

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

        // ============================================================
        // DialogueManager イベントハンドラ
        // ============================================================

        private void HandleDialogueAdvanced(DialogueEntry entry)
        {
            _waitingForChoice = false;
            if (_choiceUI != null) _choiceUI.Hide();

            if (_dialogueUI == null)
            {
                Debug.LogError("[DialogueUIBridge] DialogueUI not assigned.");
                return;
            }
            _dialogueUI.ShowLine(entry.Speaker, entry.Text);

            // --- 立ち絵制御 ---
            UpdatePortrait(entry.Speaker, entry.Emotion, entry.PortraitPosition);
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
            if (_dialogueUI != null) _dialogueUI.HideDialogue();
            if (_portraitController != null) _portraitController.HideAll();
        }

        private void OnChoiceSelected(int index)
        {
            _waitingForChoice = false;
            if (_choiceUI != null) _choiceUI.Hide();
            _dialogueManager.SelectChoice(index);
        }

        // ============================================================
        // 立ち絵制御
        // ============================================================

        /// <summary>
        /// 話者に応じて立ち絵を表示・ハイライトする。
        /// 既に表示中のキャラはそのまま（感情更新あり）、新キャラは空いてる側に配置。
        /// </summary>
        private void UpdatePortrait(string speaker, string emotion = "normal", string explicitPosition = null)
        {
            if (_portraitController == null) return;
            if (string.IsNullOrEmpty(speaker)) return;

            string position;

            if (_portraitController.IsDisplayed(speaker))
            {
                // 既に表示中 → 感情差分だけ更新してハイライト
                position = _portraitController.GetPosition(speaker);
                _portraitController.SetPortrait(position, speaker, emotion ?? "normal");
                _portraitController.HighlightSpeaker(speaker);
                return;
            }

            // 明示的な位置指定がある場合
            if (!string.IsNullOrEmpty(explicitPosition))
            {
                position = explicitPosition;
            }
            // 空いてる側に配置（左優先）
            else if (_portraitController.IsLeftEmpty)
            {
                position = "left";
            }
            else if (_portraitController.IsRightEmpty)
            {
                position = "right";
            }
            else
            {
                // 両方埋まっている場合: 右を入れ替え
                position = "right";
            }

            _portraitController.SetPortrait(position, speaker, emotion ?? "normal");
            _portraitController.HighlightSpeaker(speaker);
        }

        // ============================================================
        // 入力処理
        // ============================================================

        private void Update()
        {
            if (_dialogueManager == null || !_dialogueManager.IsActive) return;

            // 選択肢待ち中はChoiceUI側で入力処理
            if (_waitingForChoice) return;

            // 特殊ボタン: オートモード切替（Aキー）
            if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            {
                if (_dialogueUI != null)
                    _dialogueUI.ToggleAutoMode();
                return;
            }

            // 決定ボタン: テキスト送り
            bool advanceInput =
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

            if (advanceInput)
            {
                OnAdvanceInput();
            }
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

        // ============================================================
        // クリーンアップ
        // ============================================================

        private void OnDestroy()
        {
            if (_dialogueManager != null)
            {
                _dialogueManager.OnDialogueAdvanced -= HandleDialogueAdvanced;
                _dialogueManager.OnChoicesPresented -= HandleChoicesPresented;
                _dialogueManager.OnDialogueEnded -= HandleDialogueEnded;
            }
            if (Instance == this) Instance = null;
        }
    }
}
