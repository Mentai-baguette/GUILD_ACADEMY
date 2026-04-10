using UnityEngine;
using UnityEngine.UI;
using GuildAcademy.Data;

namespace GuildAcademy.UI
{
    /// <summary>
    /// 会話画面の立ち絵を制御するコンポーネント。
    /// 左右2体まで同時表示し、話し手を明るく・聞き手を暗転させる。
    ///
    /// 仕様:
    /// - 同時表示: 最大2体（左右）
    /// - 話している方が明るく、話していない方は暗転（輝度70%）
    /// - 感情差分: 通常/喜び/怒り/悲しみ/驚き（各キャラ5種）
    /// </summary>
    public class PortraitController : MonoBehaviour
    {
        [Header("立ち絵Image（Inspector接続）")]
        [SerializeField] private Image _portraitLeft;   // 左側の立ち絵
        [SerializeField] private Image _portraitRight;  // 右側の立ち絵

        [Header("立ち絵データベース")]
        [SerializeField] private CharacterPortraitDatabaseSO _portraitDatabase;

        [Header("暗転設定")]
        [Tooltip("非話者の輝度（0〜1）。仕様書では0.7")]
        [SerializeField] private float _inactiveBrightness = 0.7f;

        // 現在表示中のキャラ名
        private string _leftCharacter;
        private string _rightCharacter;

        private static readonly Color ActiveColor = Color.white;

        private Color InactiveColor => new Color(
            _inactiveBrightness,
            _inactiveBrightness,
            _inactiveBrightness,
            1f
        );

        private void Awake()
        {
            HideAll();
        }

        // ============================================================
        // 立ち絵をセットする
        // position: "left" or "right"
        // speakerName: キャラ名（CharacterPortraitSO.characterNameと一致）
        // emotion: 感情（"normal", "happy", "angry", "sad", "surprised"）
        // ============================================================
        public void SetPortrait(string position, string speakerName, string emotion = "normal")
        {
            if (_portraitDatabase == null) return;

            var portraitData = _portraitDatabase.GetByName(speakerName);
            if (portraitData == null)
            {
                // データベースに登録がないキャラ → 非表示
                ClearPortrait(position);
                return;
            }

            var sprite = portraitData.GetSprite(emotion);
            if (sprite == null)
            {
                ClearPortrait(position);
                return;
            }

            if (position == "left")
            {
                _leftCharacter = speakerName;
                _portraitLeft.sprite = sprite;
                _portraitLeft.gameObject.SetActive(true);
            }
            else if (position == "right")
            {
                _rightCharacter = speakerName;
                _portraitRight.sprite = sprite;
                _portraitRight.gameObject.SetActive(true);
            }
        }

        // ============================================================
        // 話し手をハイライトする（話し手は明るく、聞き手は暗転）
        // activeSpeaker: 現在話しているキャラ名
        // ============================================================
        public void HighlightSpeaker(string activeSpeaker)
        {
            if (_portraitLeft != null && _portraitLeft.gameObject.activeSelf)
            {
                _portraitLeft.color = (_leftCharacter == activeSpeaker)
                    ? ActiveColor
                    : InactiveColor;
            }

            if (_portraitRight != null && _portraitRight.gameObject.activeSelf)
            {
                _portraitRight.color = (_rightCharacter == activeSpeaker)
                    ? ActiveColor
                    : InactiveColor;
            }
        }

        // ============================================================
        // 指定位置の立ち絵をクリアする
        // ============================================================
        public void ClearPortrait(string position)
        {
            if (position == "left")
            {
                _leftCharacter = null;
                if (_portraitLeft != null)
                    _portraitLeft.gameObject.SetActive(false);
            }
            else if (position == "right")
            {
                _rightCharacter = null;
                if (_portraitRight != null)
                    _portraitRight.gameObject.SetActive(false);
            }
        }

        // ============================================================
        // 全立ち絵を非表示にする
        // ============================================================
        public void HideAll()
        {
            _leftCharacter = null;
            _rightCharacter = null;

            if (_portraitLeft != null)
                _portraitLeft.gameObject.SetActive(false);
            if (_portraitRight != null)
                _portraitRight.gameObject.SetActive(false);
        }

        /// <summary>指定キャラが現在表示されているか</summary>
        public bool IsDisplayed(string speakerName)
        {
            return _leftCharacter == speakerName || _rightCharacter == speakerName;
        }

        /// <summary>指定キャラが表示されている位置を返す。未表示ならnull。</summary>
        public string GetPosition(string speakerName)
        {
            if (_leftCharacter == speakerName) return "left";
            if (_rightCharacter == speakerName) return "right";
            return null;
        }

        /// <summary>左に空きがあるか</summary>
        public bool IsLeftEmpty => string.IsNullOrEmpty(_leftCharacter);

        /// <summary>右に空きがあるか</summary>
        public bool IsRightEmpty => string.IsNullOrEmpty(_rightCharacter);
    }
}
