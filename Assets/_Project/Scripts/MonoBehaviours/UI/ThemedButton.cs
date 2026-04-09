using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// テーマ対応ボタンコンポーネント。
    /// UIThemeSOから4状態（Normal/Hover/Pressed/Disabled）の色を取得して
    /// UnityのButton.ColorBlockに適用する。
    /// タブモードでは選択/非選択の2状態切替に対応。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ThemedButton : MonoBehaviour
    {
        public enum ButtonMode
        {
            Standard,  // 汎用ボタン（4状態）
            Tab        // タブボタン（選択/非選択）
        }

        [Header("テーマ")]
        [SerializeField] private UIThemeSO theme;

        [Header("モード")]
        [SerializeField] private ButtonMode mode = ButtonMode.Standard;

        [Header("UIコンポーネント")]
        [SerializeField] private Image borderImage;           // ボタン枠（任意）
        [SerializeField] private TextMeshProUGUI labelText;   // ボタンテキスト（任意）
        [SerializeField] private Image tabAccentLine;         // タブモード用の下線（任意）

        private Button _button;
        private bool _isTabSelected;

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        private void Start()
        {
            ApplyTheme();
        }

        /// <summary>現在設定されているテーマを適用する</summary>
        public void ApplyTheme()
        {
            if (theme == null || _button == null) return;

            switch (mode)
            {
                case ButtonMode.Standard:
                    ApplyStandardTheme();
                    break;
                case ButtonMode.Tab:
                    ApplyTabTheme();
                    break;
            }
        }

        /// <summary>テーマを差し替えて適用する</summary>
        public void ApplyTheme(UIThemeSO newTheme)
        {
            theme = newTheme;
            ApplyTheme();
        }

        /// <summary>タブの選択状態を設定する（Tabモード用）</summary>
        public void SetTabSelected(bool selected)
        {
            _isTabSelected = selected;
            if (mode == ButtonMode.Tab)
            {
                ApplyTabTheme();
            }
        }

        private void ApplyStandardTheme()
        {
            // Button.ColorBlock に4状態カラーを適用
            var colors = _button.colors;
            colors.normalColor = theme.btnNormal;
            colors.highlightedColor = theme.btnHover;
            colors.pressedColor = theme.btnPressed;
            colors.disabledColor = theme.btnDisabled;
            colors.colorMultiplier = 1f;
            _button.colors = colors;

            // 枠色
            if (borderImage != null)
                borderImage.color = theme.btnBorderNormal;

            // テキスト色
            if (labelText != null)
                labelText.color = _button.interactable ? theme.textNormal : theme.textDisabled;
        }

        private void ApplyTabTheme()
        {
            if (_isTabSelected)
            {
                // 選択中タブ
                if (labelText != null)
                    labelText.color = theme.tabSelected;

                if (tabAccentLine != null)
                {
                    tabAccentLine.gameObject.SetActive(true);
                    tabAccentLine.color = theme.tabAccent;
                }
            }
            else
            {
                // 非選択タブ
                if (labelText != null)
                    labelText.color = theme.tabUnselected;

                if (tabAccentLine != null)
                    tabAccentLine.gameObject.SetActive(false);
            }
        }
    }
}
