using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System;
using System.Collections;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.UI
{
    public class DialogueUI : MonoBehaviour
    {
        // === Inspectorで接続する変数 ===
        [SerializeField] private GameObject dialoguePanel; // ダイアログパネル
        [SerializeField] private TextMeshProUGUI NameText; // 名前を表示するテキスト
        [SerializeField] private TextMeshProUGUI MessageText; // 話の内容を表示するテキスト
        [SerializeField] private float typingSpeed = 0.05f; // タイピングの速度(1文字あたり何秒待つか)

        // === 会話が全部終わったことを通知するイベント ===
        // 他のスクリプトが「会話終わった？」を知るために使う
        // 使い方: dialogueUI.OnDialogueComplete += () => { 次の処理; };
        public event Action OnDialogueComplete;

        // === 内部変数 ===
        private Coroutine _typingCoroutine; // タイピングのコルーチンを覚えておく箱
        private bool _isTyping = false; // タイピング中かどうか
        private string _currentMessage;    // SkipTyping用に全文を保持

        // --- 複数セリフ管理 ---
        private DialogueLine[] _lines;     // セリフ配列（全部のセリフを保持）
        private int _currentLineIndex = 0; // 今何番目のセリフか（0から始まる）
        private bool _isActive = false;    // 会話中かどうか

        private void Awake()
        {
            HideDialogue(); // ダイアログパネルを最初は非表示にする
        }

        // ============================================================
        // 複数セリフを開始する
        // ============================================================
        // 使い方:
        //   DialogueLine[] lines = new DialogueLine[]
        //   {
        //       new DialogueLine("先生", "アカデミーに入学するぞ"),
        //       new DialogueLine("先生", "準備はいいか？"),
        //       new DialogueLine("レイ", "…はい"),
        //   };
        //   dialogueUI.StartDialogue(lines);
        // ============================================================
        public void StartDialogue(DialogueLine[] lines)
        {
            if (lines == null || lines.Length == 0) return; // セリフがなければ何もしない

            _lines = lines;            // セリフ配列を保存
            _currentLineIndex = 0;     // 最初のセリフから
            _isActive = true;          // 会話中フラグON
            ShowCurrentLine();         // 最初の1行目を表示
        }

        // ============================================================
        // 今のインデックスのセリフを画面に表示する
        // ============================================================
        private void ShowCurrentLine()
        {
            DialogueLine line = _lines[_currentLineIndex]; // 今のセリフを取り出す
            _currentMessage = line.Message;
            dialoguePanel.SetActive(true);
            NameText.text = line.SpeakerName;

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
            }
            _typingCoroutine = StartCoroutine(TypeMessage(line.Message));
        }

        // ============================================================
        // 次のセリフに進む。最後なら会話を終了する
        // ============================================================
        private void ShowNextLine()
        {
            _currentLineIndex++; // インデックスを1つ進める

            if (_currentLineIndex < _lines.Length)
            {
                // まだセリフが残っている → 次を表示
                ShowCurrentLine();
            }
            else
            {
                // 全セリフ表示済み → 会話終了
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
            OnDialogueComplete?.Invoke(); // イベント発火（聞いてる人がいれば通知）
        }

        // === 1セリフだけ表示する（後方互換。既存の呼び出しが壊れないように残す） ===
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
            dialoguePanel.SetActive(true);
            NameText.text = speakerName;

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeMessage(message));
        }

        public bool IsTypingComplete => !_isTyping;

        /// <summary>
        /// タイプライタ演出をスキップして全文を即表示する。
        /// </summary>
        public void SkipTyping()
        {
            if (!_isTyping) return;

            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            // ShowLine経由の場合、_linesがnullの可能性があるので直接MessageTextを使う
            // TypeMessageが途中まで書いたテキストの全文は保持していないため、
            // ShowLine利用時のために_currentMessageを保持する
            if (!string.IsNullOrEmpty(_currentMessage))
                MessageText.text = _currentMessage;

            _isTyping = false;
        }

        public void HideDialogue() // 会話ウィンドウを消すメソッド
        {
            dialoguePanel.SetActive(false);
        }

        // === タイプライター演出 ===
        private IEnumerator TypeMessage(string message)
        {
            _isTyping = true;
            MessageText.text = ""; // テキストを空にする

            foreach (char c in message) // 1文字ずつ取り出す
            {
                MessageText.text += c; // 1文字追加
                yield return new WaitForSeconds(typingSpeed); // 少し待つ
            }

            _isTyping = false;
        }

        // === クリック/キー入力の処理 ===
        private void Update()
        {
            // 会話中でなければ何もしない
            if (!_isActive) return;
            // Bridge経由(ShowLine)の場合は_linesがnull → Bridge側で入力を処理するためここではスキップ
            if (_lines == null) return;

            bool inputPressed =
                (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame)
                || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame);

            if (!inputPressed) return;

            if (_isTyping)
            {
                // タイピング中 → スキップ（全文を即表示）
                SkipTyping();
            }
            else
            {
                // 表示完了 → 次のセリフへ（最後なら閉じる）
                ShowNextLine();
            }
        }
    }
}
