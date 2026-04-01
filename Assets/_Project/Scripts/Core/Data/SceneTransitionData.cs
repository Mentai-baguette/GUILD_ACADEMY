using System.Collections.Generic;

namespace GuildAcademy.Core.Data
{
    public static class SceneTransitionData
    {
        private static readonly Dictionary<string, object> _data = new Dictionary<string, object>();

        public static void Set<T>(string key, T value)
        {
            _data[key] = value;
        }

        public static T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;
            return defaultValue;
        }

        public static bool Has(string key) => _data.ContainsKey(key);

        public static void Remove(string key) => _data.Remove(key);

        public static void Clear() => _data.Clear();
    }
}
