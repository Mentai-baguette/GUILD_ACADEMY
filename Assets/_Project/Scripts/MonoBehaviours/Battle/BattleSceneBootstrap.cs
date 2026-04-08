using System;
using System.Collections.Generic;
using System.Linq;
using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Battle
{
    [DisallowMultipleComponent]
    public class BattleSceneBootstrap : MonoBehaviour
    {
        private const string PartyKey = "battle.party";
        private const string EnemyKey = "battle.enemies";

        [Header("Scene Bindings")]
        [SerializeField] private BattleUIManager _uiManager;
        [SerializeField] private Transform _partyRoot;
        [SerializeField] private Transform _enemyRoot;
        [SerializeField] private BattleBackdropController _backdropController;

        [Header("Fallback Layout")]
        [SerializeField] private bool _useFallbackBattleData = true;

        private ATBSystem _atbSystem;
        private BreakSystem _breakSystem;
        private DamageCalculator _damageCalculator;
        private ActionExecutor _actionExecutor;
        private BattleFlowController _battleFlowController;

        private readonly List<CharacterStats> _party = new List<CharacterStats>();
        private readonly List<CharacterStats> _enemies = new List<CharacterStats>();

        private void Awake()
        {
            InitializeBattle();
        }

        private void Update()
        {
            if (_battleFlowController == null)
                return;

            _battleFlowController.Tick(Time.deltaTime);

            if (_uiManager != null)
            {
                _uiManager.RefreshViews();
            }
        }

        private void InitializeBattle()
        {
            _party.Clear();
            _enemies.Clear();

            ReadSceneData();

            if (_party.Count == 0 && _useFallbackBattleData)
            {
                _party.Add(CreateCharacter("Ray", 220, 60, 34, 20, 11, ElementType.None));
                _party.Add(CreateCharacter("Yuna", 180, 80, 24, 18, 12, ElementType.None));
                _party.Add(CreateCharacter("Mio", 160, 90, 30, 14, 13, ElementType.None));
                _party.Add(CreateCharacter("Kaito", 240, 30, 38, 24, 9, ElementType.None));
            }

            if (_enemies.Count == 0 && _useFallbackBattleData)
            {
                _enemies.Add(CreateCharacter("Shion", 320, 50, 40, 26, 10, ElementType.None));
            }

            _atbSystem = new ATBSystem();
            _breakSystem = new BreakSystem();
            _damageCalculator = new DamageCalculator(new UnityRandomAdapter());
            _actionExecutor = new ActionExecutor(_damageCalculator, _breakSystem, new UnityRandomAdapter());
            _battleFlowController = new BattleFlowController(_atbSystem, _actionExecutor, _breakSystem);

            if (_backdropController != null)
            {
                _backdropController.ApplyBattlePhase(GetBattlePhase());
            }

            if (_uiManager != null)
            {
                _uiManager.Bind(_battleFlowController, _party, _enemies, _atbSystem, _breakSystem);
            }

            _battleFlowController.StartBattle(_party, _enemies);
        }

        private void ReadSceneData()
        {
            if (SceneTransitionData.Has(PartyKey))
            {
                AppendCombatants(SceneTransitionData.Get<object>(PartyKey));
            }

            if (SceneTransitionData.Has(EnemyKey))
            {
                AppendCombatants(SceneTransitionData.Get<object>(EnemyKey), _enemies);
            }
        }

        private void AppendCombatants(object data, List<CharacterStats> target = null)
        {
            target ??= _party;

            if (data is CharacterStats single)
            {
                target.Add(single);
                return;
            }

            if (data is IEnumerable<CharacterStats> enumerable)
            {
                target.AddRange(enumerable.Where(c => c != null));
            }
        }

        private BattlePhase GetBattlePhase()
        {
            if (_enemies.Count == 0)
                return BattlePhase.AcademyLife;

            return _enemies.Any(e => e.Name == "Carlos") ? BattlePhase.CarlosBattle : BattlePhase.ShionPhase1;
        }

        private static CharacterStats CreateCharacter(string name, int maxHp, int maxMp, int atk, int def, int spd, ElementType element)
        {
            return new CharacterStats(name, maxHp, maxMp, atk, def, spd, element);
        }

        private sealed class UnityRandomAdapter : IRandom
        {
            public int Range(int minInclusive, int maxExclusive)
            {
                return UnityEngine.Random.Range(minInclusive, maxExclusive);
            }
        }
    }
}