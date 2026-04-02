using UnityEngine;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.UI
{
    /// <summary>
    /// DialogueUIの動作確認用。確認が終わったら削除してOK。
    /// Play開始時にテスト会話が自動で始まる。
    /// </summary>
    public class DialogueTest : MonoBehaviour
    {
        [SerializeField] private DialogueUI dialogueUI;

        private void Start()
        {
            dialogueUI.OnDialogueComplete += () =>
            {
                Debug.Log("会話が終了しました！");
            };

            DialogueLine[] lines = new DialogueLine[]
            {
                new DialogueLine("先生", "ようこそ、ギルドアカデミーへ。"),
                new DialogueLine("先生", "ここでは仲間と共に深淵の謎に挑む。"),
                new DialogueLine("レイ", "…よろしくお願いします。"),
            };

            dialogueUI.StartDialogue(lines);
        }
    }
}
