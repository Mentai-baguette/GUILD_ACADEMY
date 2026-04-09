using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GuildAcademy.Core.Dialogue;

namespace GuildAcademy.UI
{
    public class ChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject _choicePanel;
        [SerializeField] private Transform _buttonContainer;
        [SerializeField] private GameObject _choiceButtonPrefab;

        private Action<int> _onChoiceSelected;
        private readonly List<GameObject> _spawnedButtons = new List<GameObject>();

        private void Awake()
        {
            if (_choicePanel != null)
                _choicePanel.SetActive(false);
        }

        public void Show(List<DialogueChoice> choices, Action<int> onSelected)
        {
            _onChoiceSelected = onSelected;
            ClearButtons();

            if (_choicePanel != null)
                _choicePanel.SetActive(true);

            for (int i = 0; i < choices.Count; i++)
            {
                var btnObj = Instantiate(_choiceButtonPrefab, _buttonContainer);
                btnObj.SetActive(true);

                var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                    label.text = choices[i].Text;

                int index = i;
                var button = btnObj.GetComponent<Button>();
                if (button != null)
                    button.onClick.AddListener(() => SelectChoice(index));

                _spawnedButtons.Add(btnObj);
            }
        }

        public void Hide()
        {
            ClearButtons();
            if (_choicePanel != null)
                _choicePanel.SetActive(false);
        }

        private void SelectChoice(int index)
        {
            _onChoiceSelected?.Invoke(index);
        }

        private void ClearButtons()
        {
            foreach (var btn in _spawnedButtons)
            {
                if (btn != null) Destroy(btn);
            }
            _spawnedButtons.Clear();
        }
    }
}
