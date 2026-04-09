using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// テーマ対応ウィンドウコンポーネント。
    /// ダイアログ、メニューパネル、コマンドウィンドウ、選択肢ウィンドウ等で使用。
    /// UIThemeSOから背景色・枠色・テキスト色を取得して適用する。
    /// 9-Slice対応のImage想定。
    /// </summary>
    public class ThemedWindow : MonoBehaviour
    {
        [Header("テーマ")]
        [SerializeField] private UIThemeSO theme;

        [Header("ウィンドウ画像")]
        [SerializeField] private Image backgroundImage;    // ウィンドウ背景
        [SerializeField] private Image borderImage;        // 外枠
        [SerializeField] private Image borderInnerImage;   // 内枠（任意）

        [Header("テキスト（任意）")]
        [SerializeField] private TextMeshProUGUI[] normalTexts;    // 通常テキスト
        [SerializeField] private TextMeshProUGUI[] disabledTexts;  // 非活性テキスト

        [Header("区切り線（任意）")]
        [SerializeField] private Image[] separators;

        private void Start()
        {
            ApplyTheme();
        }

        /// <summary>現在設定されているテーマを適用する</summary>
        public void ApplyTheme()
        {
            if (theme == null) return;

            if (backgroundImage != null)
                backgroundImage.color = theme.windowBackground;

            if (borderImage != null)
                borderImage.color = theme.windowBorder;

            if (borderInnerImage != null)
                borderInnerImage.color = theme.windowBorderInner;

            if (normalTexts != null)
            {
                foreach (var text in normalTexts)
                {
                    if (text != null) text.color = theme.textNormal;
                }
            }

            if (disabledTexts != null)
            {
                foreach (var text in disabledTexts)
                {
                    if (text != null) text.color = theme.textDisabled;
                }
            }

            if (separators != null)
            {
                foreach (var sep in separators)
                {
                    if (sep != null) sep.color = theme.separator;
                }
            }
        }

        /// <summary>テーマを差し替えて適用する</summary>
        public void ApplyTheme(UIThemeSO newTheme)
        {
            theme = newTheme;
            ApplyTheme();
        }
    }
}
