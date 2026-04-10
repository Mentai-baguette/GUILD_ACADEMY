using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.UI
{
    /// <summary>
    /// 選択肢UI。キーボードカーソル操作、重要選択肢の★マーク強調、
    /// フェードインアニメーション、DialogueRunner連携ヘルパーを提供する。
    /// </summary>
    public class ChoiceUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Important Choice Highlight")]
        [SerializeField] private bool _highlightImportantChoices = true;
        [SerializeField] private Color _importantChoiceColor = new Color(1f, 0.84f, 0f); // #FFD700

        [Header("SE")]
        [SerializeField] private AudioClip _seSelect;
        [SerializeField] private AudioClip _seConfirm;
        [SerializeField] private AudioSource _audioSource;

        [Header("Animation")]
        [SerializeField] private float _fadeInDuration = 0.2f;

        private Action<int> _onChoiceSelected;
        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();
        private int _selectedIndex;
        private bool _isActive;
        private Coroutine _fadeCoroutine;

        // DialogueRunner連携用
        private DialogueRunner _connectedRunner;

        private void Awake()
        {
            if (_choicePanel != null)
                _choicePanel.SetActive(false);
        }

        private void Update()
        {
            if (!_isActive) return;
            if (_spawnedButtons.Count == 0) return;
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

            // Enter/Spaceで決定
            if (Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlaySE(_seConfirm);
                SelectChoice(_selectedIndex);
            }
        }

        /// <summary>
        /// 選択肢を表示する。
        /// </summary>
        public void Show(List<DialogueChoice> choices, Action<int> onSelected)
        {
            _onChoiceSelected = onSelected;
            ClearButtons();

            if (_choicePanel != null)
                _choicePanel.SetActive(true);

            for (int i = 0; i < choices.Count; i++)
            {
                var btnObj = Instantiate(_choiceButtonPrefab, _buttonContainer);
                btnObj.SetActive(true);

                var choice = choices[i];
                bool isImportant = _highlightImportantChoices && !string.IsNullOrEmpty(choice.Flag);

                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = isImportant
                        ? $"\u2605 {choice.Text}"
                        : choice.Text;

                    if (isImportant)
                        label.color = _importantChoiceColor;
                }

                int index = i;
                var button = btnObj.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => SelectChoice(index));

                _spawnedButtons.Add(btnObj);
            }

            _selectedIndex = 0;
            _isActive = true;
            UpdateSelectionHighlight();
            StartFadeIn();
        }

        /// <summary>
        /// 選択肢を非表示にする。
        /// </summary>
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

        /// <summary>
        /// DialogueRunnerのOnChoicesPresentedイベントを購読し、
        /// 選択結果をSelectChoiceに通知する接続ヘルパー。
        /// </summary>
        public void ConnectToRunner(DialogueRunner runner)
        {
            if (runner == null) return;

            // 二重購読を防ぐ
            if (_connectedRunner != null)
                DisconnectFromRunner(_connectedRunner);

            _connectedRunner = runner;
            runner.OnChoicesPresented += OnRunnerChoicesPresented;
        }

        /// <summary>
        /// DialogueRunnerのイベント購読を解除する。
        /// </summary>
        public void DisconnectFromRunner(DialogueRunner runner)
        {
            if (runner == null) return;

            runner.OnChoicesPresented -= OnRunnerChoicesPresented;

            if (_connectedRunner == runner)
                _connectedRunner = null;
        }

        private void OnRunnerChoicesPresented(List<DialogueChoice> choices)
        {
            Show(choices, index =>
            {
                _connectedRunner?.SelectChoice(index);
            });
        }

        private void SelectChoice(int index)
        {
            if (!_isActive) return;

            _onChoiceSelected?.Invoke(index);
            Hide();
        }

        private void MoveSelection(int direction)
        {
            if (_spawnedButtons.Count == 0) return;

            int newIndex = (_selectedIndex + direction + _spawnedButtons.Count) % _spawnedButtons.Count;
            if (newIndex != _selectedIndex)
            {
                _selectedIndex = newIndex;
                PlaySE(_seSelect);
                UpdateSelectionHighlight();
            }
        }

        private void UpdateSelectionHighlight()
        {
            for (int i = 0; i < _spawnedButtons.Count; i++)
            {
                var button = _spawnedButtons[i].GetComponent<Button>();
                if (button == null) continue;

                if (i == _selectedIndex)
                {
                    button.Select();
                }
            }
        }

        private void StartFadeIn()
        {
            if (_canvasGroup == null) return;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

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
