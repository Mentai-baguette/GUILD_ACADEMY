using System;
using System.Collections.Generic;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Party
{
    /// <summary>
    /// 現在のパーティ（加入済みキャラ）を管理する。
    /// 全CharacterSOを丸読みするのではなく、ストーリー進行に応じて明示的にAdd/Removeする。
    /// </summary>
    public class PartyManager
    {
        private readonly List<CharacterStats> _members = new List<CharacterStats>();
        public const int MaxActiveMembers = 5;

        public IReadOnlyList<CharacterStats> Members => _members;
        public int Count => _members.Count;

        public event Action<CharacterStats> OnMemberAdded;
        public event Action<CharacterStats> OnMemberRemoved;

        public void AddMember(CharacterStats stats)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            if (_members.Exists(m => m.Name == stats.Name)) return; // 重複防止
            _members.Add(stats);
            OnMemberAdded?.Invoke(stats);
        }

        public bool RemoveMember(string name)
        {
            var member = _members.Find(m => m.Name == name);
            if (member == null) return false;
            _members.Remove(member);
            OnMemberRemoved?.Invoke(member);
            return true;
        }

        /// <summary>
        /// バトル用にアクティブメンバー（最大5人）のリストを返す。
        /// </summary>
        public List<CharacterStats> GetBattleParty()
        {
            int count = Math.Min(_members.Count, MaxActiveMembers);
            return _members.GetRange(0, count);
        }

        public void Clear()
        {
            _members.Clear();
        }
    }
}
