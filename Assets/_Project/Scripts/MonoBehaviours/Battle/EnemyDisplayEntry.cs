using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.MonoBehaviours.Battle
{
    /// <summary>
    /// 敵1体分の表示UIエントリ。名前、HPバー、ブレイクゲージを表示。
    /// Prefabにアタッチして使用する。
    /// </summary>
    public class EnemyDisplayEntry : MonoBehaviour
    {
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Text _hpLabel;
        [SerializeField] private Slider _breakSlider;
        [SerializeField] private Text _breakLabel;
        [SerializeField] private Image _breakFill;

        [Header("Break Colors")]
        [SerializeField] private Color _breakNormalColor = new Color(0.3f, 0.6f, 1.0f);
        [SerializeField] private Color _breakFullColor = new Color(1.0f, 0.2f, 0.2f);

        private CharacterStats _stats;
        private BreakSystem _breakSystem;

        public CharacterStats Stats => _stats;

        public void Initialize(CharacterStats stats, BreakSystem breakSystem)
        {
            _stats = stats;
            _breakSystem = breakSystem;

            if (stats == null) return;

            if (_nameLabel != null) _nameLabel.text = stats.Name;

            SetupSlider(_hpSlider, 0, stats.MaxHp, stats.CurrentHp);
            SetupSlider(_breakSlider, 0, 1, 0);

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_stats == null) return;

            // HP
            if (_hpSlider != null)
            {
                _hpSlider.maxValue = _stats.MaxHp;
                _hpSlider.value = Mathf.Max(0, _stats.CurrentHp);
            }

            if (_hpLabel != null)
                _hpLabel.text = $"{Mathf.Max(0, _stats.CurrentHp)}/{_stats.MaxHp}";

            // Break gauge
            if (_breakSystem != null)
            {
                float breakPercent = _breakSystem.GetBreakGaugePercent(_stats);
                bool isBroken = _breakSystem.IsBreaking(_stats);

                if (_breakSlider != null)
                {
                    _breakSlider.minValue = 0f;
                    _breakSlider.maxValue = 1f;
                    _breakSlider.value = breakPercent;
                }

                if (_breakLabel != null)
                {
                    _breakLabel.text = isBroken ? "BREAK!" : $"BRK:{breakPercent * 100f:F0}%";
                }

                if (_breakFill != null)
                {
                    _breakFill.color = isBroken
                        ? _breakFullColor
                        : Color.Lerp(_breakNormalColor, _breakFullColor, breakPercent);
                }
            }

            // Hide if defeated
            if (_stats.CurrentHp <= 0)
            {
                var cg = GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 0.4f;
            }
        }

        private static void SetupSlider(Slider slider, float min, float max, float value)
        {
            if (slider == null) return;
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
        }
    }
}
