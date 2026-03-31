using UnityEngine;
using TMPro; //TextMeshProUGUI（テキスト表示）
using System.Collections; //コルーチン用

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel; // ダイアログパネル
    [SerializeField] private TextMeshProUGUI NameText; // 名前を表示するテキスト
    [SerializeField] private TextMeshProUGUI MessageText; // 話の内容を表示するテキスト
    [SerializeField] private float typingSpeed = 0.05f; // タイピングの速度(1文字あたり何秒待つか)

    private Coroutine typingCoroutine; // タイピングのコルーチンを覚えておく箱
    private bool isTyping = false; // タイピング中かどうか
    private string currentMessage = ""; // セリフ全文を覚えておく箱
    public void Start()
    {
        HideDialogue();// ダイアログパネルを最初は非表示にする
    }

    public void ShowDialogue(string speakerName, string message)//会話ウィンドウを出すメソッド
    {
        dialoguePanel.SetActive(true); // ダイアログパネルを表示する
        NameText.text = speakerName; // 名前をテキストにセットする
        currentMessage = message; // セリフ全文を覚えておく箱にセットする

        if (typingCoroutine != null) // もしタイピングのコルーチンが動いていたら
        {
            StopCoroutine(typingCoroutine); // タイピングのコルーチンを止める
        }
        typingCoroutine = StartCoroutine(TypeMessage()); // タイピングのコルーチンを始める
    }

    public void HideDialogue()//会話ウィンドウを消すメソッド
    {
        dialoguePanel.SetActive(false); // ダイアログパネルを非表示にする
    }

    private IEnumerator TypeMessage()//セリフを1文字ずつ表示するコルーチン
    {
        isTyping = true; // タイピング中フラグを立てる
        MessageText.text = ""; // 話の内容テキストを空にする

        foreach (char c in currentMessage) // セリフ全文の文字を1文字ずつ取り出す
        {
            MessageText.text += c; // 話の内容テキストに1文字追加する
            yield return new WaitForSeconds(typingSpeed); // タイピングの速度だけ待つ
        }

        isTyping = false; // タイピング中フラグを下ろす
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
    {
        ShowDialogue("レイ", "この学園には何か秘密があるようだ…");
    }

    if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Return))
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            MessageText.text = currentMessage;
            isTyping = false;
        }
        else if (dialoguePanel.activeSelf)
        {
            HideDialogue();
        }
    }
    }

}
