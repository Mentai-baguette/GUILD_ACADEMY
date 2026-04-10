using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using GuildAcademy.Core.Dialogue;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// 会話中の選択肢ウィンドウ。
    ///
    /// 仕様書セクション5準拠:
    /// - 画面中央に選択肢ウィンドウを表示
    /// - ▶ カーソルで現在選択中の項目を示す
    /// - 上下キーでカーソル移動、決定ボタンで確定
    /// - マウスクリックでも選択可能
    /// - テーマ対応
    /// </summary>
    public class ChoiceUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;

        [Header("テーマ（任意）")]
        [SerializeField] private UIThemeSO _theme;

        [Header("カーソル表示")]
        [Tooltip("選択中の選択肢に付けるプレフィックス")]
        [SerializeField] private string _cursorPrefix = "\u25b6 ";          // ▶
        [SerializeField] private string _inactivePrefix = "   ";            // 非選択時は空白

        [Header("色設定")]
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _unselectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        private Action<int> _onChoiceSelected;
        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private readonly List<TextMeshProUGUI> _labels = new List<TextMeshProUGUI>();
        private List<string> _choiceTexts = new List<string>();

        private int _currentIndex = 0;
        private bool _isActive = false;

        private void Awake()
        {
            if (_choicePanel != null)
                _choicePanel.SetActive(false);

            // テーマ色を適用
            if (_theme != null)
            {
                _selectedColor = _theme.textHighlight;
                _unselectedColor = _theme.textNormal;
            }
        }

        // ============================================================
        // 選択肢を表示する
        // ============================================================
        public void Show(List<DialogueChoice> choices, Action<int> onSelected)
        {
            _onChoiceSelected = onSelected;
            ClearButtons();

            if (_choicePanel != null)
                _choicePanel.SetActive(true);

            _choiceTexts.Clear();
            _currentIndex = 0;

            for (int i = 0; i < choices.Count; i++)
            {
                var btnObj = Instantiate(_choiceButtonPrefab, _buttonContainer);
                btnObj.SetActive(true);

                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    _choiceTexts.Add(choices[i].Text);
                    _labels.Add(label);
                }

                int index = i;
                var button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => SelectChoice(index));

                    // ホバー時にカーソルを移動
                    int hoverIndex = i;
                    var trigger = btnObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
                    if (trigger == null)
                        trigger = btnObj.AddComponent<UnityEngine.EventSystems.EventTrigger>();

                    var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
                    {
                        eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
                    };
                    pointerEnter.callback.AddListener(_ => SetCursorIndex(hoverIndex));
                    trigger.triggers.Add(pointerEnter);
                }

                _spawnedButtons.Add(btnObj);
            }

            UpdateCursorDisplay();
            _isActive = true;
        }

        // ============================================================
        // 選択肢を非表示にする
        // ============================================================
        public void Hide()
        {
            _isActive = false;
            ClearButtons();
            if (_choicePanel != null)
                _choicePanel.SetActive(false);
        }

        // ============================================================
        // キーボード入力
        // ============================================================
        private void Update()
        {
            if (!_isActive) return;
            if (Keyboard.current == null) return;

            // 上キー: カーソルを上に移動
            if (Keyboard.current.upArrowKey.wasPressedThisFrame
                || Keyboard.current.wKey.wasPressedThisFrame)
            {
                SetCursorIndex(_currentIndex - 1);
            }
            // 下キー: カーソルを下に移動
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame
                     || Keyboard.current.sKey.wasPressedThisFrame)
            {
                SetCursorIndex(_currentIndex + 1);
            }
            // 決定: Enter / Space
            else if (Keyboard.current.enterKey.wasPressedThisFrame
                     || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                SelectChoice(_currentIndex);
            }
        }

        // ============================================================
        // カーソル位置を変更する
        // ============================================================
        private void SetCursorIndex(int index)
        {
            if (_labels.Count == 0) return;

            // ループ（上端→下端、下端→上端）
            if (index < 0)
                index = _labels.Count - 1;
            else if (index >= _labels.Count)
                index = 0;

            _currentIndex = index;
            UpdateCursorDisplay();
        }

        // ============================================================
        // カーソル表示を更新する（▶ を選択中の項目に付ける）
        // ============================================================
        private void UpdateCursorDisplay()
        {
            for (int i = 0; i < _labels.Count; i++)
            {
                if (_labels[i] == null) continue;

                if (i == _currentIndex)
                {
                    _labels[i].text = _cursorPrefix + _choiceTexts[i];
                    _labels[i].color = _selectedColor;
                }
                else
                {
                    _labels[i].text = _inactivePrefix + _choiceTexts[i];
                    _labels[i].color = _unselectedColor;
                }
            }
        }

        // ============================================================
        // 選択確定
        // ============================================================
        private void SelectChoice(int index)
        {
            _isActive = false;
            _onChoiceSelected?.Invoke(index);
        }

        // ============================================================
        // ボタンをクリアする
        // ============================================================
        private void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null) Destroy(btn);
            }
            _spawnedButtons.Clear();
            _labels.Clear();
            _choiceTexts.Clear();
        }

        /// <summary>テーマを差し替えて適用する</summary>
        public void ApplyTheme(UIThemeSO newTheme)
        {
            _theme = newTheme;
            if (_theme != null)
            {
                _selectedColor = _theme.textHighlight;
                _unselectedColor = _theme.textNormal;
            }
            UpdateCursorDisplay();
        }
    }
}
