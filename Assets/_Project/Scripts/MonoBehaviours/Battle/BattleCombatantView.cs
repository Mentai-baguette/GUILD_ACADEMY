using GuildAcademy.Core.Battle;
using GuildAcademy.Core.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    [DisallowMultipleComponent]
    public class BattleCombatantView : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _hpText;
        [SerializeField] private TextMeshProUGUI _mpText;
        [SerializeField] private TextMeshProUGUI _atbText;
        [SerializeField] private TextMeshProUGUI _breakText;

        [Header("Bars")]
        [SerializeField] private Image _hpFill;
        [SerializeField] private Image _mpFill;
        [SerializeField] private Image _atbFill;
        [SerializeField] private Image _breakFill;

        [Header("State")]
        [SerializeField] private GameObject _selectionFrame;
        [SerializeField] private GameObject _defeatedOverlay;

        private CharacterStats _combatant;
        private ATBSystem _atbSystem;
        private BreakSystem _breakSystem;

        public bool IsPartySlot { get; private set; }

        public void Bind(CharacterStats combatant, ATBSystem atbSystem, BreakSystem breakSystem, bool isPartySlot)
        {
            _combatant = combatant;
            _atbSystem = atbSystem;
            _breakSystem = breakSystem;
            IsPartySlot = isPartySlot;
            Refresh();
        }

        public void SetSelected(bool isSelected)
        {
            if (_selectionFrame != null)
            {
                _selectionFrame.SetActive(isSelected);
            }
        }

        public void Refresh()
        {
            bool hasCombatant = _combatant != null;

            if (_nameText != null)
            {
                _nameText.text = hasCombatant ? _combatant.Name : "Empty";
            }

            if (!hasCombatant)
            {
                SetEmptyBars();
                if (_defeatedOverlay != null)
                {
                    _defeatedOverlay.SetActive(false);
                }
                return;
            }

            float hpPercent = _combatant.MaxHp > 0 ? Mathf.Clamp01((float)_combatant.CurrentHp / _combatant.MaxHp) : 0f;
            float mpPercent = _combatant.MaxMp > 0 ? Mathf.Clamp01((float)_combatant.CurrentMp / _combatant.MaxMp) : 0f;
            float atbPercent = _atbSystem != null ? Mathf.Clamp01(_atbSystem.GetGauge(_combatant) / ATBSystem.MaxGauge) : 0f;
            float breakPercent = _breakSystem != null ? Mathf.Clamp01(_breakSystem.GetBreakGaugePercent(_combatant)) : 0f;

            if (_hpText != null)
            {
                _hpText.text = _combatant.CurrentHp + "/" + _combatant.MaxHp;
            }

            if (_mpText != null)
            {
                _mpText.text = _combatant.CurrentMp + "/" + _combatant.MaxMp;
            }

            if (_atbText != null)
            {
                _atbText.text = Mathf.RoundToInt(atbPercent * 100f) + "%";
            }

            if (_breakText != null)
            {
                _breakText.text = Mathf.RoundToInt(breakPercent * 100f) + "%";
            }

            if (_hpFill != null)
            {
                _hpFill.fillAmount = hpPercent;
            }

            if (_mpFill != null)
            {
                _mpFill.fillAmount = mpPercent;
            }

            if (_atbFill != null)
            {
                _atbFill.fillAmount = atbPercent;
            }

            if (_breakFill != null)
            {
                _breakFill.fillAmount = breakPercent;
            }

            if (_defeatedOverlay != null)
            {
                _defeatedOverlay.SetActive(_combatant.CurrentHp <= 0);
            }
        }

        private void SetEmptyBars()
        {
            if (_hpText != null)
            {
                _hpText.text = "0/0";
            }

            if (_mpText != null)
            {
                _mpText.text = "0/0";
            }

            if (_atbText != null)
            {
                _atbText.text = "0%";
            }

            if (_breakText != null)
            {
                _breakText.text = "0%";
            }

            if (_hpFill != null)
            {
                _hpFill.fillAmount = 0f;
            }

            if (_mpFill != null)
            {
                _mpFill.fillAmount = 0f;
            }

            if (_atbFill != null)
            {
                _atbFill.fillAmount = 0f;
            }

            if (_breakFill != null)
            {
                _breakFill.fillAmount = 0f;
            }
        }
    }
}