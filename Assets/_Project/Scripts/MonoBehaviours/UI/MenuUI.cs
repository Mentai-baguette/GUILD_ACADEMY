using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using GuildAcademy.Core.UI;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.Audio;

namespace GuildAcademy.MonoBehaviours.UI
{
    /// <summary>
    /// メニュー画面の UI 制御。
    /// フィールドシーンに常駐する Singleton。
    /// Esc キーで開閉し、開いている間は Time.timeScale=0。
    /// 左右キー（←→ / A / D）でタブ切替。
    /// 10 タブ: ステータス / 装備 / スキルツリー / アイテム / パーティ編成
    ///          / 履修 / ノート / 図鑑 / コンフィグ / セーブ・ロード
    /// </summary>
    public class MenuUI : MonoBehaviour
    {
        #region Singleton

        private static MenuUI _instance;
        public static MenuUI Instance => _instance;

        #endregion

        #region Inspector Fields

        [Header("メニュー全体")]
        [SerializeField] private GameObject menuRoot;

        [Header("タブボタン（順番は MenuTab enum に対応）")]
        [SerializeField] private Button[] tabButtons;

        [Header("タブ名ラベル")]
        [SerializeField] private TextMeshProUGUI tabNameLabel;

        [Header("各タブのパネル（順番は MenuTab enum に対応）")]
        [SerializeField] private GameObject[] tabPanels;

        [Header("ステータス表示")]
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("SE")]
        [SerializeField] private AudioClip seTabSwitch;
        [SerializeField] private AudioClip seConfirm;
        [SerializeField] private AudioClip seCancel;

        #endregion

        #region Private Fields

        private MenuTabController _tabController;
        private bool _isOpen;
        private float _savedTimeScale = 1f;

        private static readonly string[] TabDisplayNames =
        {
            "ステータス",
            "装備",
            "スキルツリー",
            "アイテム",
            "パーティ編成",
            "履修",
            "ノート",
            "図鑑",
            "コンフィグ",
            "セーブ・ロード",
        };

        // デモ用ダミーデータ（外部から設定可能）
        private CharacterStats[] _partyStats;

        #endregion

        #region Properties

        /// <summary>メニューが開いているかどうか。</summary>
        public bool IsOpen => _isOpen;

        /// <summary>Core のタブコントローラー参照（テスト用）。</summary>
        public MenuTabController TabController => _tabController;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _tabController = new MenuTabController();
            _tabController.OnTabChanged += OnTabChanged;

            // タブボタンのクリックイベント
            if (tabButtons != null)
            {
                for (int i = 0; i < tabButtons.Length && i < MenuTabController.TabCount; i++)
                {
                    if (tabButtons[i] == null) continue;
                    int index = i;
                    tabButtons[i].onClick.AddListener(() =>
                    {
                        PlaySE(seConfirm);
                        _tabController.SetTab((MenuTab)index);
                    });
                }
            }

            // 初期状態は閉じている
            if (menuRoot != null)
                menuRoot.SetActive(false);
            _isOpen = false;
        }

        private void Update()
        {
            if (Keyboard.current == null) return;

            // メニュー開閉トグル（Escape）
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (_isOpen)
                    CloseMenu();
                else
                    OpenMenu();
                return;
            }

            if (!_isOpen) return;

            // タブ切替: 左右キー / A / D
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame)
            {
                PlaySE(seTabSwitch);
                _tabController.MoveRight();
            }
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                     Keyboard.current.aKey.wasPressedThisFrame)
            {
                PlaySE(seTabSwitch);
                _tabController.MoveLeft();
            }
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            if (_tabController != null)
            {
                _tabController.OnTabChanged -= OnTabChanged;
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// メニューを開く。Time.timeScale を 0 にしてゲームをポーズする。
        /// </summary>
        public void OpenMenu()
        {
            if (_isOpen) return;
            _isOpen = true;
            _savedTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            if (menuRoot != null)
                menuRoot.SetActive(true);

            // タブ表示を更新
            RefreshTabDisplay();
            Debug.Log("[MenuUI] メニューを開きました");
        }

        /// <summary>
        /// メニューを閉じる。Time.timeScale を復元する。
        /// </summary>
        public void CloseMenu()
        {
            if (!_isOpen) return;
            _isOpen = false;
            Time.timeScale = _savedTimeScale;

            PlaySE(seCancel);

            if (menuRoot != null)
                menuRoot.SetActive(false);

            Debug.Log("[MenuUI] メニューを閉じました");
        }

        /// <summary>
        /// パーティメンバーのステータスを設定する。
        /// ステータスタブ表示に使用。
        /// </summary>
        public void SetPartyStats(CharacterStats[] stats)
        {
            _partyStats = stats;
            if (_isOpen && _tabController.CurrentTab == MenuTab.Status)
            {
                RefreshStatusPanel();
            }
        }

        #endregion

        #region Tab Handling

        private void OnTabChanged(MenuTab newTab)
        {
            RefreshTabDisplay();
        }

        private void RefreshTabDisplay()
        {
            int currentIndex = _tabController.CurrentIndex;

            // パネル表示切替
            if (tabPanels != null)
            {
                for (int i = 0; i < tabPanels.Length; i++)
                {
                    if (tabPanels[i] != null)
                        tabPanels[i].SetActive(i == currentIndex);
                }
            }

            // タブボタンのハイライト
            if (tabButtons != null)
            {
                for (int i = 0; i < tabButtons.Length; i++)
                {
                    if (tabButtons[i] == null) continue;
                    var colors = tabButtons[i].colors;
                    colors.normalColor = (i == currentIndex)
                        ? new Color(1f, 0.9f, 0.5f, 1f)
                        : Color.white;
                    tabButtons[i].colors = colors;
                }
            }

            // タブ名ラベル
            if (tabNameLabel != null && currentIndex < TabDisplayNames.Length)
            {
                tabNameLabel.text = TabDisplayNames[currentIndex];
            }

            // 各タブのコンテンツ更新
            MenuTab tab = _tabController.CurrentTab;
            switch (tab)
            {
                case MenuTab.Status:
                    RefreshStatusPanel();
                    break;
                case MenuTab.Equipment:
                    Debug.Log("[MenuUI] 装備タブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.SkillTree:
                    Debug.Log("[MenuUI] スキルツリータブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.Item:
                    Debug.Log("[MenuUI] アイテムタブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.Party:
                    Debug.Log("[MenuUI] パーティ編成タブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.Curriculum:
                    Debug.Log("[MenuUI] 履修タブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.Note:
                    Debug.Log("[MenuUI] ノートタブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.Encyclopedia:
                    Debug.Log("[MenuUI] 図鑑タブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.Config:
                    Debug.Log("[MenuUI] コンフィグタブ: 未実装（基盤のみ）");
                    break;
                case MenuTab.SaveLoad:
                    Debug.Log("[MenuUI] セーブ・ロードタブ: 未実装（基盤のみ）");
                    break;
            }
        }

        #endregion

        #region Status Panel

        private void RefreshStatusPanel()
        {
            if (statusText == null) return;

            if (_partyStats == null || _partyStats.Length == 0)
            {
                statusText.text = "パーティメンバーがいません";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var stats in _partyStats)
            {
                if (stats == null) continue;
                sb.AppendLine($"━━━ {stats.Name} ━━━");
                sb.AppendLine($"Lv: {stats.Lv}    EXP: {stats.Exp}    SP: {stats.Sp}");
                sb.AppendLine($"HP: {stats.CurrentHp} / {stats.MaxHp}");
                sb.AppendLine($"MP: {stats.CurrentMp} / {stats.MaxMp}");
                sb.AppendLine($"ATK: {stats.Atk}   DEF: {stats.Def}");
                sb.AppendLine($"INT: {stats.Int}   RES: {stats.Res}");
                sb.AppendLine($"AGI: {stats.Agi}   DEX: {stats.Dex}   LUK: {stats.Luk}");
                sb.AppendLine();
            }
            statusText.text = sb.ToString();
        }

        #endregion

        #region Audio

        private void PlaySE(AudioClip clip)
        {
            if (clip == null) return;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySE(clip);
            }
        }

        #endregion
    }
}
