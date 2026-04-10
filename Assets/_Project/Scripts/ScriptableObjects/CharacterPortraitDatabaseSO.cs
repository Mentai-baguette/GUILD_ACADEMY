using System.Collections.Generic;
using UnityEngine;

namespace GuildAcademy.Data
{
    /// <summary>
    /// 全キャラクターの立ち絵データベース。
    /// Speaker名で検索し、対応するCharacterPortraitSOを返す。
    /// </summary>
    [CreateAssetMenu(fileName = "PortraitDatabase", menuName = "GuildAcademy/Portrait Database")]
    public class CharacterPortraitDatabaseSO : ScriptableObject
    {
        [SerializeField] private List<CharacterPortraitSO> _portraits = new List<CharacterPortraitSO>();

        // 高速検索用キャッシュ（ランタイム）
        private Dictionary<string, CharacterPortraitSO> _cache;

        /// <summary>Speaker名から立ち絵データを取得する。見つからなければnull。</summary>
        public CharacterPortraitSO GetByName(string speakerName)
        {
            if (string.IsNullOrEmpty(speakerName)) return null;

            // 初回アクセス時にキャッシュ構築
            if (_cache == null)
            {
                _cache = new Dictionary<string, CharacterPortraitSO>();
                foreach (var portrait in _portraits)
                {
                    if (portrait != null && !string.IsNullOrEmpty(portrait.characterName))
                    {
                        _cache[portrait.characterName] = portrait;
                    }
                }
            }

            _cache.TryGetValue(speakerName, out var result);
            return result;
        }

        /// <summary>キャッシュをクリアする（エディタ変更時など）</summary>
        public void ClearCache()
        {
            _cache = null;
        }
    }
}
