using System;
using System.Collections;
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
    /// キーボードカーソル操作、重要選択肢の★マーク強調、
    /// フェードインアニメーション、DialogueRunner連携ヘルパーを提供する。
    /// テーマ対応。
    /// </summary>
    public class ChoiceUI : MonoBehaviour
    {
        [Header("UI要素")]
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("重要選択肢の強調")]
        [SerializeField] private bool _highlightImportantChoices = true;
        [SerializeField] private Color _importantChoiceColor = new Color(1f, 0.84f, 0f);

        [Header("テーマ（任意）")]
        [SerializeField] private UIThemeSO _theme;

        [Header("カーソル表示")]
        [SerializeField] private string _cursorPrefix = "\u25b6 ";
        [SerializeField] private string _inactivePrefix = "   ";

        [Header("色設定")]
        [SerializeField] private Color _selectedColor = Color.white;
        [SerializeField] private Color _unselectedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        [Header("SE")]
        [SerializeField] private AudioClip _seSelect;
        [SerializeField] private AudioClip _seConfirm;
        [SerializeField] private AudioSource _audioSource;

        [Header("アニメーション")]
        [SerializeField] private float _fadeInDuration = 0.2f;

        private Action<int> _onChoiceSelected;
        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private readonly List<TextMeshProUGUI> _labels = new List<TextMeshProUGUI>();
        private readonly List<string> _choiceTexts = new List<string>();
        private readonly List<bool> _importantFlags = new List<bool>();

        private int _currentIndex;
        private bool _isActive;
        private Coroutine _fadeCoroutine;
        private DialogueRunner _connectedRunner;

        private void Awake()
        {
            if (_choicePanel != null)
                _choicePanel.SetActive(false);

            if (_theme != null)
            {
                _selectedColor = _theme.textHighlight;
                _unselectedColor = _theme.textNormal;
            }
        }

        public void Show(List<DialogueChoice> choices, Action<int> onSelected)
        {
            _onChoiceSelected = onSelected;
            ClearButtons();

            if (_choicePanel != null)
                _choicePanel.SetActive(true);

            _choiceTexts.Clear();
            _importantFlags.Clear();
            _currentIndex = 0;

            for (int i = 0; i < choices.Count; i++)
            {
                var btnObj = Instantiate(_choiceButtonPrefab, _buttonContainer);
                btnObj.SetActive(true);

                var choice = choices[i];
                bool isImportant = _highlightImportantChoices && !string.IsNullOrEmpty(choice.Flag);

                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    string text = isImportant ? $"\u2605 {choice.Text}" : choice.Text;
                    _choiceTexts.Add(text);
                    _importantFlags.Add(isImportant);
                    _labels.Add(label);
                }

                int index = i;
                var button = btnObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.AddListener(() => SelectChoice(index));

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
            StartFadeIn();
        }

        public void Hide()
        {
            _isActive = false;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            ClearButtons();
            if (_choicePanel != null)
                _choicePanel.SetActive(false);
        }

        private void Update()
        {
            if (!_isActive) return;
            if (Keyboard.current == null) return;

            if (Keyboard.current.upArrowKey.wasPressedThisFrame
                || Keyboard.current.wKey.wasPressedThisFrame)
            {
                PlaySE(_seSelect);
                SetCursorIndex(_currentIndex - 1);
            }
            else if (Keyboard.current.downArrowKey.wasPressedThisFrame
                     || Keyboard.current.sKey.wasPressedThisFrame)
            {
                PlaySE(_seSelect);
                SetCursorIndex(_currentIndex + 1);
            }
            else if (Keyboard.current.enterKey.wasPressedThisFrame
                     || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlaySE(_seConfirm);
                SelectChoice(_currentIndex);
            }
        }

        // === DialogueRunner連携 ===

        public void ConnectToRunner(DialogueRunner runner)
        {
            if (runner == null) return;
            if (_connectedRunner != null)
                DisconnectFromRunner(_connectedRunner);
            _connectedRunner = runner;
            runner.OnChoicesPresented += OnRunnerChoicesPresented;
        }

        public void DisconnectFromRunner(DialogueRunner runner)
        {
            if (runner == null) return;
            runner.OnChoicesPresented -= OnRunnerChoicesPresented;
            if (_connectedRunner == runner)
                _connectedRunner = null;
        }

        private void OnRunnerChoicesPresented(List<DialogueChoice> choices)
        {
            Show(choices, index => { _connectedRunner?.SelectChoice(index); });
        }

        // === 内部ロジック ===

        private void SetCursorIndex(int index)
        {
            if (_labels.Count == 0) return;
            if (index < 0) index = _labels.Count - 1;
            else if (index >= _labels.Count) index = 0;
            _currentIndex = index;
            UpdateCursorDisplay();
        }

        private void UpdateCursorDisplay()
        {
            for (int i = 0; i < _labels.Count; i++)
            {
                if (_labels[i] == null) continue;

                bool isImportant = i < _importantFlags.Count && _importantFlags[i];
                Color normalColor = isImportant ? _importantChoiceColor : _unselectedColor;

                if (i == _currentIndex)
                {
                    _labels[i].text = _cursorPrefix + _choiceTexts[i];
                    _labels[i].color = isImportant ? _importantChoiceColor : _selectedColor;
                }
                else
                {
                    _labels[i].text = _inactivePrefix + _choiceTexts[i];
                    _labels[i].color = normalColor;
                }
            }
        }

        private void SelectChoice(int index)
        {
            if (!_isActive) return;
            // Hide を先に実行（コールバックが次のShow を呼ぶ可能性があるため）
            Hide();
            _onChoiceSelected?.Invoke(index);
        }

        private void StartFadeIn()
        {
            if (_canvasGroup == null) return;
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeInCoroutine());
        }

        private IEnumerator FadeInCoroutine()
        {
            _canvasGroup.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                yield return null;
            }
            _canvasGroup.alpha = 1f;
            _fadeCoroutine = null;
        }

        private void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null) Destroy(btn);
            }
            _spawnedButtons.Clear();
            _labels.Clear();
            _choiceTexts.Clear();
            _importantFlags.Clear();
        }

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

        private void PlaySE(AudioClip clip)
        {
            if (clip != null && _audioSource != null)
                _audioSource.PlayOneShot(clip);
        }

        private void OnDestroy()
        {
            if (_connectedRunner != null)
                DisconnectFromRunner(_connectedRunner);
        }
    }
}
