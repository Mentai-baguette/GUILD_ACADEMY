using UnityEngine;

namespace GuildAcademy.Data
{
    /// <summary>
    /// 章ごとのUIテーマカラーを定義するScriptableObject。
    /// 1章(Academy) → 2章(Shadow) → 3章(Abyss) → 4章(Ruin) と段階的にダーク化する。
    /// </summary>
    [CreateAssetMenu(fileName = "NewUITheme", menuName = "GuildAcademy/UI Theme")]
    public class UIThemeSO : ScriptableObject
    {
        [Header("テーマ情報")]
        public string themeName;
        [Range(1, 4)] public int chapter = 1;

        [Header("ウィンドウ")]
        public Color windowBackground = new Color(0.16f, 0.16f, 0.24f, 0.80f);
        public Color windowBorder = new Color(0.55f, 0.64f, 0.77f, 1f);
        public Color windowBorderInner = new Color(0.35f, 0.40f, 0.50f, 1f);

        [Header("テキスト")]
        public Color textNormal = new Color(0.91f, 0.89f, 0.85f, 1f);
        public Color textDisabled = new Color(0.42f, 0.42f, 0.42f, 1f);
        public Color textHighlight = Color.white;

        [Header("HPゲージ")]
        public Color hpFull = new Color(0.36f, 0.72f, 0.36f, 1f);
        public Color hpMid = new Color(1f, 0.76f, 0.03f, 1f);
        public Color hpLow = new Color(0.78f, 0.16f, 0.16f, 1f);

        [Header("MPゲージ")]
        public Color mpColor = new Color(0.36f, 0.75f, 0.87f, 1f);

        [Header("ATBゲージ")]
        public Color atbColor = new Color(1f, 0.84f, 0.31f, 1f);
        public Color atbReady = new Color(1f, 0.88f, 0.51f, 1f);

        [Header("ブレイクゲージ")]
        public Color breakColor = new Color(1f, 0.54f, 0.40f, 1f);

        [Header("リンクゲージ")]
        public Color linkLow = new Color(0.15f, 0.78f, 0.85f, 1f);
        public Color linkHigh = new Color(1f, 0.84f, 0f, 1f);

        [Header("侵蝕ゲージ")]
        public Color erosionNormal = new Color(0.26f, 0.65f, 0.96f, 1f);
        public Color erosionUnstable = new Color(1f, 0.79f, 0.16f, 1f);
        public Color erosionDangerous = new Color(1f, 0.44f, 0.26f, 1f);
        public Color erosionCritical = new Color(0.94f, 0.33f, 0.31f, 1f);

        [Header("ボタン — 背景")]
        public Color btnNormal = new Color(0.18f, 0.13f, 0.25f, 1f);
        public Color btnHover = new Color(0.24f, 0.16f, 0.38f, 1f);
        public Color btnPressed = new Color(0.10f, 0.06f, 0.19f, 1f);
        public Color btnDisabled = new Color(0.20f, 0.20f, 0.20f, 1f);

        [Header("ボタン — 枠")]
        public Color btnBorderNormal = new Color(0.55f, 0.64f, 0.77f, 1f);
        public Color btnBorderHover = new Color(0.65f, 0.74f, 0.87f, 1f);
        public Color btnBorderPressed = new Color(0.35f, 0.40f, 0.50f, 1f);
        public Color btnBorderDisabled = new Color(0.33f, 0.33f, 0.33f, 1f);

        [Header("タブ")]
        public Color tabSelected = Color.white;
        public Color tabUnselected = new Color(1f, 1f, 1f, 0.4f);
        public Color tabAccent = new Color(0.55f, 0.64f, 0.77f, 1f);

        [Header("共通")]
        public Color gaugeBackground = new Color(0.05f, 0.05f, 0.10f, 0.60f);
        public Color gaugeFrame = new Color(0.55f, 0.64f, 0.77f, 0.80f);
        public Color separator = new Color(0.55f, 0.64f, 0.77f, 0.30f);
        public Color cursorColor = new Color(0.55f, 0.64f, 0.77f, 1f);
    }
}
