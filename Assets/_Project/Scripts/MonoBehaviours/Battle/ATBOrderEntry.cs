using UnityEngine;
using UnityEngine.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    /// <summary>
    /// ATB行動順リストの1エントリ。名前とゲージ値を表示する。
    /// Prefabにアタッチして使用する。
    /// </summary>
    public class ATBOrderEntry : MonoBehaviour
    {
        [SerializeField] private Text _nameLabel;
        [SerializeField] private Slider _gaugeSlider;
        [SerializeField] private Image _gaugeFill;

        [Header("Colors")]
        [SerializeField] private Color _allyColor = new Color(0.2f, 0.7f, 1.0f);
        [SerializeField] private Color _enemyColor = new Color(1.0f, 0.3f, 0.3f);

        /// <summary>
        /// 表示を更新する。
        /// </summary>
        /// <param name="characterName">キャラクター名</param>
        /// <param name="gaugeNormalized">0-1のゲージ値</param>
        /// <param name="isEnemy">敵かどうか</param>
        public void UpdateDisplay(string characterName, float gaugeNormalized, bool isEnemy)
        {
            if (_nameLabel != null)
                _nameLabel.text = characterName ?? "";

            if (_gaugeSlider != null)
            {
                _gaugeSlider.minValue = 0f;
                _gaugeSlider.maxValue = 1f;
                _gaugeSlider.value = Mathf.Clamp01(gaugeNormalized);
            }

            if (_gaugeFill != null)
                _gaugeFill.color = isEnemy ? _enemyColor : _allyColor;
        }
    }
}
