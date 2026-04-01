using System;
using System.Text;

namespace GuildAcademy.Core.Save
{
    public static class SaveSerializer
    {
        // Simple JSON serialization without external dependencies
        // Using manual approach since Core has noEngineReferences: true
        // and System.Text.Json may not be available in Unity's .NET

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
            sb.Append(SerializeDict(data.Flags, v => v ? "true" : "false"));
            sb.Append("},");
            sb.Append("\"trust\":{");
            sb.Append(SerializeDict(data.Trust, v => v.ToString()));
            sb.Append("},");
            sb.Append("\"bondPoints\":{");
            sb.Append(SerializeDict(data.BondPoints, v => v.ToString()));
            sb.Append("}}");
            return sb.ToString();
        }

        // For deserialization, use a simple approach
        // In production, consider using JsonUtility in the MonoBehaviour layer
        // This is a minimal Pure C# implementation

        private static string SerializeDict<T>(System.Collections.Generic.Dictionary<string, T> dict, Func<T, string> valueSerializer)
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
    }
}
