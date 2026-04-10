using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using GuildAcademy.Core.Calendar;

namespace GuildAcademy.Core.Events
{
    /// <summary>
    /// JSONからイベントデータを読み込むローダー。
    /// Core アセンブリは noEngineReferences=true のため、
    /// UnityEngine.JsonUtility を使わず軽量パーサで実装する。
    /// </summary>
    public static class EventDataLoader
    {
        /// <summary>
        /// JSON文字列からイベントデータリストを読み込む。
        /// </summary>
        public static List<EventData> LoadFromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return new List<EventData>();

            var result = new List<EventData>();

            // "events" 配列内の各オブジェクトを抽出
            var eventsArrayMatch = Regex.Match(json, @"""events""\s*:\s*\[(.+)\]", RegexOptions.Singleline);
            if (!eventsArrayMatch.Success)
                return result;

            var arrayContent = eventsArrayMatch.Groups[1].Value;

            // 各 { ... } ブロックを抽出
            int depth = 0;
            int start = -1;
            for (int i = 0; i < arrayContent.Length; i++)
            {
                if (arrayContent[i] == '{')
                {
                    if (depth == 0) start = i;
                    depth++;
                }
                else if (arrayContent[i] == '}')
                {
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        var objStr = arrayContent.Substring(start, i - start + 1);
                        var eventData = ParseEventObject(objStr);
                        if (!string.IsNullOrEmpty(eventData.EventId))
                            result.Add(eventData);
                        start = -1;
                    }
                }
            }

            return result;
        }

        private static EventData ParseEventObject(string obj)
        {
            return new EventData
            {
                EventId = GetStringValue(obj, "eventId"),
                Type = ParseEventType(GetStringValue(obj, "type")),
                Chapter = GetIntValue(obj, "chapter"),
                Week = GetIntValue(obj, "week"),
                Day = ParseDay(GetStringValue(obj, "day")),
                TimeSlot = ParseTimeOfDay(GetStringValue(obj, "timeSlot")),
                PrerequisiteFlags = GetStringArray(obj, "prerequisiteFlags"),
                TargetCharacterId = GetStringValue(obj, "targetCharacterId"),
                RequiredTrust = GetIntValue(obj, "requiredTrust"),
                DialogueKey = GetStringValue(obj, "dialogueKey"),
                Location = GetStringValue(obj, "location"),
                IsRepeatable = GetBoolValue(obj, "isRepeatable")
            };
        }

        private static string GetStringValue(string obj, string key)
        {
            var match = Regex.Match(obj, @"""" + key + @"""\s*:\s*""([^""]*)""");
            return match.Success ? match.Groups[1].Value : "";
        }

        private static int GetIntValue(string obj, string key)
        {
            var match = Regex.Match(obj, @"""" + key + @"""\s*:\s*(\d+)");
            return match.Success && int.TryParse(match.Groups[1].Value, out var val) ? val : 0;
        }

        private static bool GetBoolValue(string obj, string key)
        {
            var match = Regex.Match(obj, @"""" + key + @"""\s*:\s*(true|false)");
            return match.Success && match.Groups[1].Value == "true";
        }

        private static string[] GetStringArray(string obj, string key)
        {
            var match = Regex.Match(obj, @"""" + key + @"""\s*:\s*\[([^\]]*)\]");
            if (!match.Success)
                return Array.Empty<string>();

            var content = match.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(content))
                return Array.Empty<string>();

            var items = new List<string>();
            var itemMatches = Regex.Matches(content, @"""([^""]*)""");
            foreach (Match m in itemMatches)
                items.Add(m.Groups[1].Value);

            return items.ToArray();
        }

        private static EventType ParseEventType(string value)
        {
            if (string.IsNullOrEmpty(value)) return EventType.Free;
            if (Enum.TryParse<EventType>(value, true, out var result))
                return result;
            return EventType.Free;
        }

        private static CalendarDay? ParseDay(string value)
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (Enum.TryParse<CalendarDay>(value, true, out var result))
                return result;
            return null;
        }

        private static TimeOfDay ParseTimeOfDay(string value)
        {
            if (string.IsNullOrEmpty(value)) return TimeOfDay.Morning;
            if (Enum.TryParse<TimeOfDay>(value, true, out var result))
                return result;
            return TimeOfDay.Morning;
        }
    }
}
