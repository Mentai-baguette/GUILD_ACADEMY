using UnityEngine;
using TMPro;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class NPCInteractionPrompt : MonoBehaviour
    {
        [SerializeField] private GameObject _promptObject;
        [SerializeField] private TextMeshProUGUI _promptText;
        [SerializeField] private string _defaultText = "E: 話す";

        private void Awake()
        {
            if (_promptObject != null)
                _promptObject.SetActive(false);
        }

        public void Show()
        {
            if (_promptObject != null)
                _promptObject.SetActive(true);

            if (_promptText != null)
                _promptText.text = _defaultText;
        }

        public void Hide()
        {
            if (_promptObject != null)
                _promptObject.SetActive(false);
        }
    }
}
