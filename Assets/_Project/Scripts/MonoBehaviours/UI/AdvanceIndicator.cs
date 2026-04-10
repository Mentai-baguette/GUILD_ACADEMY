using UnityEngine;

namespace GuildAcademy.UI
{
    /// <summary>
    /// 会話ウィンドウの「▼」送りマーク。
    /// テキスト表示完了時に点滅表示し、入力待ちを示す。
    /// </summary>
    public class AdvanceIndicator : MonoBehaviour
    {
        [Header("点滅設定")]
        [Tooltip("点滅1サイクルの秒数")]
        [SerializeField] private float _blinkInterval = 0.5f;

        private CanvasGroup _canvasGroup;
        private float _timer;
        private bool _isVisible;

        private void Awake()
        {
            EnsureCanvasGroup();
        }

        /// <summary>CanvasGroupがなければ取得or追加する</summary>
        private void EnsureCanvasGroup()
        {
            if (_canvasGroup != null) return;
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Update()
        {
            if (!_isVisible) return;

            _timer += Time.deltaTime;
            if (_timer >= _blinkInterval)
            {
                _timer = 0f;
                // alpha を 0 ↔ 1 でトグル
                _canvasGroup.alpha = _canvasGroup.alpha > 0.5f ? 0f : 1f;
            }
        }

        /// <summary>送りマークを表示して点滅開始</summary>
        public void Show()
        {
            gameObject.SetActive(true);
            EnsureCanvasGroup();
            _isVisible = true;
            _canvasGroup.alpha = 1f;
            _timer = 0f;
        }

        /// <summary>送りマークを非表示にする</summary>
        public void Hide()
        {
            _isVisible = false;
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
