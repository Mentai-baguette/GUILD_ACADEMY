using System;
using System.Collections.Generic;
using UnityEngine;

namespace GuildAcademy.Data
{
    /// <summary>
    /// キャラクターごとの立ち絵スプライトを管理するScriptableObject。
    /// キャラ名をキーに、感情差分（通常/喜び/怒り/悲しみ/驚き）のスプライトを返す。
    /// </summary>
    [CreateAssetMenu(fileName = "NewCharacterPortrait", menuName = "GuildAcademy/Character Portrait")]
    public class CharacterPortraitSO : ScriptableObject
    {
        [Header("キャラクター情報")]
        public string characterName;  // DialogueEntry.Speaker と一致させる

        [Header("立ち絵スプライト（感情差分）")]
        public Sprite normal;
        public Sprite happy;
        public Sprite angry;
        public Sprite sad;
        public Sprite surprised;

        /// <summary>感情名からスプライトを取得する。不明な場合はnormalを返す。</summary>
        public Sprite GetSprite(string emotion = "normal")
        {
            return emotion?.ToLower() switch
            {
                "happy" => happy != null ? happy : normal,
                "angry" => angry != null ? angry : normal,
                "sad" => sad != null ? sad : normal,
                "surprised" => surprised != null ? surprised : normal,
                _ => normal
            };
        }
    }
}
