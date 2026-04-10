using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// 会話ウィンドウのUI表示を担当するコンポーネント。
    ///
    /// 仕様書セクション5「会話画面」準拠:
    /// - テキストウィンドウ（下部）に名前＋本文を表示
    /// - タイプライター演出
    /// - ▼送りマーク（テキスト表示完了時に点滅）
    /// - オートモード（特殊ボタンでトグル）
    /// - キャンセルボタン押しっぱなしで早送り
    /// - テーマ対応（UIThemeSO）
    /// </summary>
    public class DialogueUI : MonoBehaviour
    {
        // === Inspectorで接続する変数 ===
        [Header("UI要素")]
        [SerializeField] private GameObject dialoguePanel;       // ダイアログパネル全体
        [SerializeField] private TextMeshProUGUI NameText;       // 名前を表示するテキスト
        [SerializeField] private TextMeshProUGUI MessageText;    // 話の内容を表示するテキスト

        [Header("送りマーク")]
        [SerializeField] private AdvanceIndicator _advanceIndicator;  // ▼送りマーク

        [Header("テーマ（任意）")]
        [SerializeField] private UIThemeSO _theme;

        [Header("タイプライター設定")]
        [SerializeField] private float typingSpeed = 0.05f;          // 通常速度（1文字あたり秒）
        [SerializeField] private float fastTypingSpeed = 0.01f;      // 早送り速度

        [Header("オートモード設定")]
        [SerializeField] private float autoWaitTime = 1.5f;          // 表示完了後の自動送り待機時間

        // === イベント ===
        /// <summary>会話が全部終わったことを通知するイベント</summary>
        public event Action OnDialogueComplete;

        /// <summary>オートモードの状態が変わったときに通知</summary>
        public event Action<bool> OnAutoModeChanged;

        // === 内部変数 ===
        private Coroutine _typingCoroutine;
        private bool _isTyping = false;
        private string _currentMessage;        // SkipTyping用に全文を保持

        // --- 複数セリフ管理 ---
        private DialogueLine[] _lines;
        private int _currentLineIndex = 0;
        private bool _isActive = false;

        // --- オートモード ---
        private bool _isAutoMode = false;
        private Coroutine _autoAdvanceCoroutine;

        /// <summary>オートモードON/OFF</summary>
        public bool IsAutoMode => _isAutoMode;

        /// <summary>タイプライタ演出が完了しているか</summary>
        public bool IsTypingComplete => !_isTyping;

        /// <summary>会話中かどうか</summary>
        public bool IsActive => _isActive;

        private void Awake()
        {
            AutoResolveReferences();
            ApplyTheme();

            // dialoguePanelがあれば非表示にする（nullなら何もしない）
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        /// <summary>Inspector未設定のフィールドを子オブジェクトから自動取得する</summary>
        private void AutoResolveReferences()
        {
            // dialoguePanelが未設定 → 自分自身のGameObjectをパネルとして使う
            if (dialoguePanel == null)
                dialoguePanel = gameObject;

            // TextMeshProUGUI を子から探す
            if (NameText == null || MessageText == null)
            {
                var tmps = GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
                foreach (var tmp in tmps)
                {
                    if (NameText == null && tmp.gameObject.name.Contains("Name"))
                        NameText = tmp;
                    else if (MessageText == null && tmp.gameObject.name.Contains("Message"))
                        MessageText = tmp;
                }
            }

            // AdvanceIndicator を子から探す
            if (_advanceIndicator == null)
                _advanceIndicator = GetComponentInChildren<AdvanceIndicator>(true);
        }

        // ============================================================
        // テーマ適用
        // ============================================================
        private void ApplyTheme()
        {
            if (_theme == null) return;

            if (NameText != null)
                NameText.color = _theme.textHighlight;
            if (MessageText != null)
                MessageText.color = _theme.textNormal;
        }

        /// <summary>テーマを差し替えて適用する</summary>
        public void ApplyTheme(UIThemeSO newTheme)
        {
            _theme = newTheme;
            ApplyTheme();
        }

        // ============================================================
        // 複数セリフを開始する（既存API互換）
        // ============================================================
        public void StartDialogue(DialogueLine[] lines)
        {
            if (lines == null || lines.Length == 0) return;

            _lines = lines;
            _currentLineIndex = 0;
            _isActive = true;
            ShowCurrentLine();
        }

        // ============================================================
        // 今のインデックスのセリフを画面に表示する
        // ============================================================
        private void ShowCurrentLine()
        {
            DialogueLine line = _lines[_currentLineIndex];
            _currentMessage = line.Message;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (NameText != null) NameText.text = line.SpeakerName;
            HideAdvanceIndicator();

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeMessage(line.Message));
        }

        // ============================================================
        // 次のセリフに進む。最後なら会話を終了する
        // ============================================================
        private void ShowNextLine()
        {
            _currentLineIndex++;

            if (_currentLineIndex < _lines.Length)
            {
                ShowCurrentLine();
            }
            else
            {
                EndDialogue();
            }
        }

        // ============================================================
        // 会話を終了してパネルを閉じる
        // ============================================================
        private void EndDialogue()
        {
            _isActive = false;
            HideDialogue();
            OnDialogueComplete?.Invoke();
        }

        // === 1セリフだけ表示する（後方互換） ===
        public void ShowDialogue(string speakerName, string message)
        {
            StartDialogue(new DialogueLine[]
            {
                new DialogueLine(speakerName, message)
            });
        }

        // === DialogueUIBridgeから1行ずつ表示するためのメソッド ===
        public void ShowLine(string speakerName, string message)
        {
            _isActive = true;
            _currentMessage = message;
            if (dialoguePanel != null) dialoguePanel.SetActive(true);
            if (NameText != null) NameText.text = speakerName;
            HideAdvanceIndicator();

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeMessage(message));
        }

        /// <summary>タイプライタ演出をスキップして全文を即表示する。</summary>
        public void SkipTyping()
        {
            if (!_isTyping) return;

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            if (!string.IsNullOrEmpty(_currentMessage))
                MessageText.text = _currentMessage;

            _isTyping = false;
            ShowAdvanceIndicator();
            StartAutoAdvanceIfNeeded();
        }

        public void HideDialogue()
        {
            _isActive = false;
            StopAutoAdvance();
            HideAdvanceIndicator();
            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);
        }

        // ============================================================
        // タイプライター演出
        // ============================================================
        private IEnumerator TypeMessage(string message)
        {
            _isTyping = true;
            MessageText.text = "";
            HideAdvanceIndicator();

            foreach (char c in message)
            {
                MessageText.text += c;

                // キャンセルボタン（Escapeキー）押しっぱなしで早送り
                float speed = IsHoldingCancel() ? fastTypingSpeed : typingSpeed;
                yield return new WaitForSeconds(speed);
            }

            _isTyping = false;
            ShowAdvanceIndicator();
            StartAutoAdvanceIfNeeded();
        }

        // ============================================================
        // 送りマーク制御
        // ============================================================
        private void ShowAdvanceIndicator()
        {
            if (_advanceIndicator != null)
                _advanceIndicator.Show();
        }

        private void HideAdvanceIndicator()
        {
            if (_advanceIndicator != null)
                _advanceIndicator.Hide();
        }

        // ============================================================
        // オートモード
        // ============================================================
        /// <summary>オートモードをトグルする</summary>
        public void ToggleAutoMode()
        {
            _isAutoMode = !_isAutoMode;
            OnAutoModeChanged?.Invoke(_isAutoMode);

            if (_isAutoMode && !_isTyping && _isActive)
            {
                StartAutoAdvanceIfNeeded();
            }
            else
            {
                StopAutoAdvance();
            }
        }

        private void StartAutoAdvanceIfNeeded()
        {
            if (!_isAutoMode || _isTyping) return;

            StopAutoAdvance();
            _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay());
        }

        private void StopAutoAdvance()
        {
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }
        }

        private IEnumerator AutoAdvanceAfterDelay()
        {
            yield return new WaitForSeconds(autoWaitTime);

            // 自動送り: Bridge経由の場合は_linesがnull
            if (_lines != null)
            {
                ShowNextLine();
            }
            // Bridge経由の場合は外部（Bridge）が Advance() を呼ぶので
            // ここではイベント通知のみ
        }

        // ============================================================
        // キャンセルボタン（早送り）判定
        // ============================================================
        private bool IsHoldingCancel()
        {
            return Keyboard.current != null && Keyboard.current.escapeKey.isPressed;
        }

        // ============================================================
        // クリック/キー入力の処理
        // ============================================================
        private void Update()
        {
            if (!_isActive) return;
            // Bridge経由(ShowLine)の場合は_linesがnull → Bridge側で入力を処理するためここではスキップ
            if (_lines == null) return;

            // オートモード切替: Aキー
            if (Keyboard.current != null && Keyboard.current.aKey.wasPressedThisFrame)
            {
                ToggleAutoMode();
                return;
            }

            // オートモード中は手動送りしない（クリックでオートを解除する仕様にもできる）
            if (_isAutoMode) return;

            bool inputPressed =
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

            if (!inputPressed) return;

            if (_isTyping)
            {
                SkipTyping();
            }
            else
            {
                ShowNextLine();
            }
        }
    }
}
