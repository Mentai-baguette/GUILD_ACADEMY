using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Core.Data;

namespace GuildAcademy.MonoBehaviours.Battle
{
    /// <summary>
    /// パーティメンバー1人分のステータス表示UIエントリ。
    /// Prefabにアタッチして使用する。
    /// </summary>
    public class PartyStatusEntry : MonoBehaviour
    {
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Text _levelLabel;
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private Text _hpLabel;
        [SerializeField] private Slider _mpSlider;
        [SerializeField] private Text _mpLabel;

        private CharacterStats _stats;

        public CharacterStats Stats => _stats;

        public void Initialize(CharacterStats stats)
        {
            _stats = stats;
            if (stats == null) return;

            if (_nameLabel != null) _nameLabel.text = stats.Name;
            if (_levelLabel != null) _levelLabel.text = $"Lv.{stats.Lv}";

            SetupSlider(_hpSlider, 0, stats.MaxHp, stats.CurrentHp);
            SetupSlider(_mpSlider, 0, stats.MaxMp, stats.CurrentMp);

            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            if (_stats == null) return;

            if (_hpSlider != null)
            {
                _hpSlider.maxValue = _stats.MaxHp;
                _hpSlider.value = _stats.CurrentHp;
            }

            if (_hpLabel != null)
                _hpLabel.text = $"{_stats.CurrentHp}/{_stats.MaxHp}";

            if (_mpSlider != null)
            {
                _mpSlider.maxValue = _stats.MaxMp;
                _mpSlider.value = _stats.CurrentMp;
            }

            if (_mpLabel != null)
                _mpLabel.text = $"{_stats.CurrentMp}/{_stats.MaxMp}";
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
