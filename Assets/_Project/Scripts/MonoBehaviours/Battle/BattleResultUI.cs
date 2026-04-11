using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using GuildAcademy.MonoBehaviours.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    /// <summary>
    /// バトル勝利時のリザルト画面UI。
    /// BattleManager.OnBattleFinished を購読し、PlayerVictory 時にリザルトパネルを表示する。
    /// </summary>
    public class BattleResultUI : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _resultPanel;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Text Fields")]
        [SerializeField] private Text _victoryText;
        [SerializeField] private Text _expText;
        [SerializeField] private Text _goldText;
        [SerializeField] private Text _spText;
        [SerializeField] private Text _droppedItemsText;
        [SerializeField] private Text _levelUpText;

        [Header("Settings")]
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _countUpDuration = 1.0f;

        private BattleManager _battleManager;
        private BattleResultData _resultData;
        private bool _waitingForConfirm;

        private void Awake()
        {
            if (_resultPanel != null)
                _resultPanel.SetActive(false);

            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }

        private void Start()
        {
            _battleManager = FindObjectOfType<BattleManager>();
            if (_battleManager != null)
            {
                _battleManager.OnBattleFinished += OnBattleFinished;
            }
            else
            {
                Debug.LogWarning("[BattleResultUI] BattleManager not found in scene.");
            }
        }

        private void OnBattleFinished(BattleResult result)
        {
            if (result != BattleResult.PlayerVictory)
                return;

            // リザルト計算
            var calculator = new BattleResultCalculator();
            var party = new List<CharacterStats>(_battleManager.Party);
            var enemies = new List<CharacterStats>(_battleManager.Enemies);
            _resultData = calculator.Calculate(party, enemies);

            // パネル表示開始
            StartCoroutine(ShowResultCoroutine());
        }

        private IEnumerator ShowResultCoroutine()
        {
            if (_resultPanel != null)
                _resultPanel.SetActive(true);

            // VICTORY! テキスト
            if (_victoryText != null)
                _victoryText.text = "VICTORY!";

            // フェードイン
            yield return StartCoroutine(FadeInCoroutine());

            // EXP カウントアップ
            yield return StartCoroutine(CountUpCoroutine(_expText, "EXP: ", _resultData.TotalEXP));

            // Gold カウントアップ
            yield return StartCoroutine(CountUpCoroutine(_goldText, "Gold: ", _resultData.TotalGold));

            // SP 表示
            if (_spText != null)
                _spText.text = $"SP: +{_resultData.TotalSP}";

            // ドロップアイテム表示
            if (_droppedItemsText != null)
            {
                if (_resultData.DroppedItems != null && _resultData.DroppedItems.Count > 0)
                    _droppedItemsText.text = "Items: " + string.Join(", ", _resultData.DroppedItems);
                else
                    _droppedItemsText.text = "";
            }

            // レベルアップ情報表示
            if (_levelUpText != null)
            {
                var lines = new List<string>();
                foreach (var kvp in _resultData.LevelUps)
                {
                    lines.Add($"{kvp.Key}: Lv{kvp.Value.OldLevel} -> Lv{kvp.Value.NewLevel}");
                }
                _levelUpText.text = lines.Count > 0 ? string.Join("\n", lines) : "";
            }

            _waitingForConfirm = true;
        }

        private IEnumerator FadeInCoroutine()
        {
            if (_canvasGroup == null)
                yield break;

            float elapsed = 0f;
            _canvasGroup.alpha = 0f;

            while (elapsed < _fadeInDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / _fadeInDuration);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
        }

        private IEnumerator CountUpCoroutine(Text textField, string prefix, int targetValue)
        {
            if (textField == null)
                yield break;

            float elapsed = 0f;
            while (elapsed < _countUpDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _countUpDuration);
                int current = Mathf.RoundToInt(Mathf.Lerp(0, targetValue, t));
                textField.text = $"{prefix}{current}";
                yield return null;
            }

            textField.text = $"{prefix}{targetValue}";
        }

        private void Update()
        {
            if (!_waitingForConfirm) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                _waitingForConfirm = false;
                ReturnToField();
            }
        }

        private void ReturnToField()
        {
            var setup = _battleManager?.Setup;
            if (setup == null) return;

            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(setup.ReturnSceneName);
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(setup.ReturnSceneName);
            }
        }

        private void OnDestroy()
        {
            if (_battleManager != null)
            {
                _battleManager.OnBattleFinished -= OnBattleFinished;
            }
        }
    }
}
