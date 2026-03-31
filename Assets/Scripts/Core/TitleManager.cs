using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    // === NEW GAME ボタン用 ===
    public void OnNewGame()
    {
        // "Field" という名前のシーンに切り替える
        SceneManager.LoadScene("Field");
    }

    // === CONTINUE ボタン用 ===
    public void OnContinue()
    {
        // Week2でセーブ機能作るまでは仮置き
        Debug.Log("セーブデータ読み込み：未実装");
    }

    // === EXIT ボタン用 ===
    public void OnExit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
