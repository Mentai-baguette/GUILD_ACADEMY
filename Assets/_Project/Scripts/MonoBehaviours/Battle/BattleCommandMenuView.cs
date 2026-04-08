using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GuildAcademy.MonoBehaviours.Battle
{
    [DisallowMultipleComponent]
    public class BattleCommandMenuView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _actorLabel;
        [SerializeField] private Button _attackButton;
        [SerializeField] private Button _skillButton;
        [SerializeField] private Button _itemButton;
        [SerializeField] private Button _defendButton;

        public event Action OnAttackRequested;
        public event Action OnSkillRequested;
        public event Action OnItemRequested;
        public event Action OnDefendRequested;

        private void Awake()
        {
            if (_attackButton != null)
            {
                _attackButton.onClick.AddListener(() => OnAttackRequested?.Invoke());
            }

            if (_skillButton != null)
            {
                _skillButton.onClick.AddListener(() => OnSkillRequested?.Invoke());
            }

            if (_itemButton != null)
            {
                _itemButton.onClick.AddListener(() => OnItemRequested?.Invoke());
            }

            if (_defendButton != null)
            {
                _defendButton.onClick.AddListener(() => OnDefendRequested?.Invoke());
            }

            SetVisible(false);
        }

        public void SetVisible(bool isVisible)
        {
            if (_root != null)
            {
                _root.SetActive(isVisible);
            }
        }

        public void SetActor(string actorName)
        {
            if (_actorLabel != null)
            {
                _actorLabel.text = actorName + " のコマンド";
            }
        }
    }
}