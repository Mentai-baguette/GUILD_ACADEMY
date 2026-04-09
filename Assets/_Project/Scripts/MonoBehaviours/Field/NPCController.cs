using UnityEngine;
using GuildAcademy.UI;

namespace GuildAcademy.MonoBehaviours.Field
{
    public class NPCController : MonoBehaviour
    {
        [Header("NPC Identity")]
        [SerializeField] private string _npcName;

        [Header("Dialogue")]
        [SerializeField] private string _dialogueSourceKey = "Dialogues/chapter1_dialogue";
        [SerializeField] private string _dialogueEntryId;

        [Header("Chapter Dialogue Override")]
        [SerializeField] private ChapterDialogue[] _chapterDialogues;

        [Header("Interaction")]
        [SerializeField] private KeyCode _interactKey = KeyCode.E;
        [SerializeField] private NPCInteractionPrompt _prompt;

        [Header("Optional")]
        [SerializeField] private InfoFlagTrigger _infoFlagTrigger;

        private bool _playerInRange;
        private bool _inConversation;

        [System.Serializable]
        public class ChapterDialogue
        {
            public int chapter;
            public string entryId;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = true;
            if (_prompt != null) _prompt.Show();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _playerInRange = false;
            if (_prompt != null) _prompt.Hide();
        }

        private void Update()
        {
            if (!_playerInRange || _inConversation) return;

            if (Input.GetKeyDown(_interactKey))
            {
                StartConversation();
            }
        }

        private void StartConversation()
        {
            var bridge = DialogueUIBridge.Instance;
            if (bridge == null)
            {
                Debug.LogWarning($"[NPCController] DialogueUIBridge not found. NPC: {_npcName}");
                return;
            }

            if (bridge.IsActive) return;

            _inConversation = true;
            if (_prompt != null) _prompt.Hide();

            string entryId = GetCurrentEntryId();
            if (string.IsNullOrEmpty(entryId))
            {
                Debug.LogWarning($"[NPCController] No dialogue entry configured for NPC: {_npcName}");
                _inConversation = false;
                return;
            }

            bridge.StartDialogue(_dialogueSourceKey, entryId);

            // Subscribe to dialogue end to clean up
            void OnEnd()
            {
                _inConversation = false;
                if (_infoFlagTrigger != null)
                    _infoFlagTrigger.TryComplete();
            }

            // We need a one-shot subscription. Use the bridge's manager end event indirectly.
            StartCoroutine(WaitForDialogueEnd(bridge, OnEnd));
        }

        private string GetCurrentEntryId()
        {
            if (_chapterDialogues != null && _chapterDialogues.Length > 0)
            {
                int currentChapter = GetCurrentChapter();
                for (int i = _chapterDialogues.Length - 1; i >= 0; i--)
                {
                    if (_chapterDialogues[i].chapter <= currentChapter)
                        return _chapterDialogues[i].entryId;
                }
            }
            return _dialogueEntryId;
        }

        private int GetCurrentChapter()
        {
            // TODO: Get from CalendarManager when ScheduleManagerMB is implemented
            return 1;
        }

        private System.Collections.IEnumerator WaitForDialogueEnd(DialogueUIBridge bridge, System.Action onEnd)
        {
            yield return null; // Wait one frame to ensure dialogue is started
            while (bridge.IsActive)
                yield return null;
            onEnd?.Invoke();
        }
    }
}
