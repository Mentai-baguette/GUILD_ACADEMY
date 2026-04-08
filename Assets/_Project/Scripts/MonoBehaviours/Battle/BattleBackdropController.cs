using GuildAcademy.Core.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    [DisallowMultipleComponent]
    public class BattleBackdropController : MonoBehaviour
    {
        [SerializeField] private Image _backgroundImage;
        [SerializeField] private Sprite _academyBackdrop;
        [SerializeField] private Sprite _shionBackdrop;
        [SerializeField] private Sprite _carlosBackdrop;
        [SerializeField] private Color _fallbackColor = new Color(0.11f, 0.11f, 0.16f, 1f);

        public void ApplyBattlePhase(BattlePhase battlePhase)
        {
            if (_backgroundImage == null)
                return;

            switch (battlePhase)
            {
                case BattlePhase.ShionPhase1:
                case BattlePhase.ShionPhase2:
                    _backgroundImage.sprite = _shionBackdrop;
                    _backgroundImage.color = Color.white;
                    break;
                case BattlePhase.CarlosBattle:
                    _backgroundImage.sprite = _carlosBackdrop;
                    _backgroundImage.color = Color.white;
                    break;
                default:
                    _backgroundImage.sprite = _academyBackdrop;
                    _backgroundImage.color = _academyBackdrop != null ? Color.white : _fallbackColor;
                    break;
            }
        }
    }
}