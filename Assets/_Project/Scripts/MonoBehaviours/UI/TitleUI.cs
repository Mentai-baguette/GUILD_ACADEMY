using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.UI
{
    /// <summary>
    /// タイトル画面のUI制御。
    /// screen-flow.md §2 準拠。
    /// NEW GAME → 難易度選択パネル表示 → 選択後にフィールドへ遷移
    /// CONTINUE → セーブスロット選択（未実装時はログ出力）
    /// EXIT → ゲーム終了
    /// 上下キーで選択移動、Enter/Spaceで決定。
    /// </summary>
    public class TitleUI : MonoBehaviour
    {
        [Header("タイトルメニューボタン")]
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button exitButton;

        [Header("難易度選択パネル（§3）")]
        [SerializeField] private GameObject difficultyPanel;
        [SerializeField] private Button easyButton;
        [SerializeField] private Button normalButton;
        [SerializeField] private Button hardButton;
        [SerializeField] private Button nightmareButton;

        [Header("カーソル表示（任意）")]
        [SerializeField] private GameObject cursorIndicator;

        [Header("遷移先シーン名")]
        [SerializeField] private string fieldSceneName = "Field";

        [Header("バージョン表示（任意）")]
        [SerializeField] private TextMeshProUGUI versionText;

        [Header("SE（AudioManager実装後に設定）")]
        [SerializeField] private AudioClip seSelect;
        [SerializeField] private AudioClip seConfirm;
        [SerializeField] private AudioSource audioSource;

        private Button[] _currentButtons;
        private Button[] _titleButtons;
        private Button[] _difficultyButtons;
        private int _selectedIndex = 0;
        private bool _isInDifficultySelect = false;

        private void Start()
        {
            _titleButtons = new[] { newGameButton, continueButton, exitButton };
            _difficultyButtons = new[] { easyButton, normalButton, hardButton, nightmareButton };
            _currentButtons = _titleButtons;

            // タイトルメニューのクリックイベント
            newGameButton.onClick.AddListener(OnNewGame);
            continueButton.onClick.AddListener(OnContinue);
            exitButton.onClick.AddListener(OnExit);

            // 難易度選択のクリックイベント
            if (easyButton != null) easyButton.onClick.AddListener(() => OnDifficultySelected(0));
            if (normalButton != null) normalButton.onClick.AddListener(() => OnDifficultySelected(1));
            if (hardButton != null) hardButton.onClick.AddListener(() => OnDifficultySelected(2));
            if (nightmareButton != null) nightmareButton.onClick.AddListener(() => OnDifficultySelected(3));

            // 難易度パネルは最初非表示
            if (difficultyPanel != null)
                difficultyPanel.SetActive(false);

            // Nightmareは初回プレイ時は非活性
            // TODO: NG+クリアフラグで判定する
            if (nightmareButton != null)
                nightmareButton.interactable = false;

            // セーブデータがなければCONTINUEを無効化
            // TODO: SaveManagerが実装されたらセーブ有無を判定する
            // continueButton.interactable = SaveManager.HasSaveData();

            // バージョン表示
            if (versionText != null)
                versionText.text = $"v{Application.version}";

            // 初期選択
            UpdateSelection();
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // 上下キーで選択移動
            if (Keyboard.current.downArrowKey.wasPressedThisFrame ||
                Keyboard.current.sKey.wasPressedThisFrame)
            {
                MoveSelection(1);
            }
            else if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
                     Keyboard.current.wKey.wasPressedThisFrame)
            {
                MoveSelection(-1);
            }

            // 決定
            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlaySE(seConfirm);
                ConfirmSelection();
            }

            // キャンセル（難易度選択中のみ）
            if (_isInDifficultySelect &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseDifficultyPanel();
            }
        }

        private void MoveSelection(int direction)
        {
            int nextIndex = _selectedIndex;
            int attempts = 0;
            do
            {
                nextIndex = (nextIndex + direction + _currentButtons.Length) % _currentButtons.Length;
                attempts++;
            }
            while (_currentButtons[nextIndex] != null &&
                   !_currentButtons[nextIndex].interactable &&
                   attempts < _currentButtons.Length);

            if (_currentButtons[nextIndex] != null && _currentButtons[nextIndex].interactable)
            {
                _selectedIndex = nextIndex;
                PlaySE(seSelect);
                UpdateSelection();
            }
        }

        private void ConfirmSelection()
        {
            if (_selectedIndex >= 0 && _selectedIndex < _currentButtons.Length &&
                _currentButtons[_selectedIndex] != null)
            {
                _currentButtons[_selectedIndex].onClick.Invoke();
            }
        }

        private void UpdateSelection()
        {
            if (_currentButtons[_selectedIndex] != null)
                _currentButtons[_selectedIndex].Select();

            if (cursorIndicator != null && _currentButtons[_selectedIndex] != null)
            {
                cursorIndicator.transform.position =
                    _currentButtons[_selectedIndex].transform.position + Vector3.left * 120f;
            }
        }

        // === タイトルメニュー ===

        private void OnNewGame()
        {
            if (difficultyPanel != null)
            {
                OpenDifficultyPanel();
            }
            else
            {
                // 難易度パネルが未設定の場合は直接遷移
                StartGame();
            }
        }

        private void OnContinue()
        {
            // TODO: セーブスロット選択画面を表示する（GA-25 SaveManager連携）
            Debug.Log("セーブデータ読み込み：未実装");
        }

        private void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // === 難易度選択（§3） ===

        private void OpenDifficultyPanel()
        {
            _isInDifficultySelect = true;
            difficultyPanel.SetActive(true);
            _currentButtons = _difficultyButtons;
            _selectedIndex = 1; // デフォルトはNORMAL
            UpdateSelection();
        }

        private void CloseDifficultyPanel()
        {
            _isInDifficultySelect = false;
            difficultyPanel.SetActive(false);
            _currentButtons = _titleButtons;
            _selectedIndex = 0;
            UpdateSelection();
        }

        private void OnDifficultySelected(int difficultyIndex)
        {
            // TODO: DifficultyManager.SetDifficulty() に連携（GA-85）
            // 0=Easy(x0.7), 1=Normal(x1.0), 2=Hard(x1.3), 3=Nightmare(x1.6,NG+専用)
            Debug.Log($"難易度選択: {difficultyIndex}");
            StartGame();
        }

        private void StartGame()
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(fieldSceneName);
            }
        }

        // === SE再生 ===

        private void PlaySE(AudioClip clip)
        {
            if (clip != null && audioSource != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
