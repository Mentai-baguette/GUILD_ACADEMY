using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace GuildAcademy.Core.Save
{
    public class DefaultSaveSerializer : ISaveSerializer
    {
        public string Serialize(SaveData data)
        {
            return SaveSerializer.Serialize(data);
        }

        public SaveData Deserialize(string json)
        {
            return SaveSerializer.Deserialize(json);
        }
    }

    public static class SaveSerializer
    {
        public static string Serialize(SaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var sb = new StringBuilder();
            sb.Append("{");
            sb.Append($"\"saveId\":\"{Escape(data.SaveId ?? "")}\",");
            sb.Append($"\"timestamp\":\"{Escape(data.Timestamp ?? "")}\",");
            sb.Append($"\"playTimeSeconds\":{data.PlayTimeSeconds},");
            sb.Append($"\"erosionValue\":{data.ErosionValue},");
            sb.Append($"\"currentScene\":\"{Escape(data.CurrentScene ?? "")}\",");
            sb.Append($"\"currentDialogueId\":\"{Escape(data.CurrentDialogueId ?? "")}\",");
            sb.Append($"\"chapterNumber\":{data.ChapterNumber},");
            sb.Append("\"flags\":{");
            sb.Append(SerializeDictValues(data.Flags, v => v ? "true" : "false"));
            sb.Append("},");
            sb.Append("\"trust\":{");
            sb.Append(SerializeDictValues(data.Trust, v => v.ToString()));
            sb.Append("},");
            sb.Append("\"bondPoints\":{");
            sb.Append(SerializeDictValues(data.BondPoints, v => v.ToString()));
            sb.Append("}}");
            return sb.ToString();
        }

        public static SaveData Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
                throw new ArgumentException("JSON string cannot be null or empty", nameof(json));

            return new SaveData
            {
                SaveId = ExtractString(json, "saveId"),
                Timestamp = ExtractString(json, "timestamp"),
                PlayTimeSeconds = ExtractInt(json, "playTimeSeconds"),
                ErosionValue = ExtractInt(json, "erosionValue"),
                CurrentScene = ExtractString(json, "currentScene"),
                CurrentDialogueId = ExtractString(json, "currentDialogueId"),
                ChapterNumber = ExtractInt(json, "chapterNumber"),
                Flags = ExtractBoolDict(json, "flags"),
                Trust = ExtractIntDict(json, "trust"),
                BondPoints = ExtractIntDict(json, "bondPoints")
            };
        }

        private static string ExtractString(string json, string key)
        {
            var match = Regex.Match(json, $"\"{key}\":\"((?:[^\"\\\\]|\\\\.)*)\"");
            return match.Success ? Unescape(match.Groups[1].Value) : "";
        }

        private static int ExtractInt(string json, string key)
        {
            var match = Regex.Match(json, $"\"{key}\":(\\d+)");
            return match.Success ? int.Parse(match.Groups[1].Value) : 0;
        }

        private static Dictionary<string, bool> ExtractBoolDict(string json, string key)
        {
            var dict = new Dictionary<string, bool>();
            var blockMatch = Regex.Match(json, $"\"{key}\":{{([^}}]*)}}");
            if (!blockMatch.Success) return dict;
            var entries = Regex.Matches(blockMatch.Groups[1].Value, "\"([^\"]+)\":(true|false)");
            foreach (Match m in entries)
                dict[m.Groups[1].Value] = m.Groups[2].Value == "true";
            return dict;
        }

        private static Dictionary<string, int> ExtractIntDict(string json, string key)
        {
            var dict = new Dictionary<string, int>();
            var blockMatch = Regex.Match(json, $"\"{key}\":{{([^}}]*)}}");
            if (!blockMatch.Success) return dict;
            var entries = Regex.Matches(blockMatch.Groups[1].Value, "\"([^\"]+)\":(\\d+)");
            foreach (Match m in entries)
                dict[m.Groups[1].Value] = int.Parse(m.Groups[2].Value);
            return dict;
        }

        private static string SerializeDictValues<T>(Dictionary<string, T> dict, Func<T, string> valueSerializer)
        {
            if (dict == null || dict.Count == 0) return "";
            var sb = new StringBuilder();
            bool first = true;
            foreach (var kvp in dict)
            {
                if (!first) sb.Append(",");
                sb.Append($"\"{Escape(kvp.Key)}\":{valueSerializer(kvp.Value)}");
                first = false;
            }
            return sb.ToString();
        }

        private static string Escape(string s)
        {
            return s?.Replace("\\", "\\\\").Replace("\"", "\\\"") ?? "";
        }

        private static string Unescape(string s)
        {
            return s?.Replace("\\\"", "\"").Replace("\\\\", "\\") ?? "";
        }
    }
}
