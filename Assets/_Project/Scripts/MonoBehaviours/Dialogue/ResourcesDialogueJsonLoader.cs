using System;
using System.Collections.Generic;
using System.IO;
using GuildAcademy.Core.Dialogue;
using UnityEngine;

namespace GuildAcademy.MonoBehaviours.Dialogue
{
    public class ResourcesDialogueJsonLoader : IDialogueSource
    {
        public List<DialogueEntry> LoadEntries(string sourceKey)
        {
            if (string.IsNullOrWhiteSpace(sourceKey))
                throw new ArgumentException("Source key cannot be null or empty.", nameof(sourceKey));

            var asset = Resources.Load<TextAsset>(sourceKey);
            if (asset == null)
                throw new FileNotFoundException($"Dialogue JSON was not found in Resources: {sourceKey}");

            return Parse(asset.text, sourceKey);
        }

        public List<DialogueEntry> Parse(string json, string sourceName = "inline_json")
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new ArgumentException("JSON content is empty.", nameof(json));

            DialogueFileDto dto;
            try
            {
                dto = JsonUtility.FromJson<DialogueFileDto>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException($"Failed to parse dialogue JSON: {sourceName}", ex);
            }

            if (dto?.entries == null || dto.entries.Length == 0)
                throw new InvalidDataException($"Dialogue JSON has no entries: {sourceName}");

            var entries = new List<DialogueEntry>(dto.entries.Length);
            foreach (var entryDto in dto.entries)
            {
                if (string.IsNullOrWhiteSpace(entryDto.id))
                    throw new InvalidDataException($"Dialogue entry id is empty: {sourceName}");

                var entry = new DialogueEntry
                {
                    Id = entryDto.id,
                    Speaker = entryDto.speaker,
                    Text = entryDto.text,
                    Next = entryDto.next,
                    Choices = ConvertChoices(entryDto.choices)
                };

                entries.Add(entry);
            }

            return entries;
        }

        private static List<DialogueChoice> ConvertChoices(DialogueChoiceDto[] choices)
        {
            if (choices == null || choices.Length == 0)
                return null;

            var converted = new List<DialogueChoice>(choices.Length);
            foreach (var choiceDto in choices)
            {
                converted.Add(new DialogueChoice
                {
                    Text = choiceDto.text,
                    Next = choiceDto.next,
                    Flag = choiceDto.flag,
                    TrustEffects = ConvertTrustEffects(choiceDto.trustEffects)
                });
            }

            return converted;
        }

        private static Dictionary<string, int> ConvertTrustEffects(TrustEffectDto[] trustEffects)
        {
            if (trustEffects == null || trustEffects.Length == 0)
                return null;

            var converted = new Dictionary<string, int>();
            foreach (var effect in trustEffects)
            {
                if (string.IsNullOrWhiteSpace(effect.characterId))
                    throw new InvalidDataException("trustEffects.characterId cannot be empty.");

                converted[effect.characterId] = effect.value;
            }

            return converted;
        }

        [Serializable]
        private class DialogueFileDto
        {
            public DialogueEntryDto[] entries;
        }

        [Serializable]
        private class DialogueEntryDto
        {
            public string id;
            public string speaker;
            public string text;
            public string next;
            public DialogueChoiceDto[] choices;
        }

        [Serializable]
        private class DialogueChoiceDto
        {
            public string text;
            public string next;
            public string flag;
            public TrustEffectDto[] trustEffects;
        }

        [Serializable]
        private class TrustEffectDto
        {
            public string characterId;
            public int value;
        }
    }
}
