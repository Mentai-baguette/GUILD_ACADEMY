using UnityEngine;
using UnityEngine.InputSystem;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.UI
{
    /// <summary>
    /// テスト用：Tキーを押すと会話ウィンドウを表示する。
    /// 動作確認が終わったら削除してOK。
    /// </summary>
    public class DialogueTestTrigger : MonoBehaviour
    {
        [Header("テスト用セリフ（DialogueManager不要で動く）")]
        [SerializeField] private DialogueUI _dialogueUI;

        private void Update()
        {
            if (Keyboard.current == null) return;

            // Tキーでテスト会話を開始
            if (Keyboard.current.tKey.wasPressedThisFrame)
            {
                TestDialogue();
            }
        }

        private void TestDialogue()
        {
            if (_dialogueUI == null)
                _dialogueUI = GetComponentInChildren<DialogueUI>();

            if (_dialogueUI == null)
            {
                Debug.LogError("[TestTrigger] DialogueUI が見つからない！");
                return;
            }

            Debug.Log("[TestTrigger] テスト会話開始");

            var lines = new DialogueLine[]
            {
                new DialogueLine("Yuna", "レイ、朝だよ〜。起きて！"),
                new DialogueLine("Ray", "…あと5分…"),
                new DialogueLine("Yuna", "もう！ギルドの人来てるって！"),
                new DialogueLine("Ray", "…わかった、起きる。"),
            };

            _dialogueUI.StartDialogue(lines);
        }
    }
}
