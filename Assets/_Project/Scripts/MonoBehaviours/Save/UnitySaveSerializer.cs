using UnityEngine;
using GuildAcademy.Core.Save;

namespace GuildAcademy.MonoBehaviours.Save
{
    /// <summary>
    /// Unity JsonUtility を使った ISaveSerializer 実装。
    /// Core層の DefaultSaveSerializer より堅牢なJSON処理が可能。
    /// 将来的にはこちらをメインで使用する。
    /// </summary>
    public class UnitySaveSerializer : ISaveSerializer
    {
        public string Serialize(SaveData data)
        {
            var wrapper = SaveDataWrapper.FromSaveData(data);
            return JsonUtility.ToJson(wrapper, true);
        }

        public SaveData Deserialize(string json)
        {
            var wrapper = JsonUtility.FromJson<SaveDataWrapper>(json);
            return wrapper.ToSaveData();
        }
    }

    /// <summary>
    /// JsonUtility は Dictionary をシリアライズできないため、
    /// List ベースのラッパーで変換する。
    /// </summary>
    [System.Serializable]
    internal class SaveDataWrapper
    {
        public string saveId;
        public string timestamp;
        public int playTimeSeconds;
        public int erosionValue;
        public string currentScene;
        public string currentDialogueId;
        public int chapterNumber;
        public SerializableEntry<bool>[] flags;
        public SerializableEntry<int>[] trust;
        public SerializableEntry<int>[] bondPoints;

        public static SaveDataWrapper FromSaveData(SaveData data)
        {
            return new SaveDataWrapper
            {
                saveId = data.SaveId ?? "",
                timestamp = data.Timestamp ?? "",
                playTimeSeconds = data.PlayTimeSeconds,
                erosionValue = data.ErosionValue,
                currentScene = data.CurrentScene ?? "",
                currentDialogueId = data.CurrentDialogueId ?? "",
                chapterNumber = data.ChapterNumber,
                flags = ToEntries(data.Flags),
                trust = ToEntries(data.Trust),
                bondPoints = ToEntries(data.BondPoints)
            };
        }

        public SaveData ToSaveData()
        {
            var data = new SaveData
            {
                SaveId = saveId,
                Timestamp = timestamp,
                PlayTimeSeconds = playTimeSeconds,
                ErosionValue = erosionValue,
                CurrentScene = currentScene,
                CurrentDialogueId = currentDialogueId,
                ChapterNumber = chapterNumber
            };
            if (flags != null)
                foreach (var e in flags) data.Flags[e.key] = e.value;
            if (trust != null)
                foreach (var e in trust) data.Trust[e.key] = e.value;
            if (bondPoints != null)
                foreach (var e in bondPoints) data.BondPoints[e.key] = e.value;
            return data;
        }

        private static SerializableEntry<T>[] ToEntries<T>(System.Collections.Generic.Dictionary<string, T> dict)
        {
            if (dict == null || dict.Count == 0)
                return System.Array.Empty<SerializableEntry<T>>();
            var entries = new SerializableEntry<T>[dict.Count];
            int i = 0;
            foreach (var kvp in dict)
                entries[i++] = new SerializableEntry<T> { key = kvp.Key, value = kvp.Value };
            return entries;
        }
    }

    [System.Serializable]
    internal class SerializableEntry<T>
    {
        public string key;
        public T value;
    }
}
