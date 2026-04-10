using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Party
{
    /// <summary>
    /// 現在のパーティ（加入済みキャラ）を管理する。
    /// 全CharacterSOを丸読みするのではなく、ストーリー進行に応じて明示的にAdd/Removeする。
    /// FF10式メンバー入替、EXP配分、隊列管理に対応。
    /// </summary>
    public class PartyManager
    {
        private readonly List<CharacterStats> _members = new List<CharacterStats>();
        private readonly List<CharacterStats> _battleMembers = new List<CharacterStats>();
        private readonly List<CharacterStats> _reserveMembers = new List<CharacterStats>();
        private readonly Dictionary<CharacterStats, int> _turnCounts = new Dictionary<CharacterStats, int>();
        private readonly Dictionary<CharacterStats, FormationRow> _formations = new Dictionary<CharacterStats, FormationRow>();

        public const int MaxActiveMembers = 5;
        public const int MaxBattleMembers = 3;

        private CharacterStats _leader;
        private IATBResettable _atbSystem;
        private IsInLessonCheck _isInLessonCheck;

        public IReadOnlyList<CharacterStats> Members => _members.AsReadOnly();
        public int Count => _members.Count;
        public CharacterStats Leader => _leader;

        public event Action<CharacterStats> OnMemberAdded;
        public event Action<CharacterStats> OnMemberRemoved;
        public event Action<CharacterStats, CharacterStats> OnMemberSwapped;

        /// <summary>
        /// ATBシステムの参照を設定する。入替時のゲージリセットに使用。
        /// </summary>
        public void SetATBSystem(IATBResettable atbSystem)
        {
            _atbSystem = atbSystem;
        }

        /// <summary>
        /// 授業中判定デリゲートを設定する。
        /// </summary>
        public void SetScheduleCheck(IsInLessonCheck check)
        {
            _isInLessonCheck = check;
        }

        /// <summary>
        /// リーダーを設定する（例: レイ）。リーダーはRemoveできず、バトルパーティの先頭に固定。
        /// </summary>
        public void SetLeader(CharacterStats leader)
        {
            if (leader == null) throw new ArgumentNullException(nameof(leader));
            if (!_members.Contains(leader))
                throw new InvalidOperationException("Leader must be a party member. Call AddMember first.");
            _leader = leader;
        }

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

            // リーダーは削除不可
            if (_leader != null && _leader == member) return false;

            _members.Remove(member);
            _battleMembers.Remove(member);
            _reserveMembers.Remove(member);
            _turnCounts.Remove(member);
            _formations.Remove(member);
            OnMemberRemoved?.Invoke(member);
            return true;
        }

        /// <summary>
        /// バトル用にアクティブメンバーのリストを返す。
        /// SetBattleFormationで前衛が設定済みならその前衛メンバーを返す（授業中除外）。
        /// 未設定なら全メンバーから最大5人を返す（後方互換）。
        /// リーダーは先頭に配置。
        /// </summary>
        public List<CharacterStats> GetBattleParty()
        {
            List<CharacterStats> available;

            if (_battleMembers.Count > 0)
            {
                // SetBattleFormationで設定済み: 前衛メンバーを返す
                available = _battleMembers.Where(m => IsAvailableForParty(m)).ToList();
            }
            else
            {
                // 未設定: 全メンバーから最大5人（後方互換）
                available = _members.Where(m => IsAvailableForParty(m)).ToList();
                int count = Math.Min(available.Count, MaxActiveMembers);
                available = available.GetRange(0, count);
            }

            // リーダーを先頭に
            if (_leader != null && available.Contains(_leader))
            {
                available.Remove(_leader);
                available.Insert(0, _leader);
            }

            return available;
        }

        /// <summary>
        /// 前衛バトルメンバー（最大3人）を指定する。
        /// 残りは自動的に控えに回る。
        /// </summary>
        public void SetBattleFormation(List<CharacterStats> battle)
        {
            if (battle == null) throw new ArgumentNullException(nameof(battle));

            // null要素チェック
            foreach (var member in battle)
            {
                if (member == null)
                    throw new ArgumentException("Battle formation cannot contain null members.");
            }

            // 重複チェック
            var unique = new HashSet<CharacterStats>(battle);
            if (unique.Count != battle.Count)
                throw new ArgumentException("Battle formation cannot contain duplicate members.");

            // リーダー設定済みで編成に含まれていない場合は自動追加
            var effective = new List<CharacterStats>(battle);
            if (_leader != null && !effective.Contains(_leader) && _members.Contains(_leader))
            {
                effective.Insert(0, _leader);
            }

            if (effective.Count > MaxBattleMembers)
                throw new ArgumentException($"Battle members cannot exceed {MaxBattleMembers}.");

            _battleMembers.Clear();
            _reserveMembers.Clear();

            foreach (var member in effective)
            {
                if (!_members.Contains(member))
                    throw new ArgumentException($"{member.Name} is not a party member.");
                _battleMembers.Add(member);
            }

            // 残りのメンバーを控えに
            foreach (var member in _members)
            {
                if (!_battleMembers.Contains(member))
                    _reserveMembers.Add(member);
            }
        }

        /// <summary>
        /// バトルメンバーリストを返す。
        /// </summary>
        public IReadOnlyList<CharacterStats> GetBattleMembers()
        {
            return _battleMembers.AsReadOnly();
        }

        /// <summary>
        /// 控えメンバーリストを返す。
        /// </summary>
        public List<CharacterStats> GetReserveMembers()
        {
            return new List<CharacterStats>(_reserveMembers);
        }

        /// <summary>
        /// FF10式メンバー入替。バトルメンバーと控えメンバーを交換する。
        /// 入替後、入場メンバーのATBゲージは0にリセット。
        /// </summary>
        public bool SwapMember(int battleIndex, int reserveIndex)
        {
            if (battleIndex < 0 || battleIndex >= _battleMembers.Count)
                return false;
            if (reserveIndex < 0 || reserveIndex >= _reserveMembers.Count)
                return false;

            var outMember = _battleMembers[battleIndex];
            var inMember = _reserveMembers[reserveIndex];

            _battleMembers[battleIndex] = inMember;
            _reserveMembers[reserveIndex] = outMember;

            // 入場メンバーのATBゲージをリセット
            _atbSystem?.ResetGauge(inMember);

            OnMemberSwapped?.Invoke(outMember, inMember);
            return true;
        }

        /// <summary>
        /// 指定メンバーの出場ターン数を+1する。
        /// </summary>
        public void IncrementTurnCount(CharacterStats member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (_turnCounts.ContainsKey(member))
                _turnCounts[member]++;
            else
                _turnCounts[member] = 1;
        }

        /// <summary>
        /// 獲得EXPを出場ターン比率で配分する。
        /// 出場メンバー: totalEXP * (自分のターン数 / 全ターン数合計)
        /// 控えメンバー: 上記の半分
        /// 最低保証: 1 EXP
        /// </summary>
        public Dictionary<CharacterStats, int> DistributeEXP(int totalEXP)
        {
            var result = new Dictionary<CharacterStats, int>();
            if (totalEXP <= 0) return result;

            int totalTurns = 0;
            foreach (var kv in _turnCounts)
                totalTurns += kv.Value;

            if (totalTurns == 0)
            {
                // ターンカウントがない場合、全メンバーに最低保証
                foreach (var member in _members)
                    result[member] = 1;
                return result;
            }

            // 出場メンバー（ターンカウントがあるメンバー）
            var participated = new HashSet<CharacterStats>(_turnCounts.Keys);

            foreach (var member in _members)
            {
                int exp;
                if (participated.Contains(member))
                {
                    // 出場メンバー: totalEXP * (自分のターン数 / 全ターン数合計)
                    int turns = _turnCounts[member];
                    exp = (int)((double)totalEXP * turns / totalTurns);
                }
                else
                {
                    // 控えメンバー: 出場メンバーの平均EXPの半分
                    int avgBattleExp = totalEXP / participated.Count;
                    exp = avgBattleExp / 2;
                }

                exp = Math.Max(exp, 1); // 最低保証
                result[member] = exp;
            }

            return result;
        }

        /// <summary>
        /// バトル終了時にターンカウントをリセットする。
        /// </summary>
        public void ResetTurnCounts()
        {
            _turnCounts.Clear();
        }

        /// <summary>
        /// 指定メンバーがパーティ参加可能かどうか（授業中でない）。
        /// </summary>
        public bool IsAvailableForParty(CharacterStats member)
        {
            if (_isInLessonCheck == null) return true;
            return !_isInLessonCheck(member);
        }

        /// <summary>
        /// キャラクターの隊列（前列/後列）を設定する。
        /// </summary>
        public void SetFormation(CharacterStats member, FormationRow row)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            _formations[member] = row;
        }

        /// <summary>
        /// キャラクターの隊列を取得する。未設定の場合はFrontを返す。
        /// </summary>
        public FormationRow GetFormation(CharacterStats member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (_formations.TryGetValue(member, out var row))
                return row;
            return FormationRow.Front;
        }

        public void Clear()
        {
            _members.Clear();
            _battleMembers.Clear();
            _reserveMembers.Clear();
            _turnCounts.Clear();
            _formations.Clear();
            _leader = null;
            _isInLessonCheck = null;
            _atbSystem = null;
        }
    }
}
