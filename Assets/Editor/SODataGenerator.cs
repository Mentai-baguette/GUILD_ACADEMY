#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using GuildAcademy.Data;
using GuildAcademy.Core.Data;

namespace GuildAcademy.Editor
{
    public static class SODataGenerator
    {
        [MenuItem("GuildAcademy/Generate All SO Data")]
        public static void GenerateAll()
        {
            GenerateSkills();
            GenerateCharacters();
            GenerateEnemies();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SODataGenerator] All SO data generated successfully.");
        }

        [MenuItem("GuildAcademy/Generate Skills")]
        public static void GenerateSkills()
        {
            CreateSkill("Skill_Slash", "スラッシュ", "基本的な斬撃攻撃", 100, 0, ElementType.None, SkillTargetType.SingleEnemy, false, false, 10);
            CreateSkill("Skill_HeavyStrike", "ヘビーストライク", "力を込めた一撃", 150, 8, ElementType.None, SkillTargetType.SingleEnemy, false, false, 20);
            CreateSkill("Skill_DarkSlash", "ダークスラッシュ", "闇を纏った斬撃", 130, 12, ElementType.Dark, SkillTargetType.SingleEnemy, false, false, 15);
            CreateSkill("Skill_Heal", "ヒール", "味方一人のHPを回復", 80, 10, ElementType.Light, SkillTargetType.SingleAlly, true, true, 0);
            CreateSkill("Skill_AreaHeal", "エリアヒール", "味方全体のHPを回復", 50, 20, ElementType.Light, SkillTargetType.AllAllies, true, true, 0);
            CreateSkill("Skill_HolyLight", "ホーリーライト", "光属性の魔法攻撃", 120, 15, ElementType.Light, SkillTargetType.SingleEnemy, false, true, 10);
            CreateSkill("Skill_Fireball", "ファイアボール", "炎属性の魔法攻撃", 130, 14, ElementType.Fire, SkillTargetType.SingleEnemy, false, true, 15);
            CreateSkill("Skill_Inferno", "インフェルノ", "炎属性の全体魔法", 90, 22, ElementType.Fire, SkillTargetType.AllEnemies, false, true, 10);
            CreateSkill("Skill_WindBlade", "ウィンドブレード", "風属性の魔法攻撃", 120, 12, ElementType.Wind, SkillTargetType.SingleEnemy, false, true, 12);
            CreateSkill("Skill_Gale", "ゲイル", "風属性の全体魔法", 85, 20, ElementType.Wind, SkillTargetType.AllEnemies, false, true, 8);
            CreateSkill("Skill_IceLance", "アイスランス", "氷属性の魔法攻撃", 130, 14, ElementType.Ice, SkillTargetType.SingleEnemy, false, true, 15);
            CreateSkill("Skill_Blizzard", "ブリザード", "氷属性の全体魔法", 90, 24, ElementType.Ice, SkillTargetType.AllEnemies, false, true, 10);
            CreateSkill("Skill_StoneWall", "ストーンウォール", "自身の防御力を上昇", 0, 10, ElementType.Earth, SkillTargetType.Self, false, true, 0);
            CreateSkill("Skill_EarthShake", "アースシェイク", "地属性の全体攻撃", 95, 22, ElementType.Earth, SkillTargetType.AllEnemies, false, true, 12);
            CreateSkill("Skill_ShadowBind", "シャドウバインド", "闇属性の魔法攻撃", 110, 16, ElementType.Dark, SkillTargetType.SingleEnemy, false, true, 10);
            CreateSkill("Skill_QuickSlash", "クイックスラッシュ", "素早い連続斬り", 80, 6, ElementType.None, SkillTargetType.SingleEnemy, false, false, 8);
            CreateSkill("Skill_Protect", "プロテクト", "味方一人の防御力を上昇", 0, 8, ElementType.None, SkillTargetType.SingleAlly, false, true, 0);
            // Enemy skills
            CreateSkill("Skill_Bite", "かみつき", "噛みつき攻撃", 90, 0, ElementType.None, SkillTargetType.SingleEnemy, false, false, 8);
            CreateSkill("Skill_Tackle", "たいあたり", "体当たり攻撃", 80, 0, ElementType.None, SkillTargetType.SingleEnemy, false, false, 5);
            CreateSkill("Skill_PoisonBreath", "毒のブレス", "毒属性のブレス攻撃", 70, 5, ElementType.None, SkillTargetType.AllEnemies, false, false, 0);
            CreateSkill("Skill_DarkPulse", "ダークパルス", "闇属性の波動攻撃", 140, 20, ElementType.Dark, SkillTargetType.AllEnemies, false, true, 15);
            CreateSkill("Skill_AbyssJudge", "深淵の裁き", "闇属性の強力な魔法", 200, 30, ElementType.Dark, SkillTargetType.SingleEnemy, false, true, 25);
            Debug.Log("[SODataGenerator] Skills generated.");
        }

        [MenuItem("GuildAcademy/Generate Characters")]
        public static void GenerateCharacters()
        {
            // Lv1 base stats from docs/specs/character-stats-table.md
            CreateCharacter(CharacterId.Ray, "レイ", "主人公。闇属性の剣士",
                105, 25, 12, 10, 11, 9, 10, 10, 100, 1,
                ElementType.Dark, ElementType.Light, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Yuna, "ユナ", "光属性のヒーラー",
                90, 35, 6, 8, 14, 14, 9, 8, 110, 1,
                ElementType.Light, ElementType.Dark, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Mio, "ミオ", "炎属性の魔法使い",
                82, 38, 6, 6, 16, 10, 10, 9, 95, 1,
                ElementType.Fire, ElementType.Ice, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Kaito, "カイト", "無属性のタンク戦士",
                120, 15, 16, 11, 5, 7, 8, 9, 90, 1,
                ElementType.None, ElementType.None, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Shion, "シオン", "光属性のオールラウンダー",
                110, 30, 11, 11, 12, 11, 11, 11, 105, 1,
                ElementType.Light, ElementType.Dark, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Rin, "リン", "地属性のタンク",
                130, 18, 7, 16, 6, 12, 5, 7, 95, 1,
                ElementType.Earth, ElementType.Wind, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Vein, "ヴェイン", "闇属性のデバッファー",
                95, 30, 8, 8, 13, 9, 9, 15, 82, 1,
                ElementType.Dark, ElementType.Light, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Mel, "メル", "風属性のサポーター",
                95, 32, 7, 9, 11, 13, 10, 10, 115, 1,
                ElementType.Wind, ElementType.Earth, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Jin, "ジン", "風属性のスピードアタッカー",
                80, 20, 13, 6, 7, 6, 16, 14, 90, 1,
                ElementType.Wind, ElementType.Earth, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Setsuna, "セツナ", "氷属性の魔法使い",
                82, 40, 5, 6, 16, 11, 8, 10, 100, 1,
                ElementType.Ice, ElementType.Fire, ElementType.None, ElementType.None);

            CreateCharacter(CharacterId.Renji, "レンジ", "炎属性の物理アタッカー",
                100, 18, 14, 9, 8, 7, 13, 11, 88, 1,
                ElementType.Fire, ElementType.Ice, ElementType.None, ElementType.None);

            Debug.Log("[SODataGenerator] Characters generated.");
        }

        [MenuItem("GuildAcademy/Generate Enemies")]
        public static void GenerateEnemies()
        {
            // Chapter 1 dungeon enemies (Training Ground Lv1-8)
            CreateEnemy("Enemy_Slime", "スライム", "最弱のモンスター", false, BattlePhase.PreAcademy,
                30, 5, 5, 3, 2, 2, 4, 3, 80, 1,
                ElementType.None, ElementType.Fire, ElementType.None, ElementType.None,
                50, 10, 5, 2, new[] { "Skill_Tackle" });

            CreateEnemy("Enemy_Goblin", "ゴブリン", "小柄だが狡猾な魔物", false, BattlePhase.PreAcademy,
                45, 8, 8, 5, 3, 3, 6, 5, 85, 3,
                ElementType.None, ElementType.None, ElementType.None, ElementType.None,
                60, 15, 8, 4, new[] { "Skill_Slash", "Skill_Tackle" });

            CreateEnemy("Enemy_Wolf", "オオカミ", "素早い獣", false, BattlePhase.PreAcademy,
                40, 5, 10, 4, 2, 3, 10, 7, 90, 4,
                ElementType.None, ElementType.Ice, ElementType.None, ElementType.None,
                60, 12, 7, 3, new[] { "Skill_Bite", "Skill_QuickSlash" });

            CreateEnemy("Enemy_Bat", "コウモリ", "暗闇に潜む飛行種", false, BattlePhase.PreAcademy,
                25, 10, 6, 3, 5, 4, 12, 8, 90, 2,
                ElementType.Dark, ElementType.Light, ElementType.None, ElementType.Dark,
                50, 8, 5, 2, new[] { "Skill_Bite" });

            CreateEnemy("Enemy_Treant", "トレント", "森の番人。硬い", false, BattlePhase.PreAcademy,
                80, 10, 7, 12, 4, 6, 3, 4, 75, 6,
                ElementType.Earth, ElementType.Fire, ElementType.Earth, ElementType.None,
                80, 20, 12, 6, new[] { "Skill_Tackle", "Skill_EarthShake" });

            CreateEnemy("Enemy_Mushroom", "ポイズンマッシュ", "毒胞子を撒く菌類", false, BattlePhase.PreAcademy,
                35, 15, 4, 5, 7, 5, 3, 4, 80, 5,
                ElementType.Earth, ElementType.Fire, ElementType.None, ElementType.None,
                55, 10, 6, 3, new[] { "Skill_PoisonBreath" });

            // Shion Boss (3 phases)
            CreateEnemy("Boss_Shion_Phase1", "シオン【堕天の騎士】", "闇に堕ちたシオンの第1形態", true, BattlePhase.ShionPhase1,
                12000, 500, 55, 45, 50, 40, 42, 40, 100, 55,
                ElementType.Dark, ElementType.None, ElementType.None, ElementType.Dark,
                200, 50, 0, 0, new[] { "Skill_DarkSlash", "Skill_DarkPulse", "Skill_HeavyStrike" });

            CreateEnemy("Boss_Shion_Phase2", "シオン【深淵の裁定者】", "完全闇化したシオンの第2形態", true, BattlePhase.ShionPhase2,
                20000, 800, 70, 55, 65, 50, 50, 45, 100, 75,
                ElementType.Dark, ElementType.None, ElementType.None, ElementType.Dark,
                300, 80, 0, 0, new[] { "Skill_AbyssJudge", "Skill_DarkPulse", "Skill_DarkSlash" });

            Debug.Log("[SODataGenerator] Enemies generated.");
        }

        private static void CreateSkill(string fileName, string skillName, string desc,
            int power, int mpCost, ElementType element, SkillTargetType targetType,
            bool isHealing, bool isMagic, int breakValue)
        {
            string path = $"Assets/Resources/Data/Skills/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<SkillDataSO>(path);
            if (existing != null) return;

            var so = ScriptableObject.CreateInstance<SkillDataSO>();
            so.skillName = skillName;
            so.description = desc;
            so.power = power;
            so.mpCost = mpCost;
            so.element = element;
            so.targetType = targetType;
            so.isHealing = isHealing;
            so.isMagic = isMagic;
            so.breakValue = breakValue;

            EnsureDirectory("Assets/Resources/Data/Skills");
            AssetDatabase.CreateAsset(so, path);
        }

        private static void CreateCharacter(CharacterId id, string charName, string desc,
            int hp, int mp, int atk, int def, int intStat, int res, int agi, int dex, int luk, int level,
            ElementType element, ElementType weak, ElementType resist, ElementType nullEl)
        {
            string path = $"Assets/Resources/Data/Characters/Char_{id}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<CharacterDataSO>(path);
            if (existing != null) return;

            var so = ScriptableObject.CreateInstance<CharacterDataSO>();
            so.id = id;
            so.characterName = charName;
            so.description = desc;
            so.maxHp = hp;
            so.maxMp = mp;
            so.atk = atk;
            so.def = def;
            so.intStat = intStat;
            so.res = res;
            so.agi = agi;
            so.dex = dex;
            so.luk = luk;
            so.level = level;
            so.element = element;
            so.weakElement = weak;
            so.resistElement = resist;
            so.nullElement = nullEl;

            EnsureDirectory("Assets/Resources/Data/Characters");
            AssetDatabase.CreateAsset(so, path);
        }

        private static void CreateEnemy(string fileName, string enemyName, string desc,
            bool isBoss, BattlePhase bossPhase,
            int hp, int mp, int atk, int def, int intStat, int res, int agi, int dex, int luk, int level,
            ElementType element, ElementType weak, ElementType resist, ElementType nullEl,
            int breakMax, int breakWeakHit, int expReward, int goldReward,
            string[] skillFileNames)
        {
            string path = $"Assets/Resources/Data/Enemies/{fileName}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<EnemyDataSO>(path);
            if (existing != null) return;

            var so = ScriptableObject.CreateInstance<EnemyDataSO>();
            so.enemyName = enemyName;
            so.description = desc;
            so.isBoss = isBoss;
            so.bossPhase = bossPhase;
            so.maxHp = hp;
            so.maxMp = mp;
            so.atk = atk;
            so.def = def;
            so.intStat = intStat;
            so.res = res;
            so.agi = agi;
            so.dex = dex;
            so.luk = luk;
            so.level = level;
            so.element = element;
            so.weakElement = weak;
            so.resistElement = resist;
            so.nullElement = nullEl;
            so.breakGaugeMax = breakMax;
            so.breakWeakHitValue = breakWeakHit;
            so.expReward = expReward;
            so.goldReward = goldReward;

            // Link skill references
            var skills = new SkillDataSO[skillFileNames.Length];
            for (int i = 0; i < skillFileNames.Length; i++)
            {
                skills[i] = AssetDatabase.LoadAssetAtPath<SkillDataSO>(
                    $"Assets/Resources/Data/Skills/{skillFileNames[i]}.asset");
            }
            so.skills = skills;

            EnsureDirectory("Assets/Resources/Data/Enemies");
            AssetDatabase.CreateAsset(so, path);
        }

        private static void EnsureDirectory(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = System.IO.Path.GetDirectoryName(path).Replace("\\", "/");
                string folder = System.IO.Path.GetFileName(path);
                if (!AssetDatabase.IsValidFolder(parent))
                    EnsureDirectory(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }
}
#endif
