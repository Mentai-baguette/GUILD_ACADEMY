using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// テーマ対応ボタンコンポーネント。
    /// UIThemeSOから4状態（Normal/Hover/Pressed/Disabled）の色を取得して
    /// UnityのButton.ColorBlockに適用する。
    /// 枠色もポインタイベントに連動して4状態を切り替える。
    /// タブモードでは選択/非選択の2状態切替に対応。
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ThemedButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler
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

        /// <summary>interactable変更時に枠色・テキスト色を再適用する</summary>
        public void RefreshState()
        {
            if (theme == null || _button == null) return;

            if (mode == ButtonMode.Standard)
            {
                UpdateBorderColor();
                if (labelText != null)
                    labelText.color = _button.interactable ? theme.textNormal : theme.textDisabled;
            }
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

            // 枠色（初期状態）
            UpdateBorderColor();

            // テキスト色
            if (labelText != null)
                labelText.color = _button.interactable ? theme.textNormal : theme.textDisabled;
        }

        private void UpdateBorderColor()
        {
            if (borderImage == null || theme == null) return;

            if (_button != null && !_button.interactable)
            {
                borderImage.color = theme.btnBorderDisabled;
                return;
            }

            if (_isPressed)
                borderImage.color = theme.btnBorderPressed;
            else if (_isHovered)
                borderImage.color = theme.btnBorderHover;
            else
                borderImage.color = theme.btnBorderNormal;
        }

        private bool _isHovered;
        private bool _isPressed;

        public void OnPointerEnter(PointerEventData eventData)
        {
            _isHovered = true;
            if (mode == ButtonMode.Standard) UpdateBorderColor();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _isHovered = false;
            _isPressed = false;
            if (mode == ButtonMode.Standard) UpdateBorderColor();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _isPressed = true;
            if (mode == ButtonMode.Standard) UpdateBorderColor();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isPressed = false;
            if (mode == ButtonMode.Standard) UpdateBorderColor();
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
