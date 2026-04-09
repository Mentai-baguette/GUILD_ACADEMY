using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// 汎用ゲージUIコンポーネント。HP/MP/ATB/ブレイク/侵蝕/リンク等に使える。
    /// 3レイヤー構成: 背景(Track) + フィル(Fill) + 枠(Frame)
    /// UIThemeSOのカラーを参照し、テーマ変更時に自動で色を更新する。
    /// </summary>
    public class GaugeUI : MonoBehaviour
    {
        public enum GaugeType
        {
            HP,
            MP,
            ATB,
            Break,
            Erosion,
            Link
        }

        [Header("ゲージ種別")]
        [SerializeField] private GaugeType gaugeType = GaugeType.HP;

        [Header("UIコンポーネント")]
        [SerializeField] private Image backgroundImage;  // 背景(Track)
        [SerializeField] private Image fillImage;         // フィル(Fill)
        [SerializeField] private Image frameImage;        // 枠(Frame)

        [Header("テーマ")]
        [SerializeField] private UIThemeSO theme;

        [Header("値")]
        [Range(0f, 1f)]
        [SerializeField] private float fillAmount = 1f;

        [Header("HP色変化しきい値")]
        [SerializeField] private float hpMidThreshold = 0.5f;
        [SerializeField] private float hpLowThreshold = 0.2f;

        [Header("リンクゲージ")]
        [SerializeField] private float linkThreshold = 0.5f;

        /// <summary>現在の値（0〜1）</summary>
        public float FillAmount
        {
            get => fillAmount;
            set => SetFill(value);
        }

        private void Start()
        {
            ApplyTheme();
            UpdateFill();
        }

        /// <summary>ゲージの値を設定する（0〜1）</summary>
        public void SetFill(float value)
        {
            fillAmount = Mathf.Clamp01(value);
            UpdateFill();
        }

        /// <summary>現在値/最大値で設定する</summary>
        public void SetValue(float current, float max)
        {
            if (max <= 0f)
            {
                SetFill(0f);
                return;
            }
            SetFill(current / max);
        }

        /// <summary>テーマを適用する</summary>
        public void ApplyTheme()
        {
            if (theme == null) return;

            if (backgroundImage != null)
                backgroundImage.color = theme.gaugeBackground;

            if (frameImage != null)
                frameImage.color = theme.gaugeFrame;

            UpdateFillColor();
        }

        /// <summary>テーマを差し替えて適用する</summary>
        public void ApplyTheme(UIThemeSO newTheme)
        {
            theme = newTheme;
            ApplyTheme();
        }

        private void UpdateFill()
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = fillAmount;
            }
            UpdateFillColor();
        }

        private void UpdateFillColor()
        {
            if (fillImage == null || theme == null) return;

            fillImage.color = GetFillColor();
        }

        private Color GetFillColor()
        {
            switch (gaugeType)
            {
                case GaugeType.HP:
                    if (fillAmount <= hpLowThreshold) return theme.hpLow;
                    if (fillAmount <= hpMidThreshold) return theme.hpMid;
                    return theme.hpFull;

                case GaugeType.MP:
                    return theme.mpColor;

                case GaugeType.ATB:
                    return fillAmount >= 1f ? theme.atbReady : theme.atbColor;

                case GaugeType.Break:
                    return theme.breakColor;

                case GaugeType.Erosion:
                    if (fillAmount >= 0.75f) return theme.erosionCritical;
                    if (fillAmount >= 0.50f) return theme.erosionDangerous;
                    if (fillAmount >= 0.25f) return theme.erosionUnstable;
                    return theme.erosionNormal;

                case GaugeType.Link:
                    return fillAmount >= linkThreshold ? theme.linkHigh : theme.linkLow;

                default:
                    return Color.white;
            }
        }
    }
}
