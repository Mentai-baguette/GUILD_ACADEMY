using System;
using NUnit.Framework;
using GuildAcademy.Core.Save;

namespace GuildAcademy.Tests.EditMode.Save
{
    [TestFixture]
    public class SaveDataTests
    {
        [Test]
        public void NewSaveData_HasDefaultValues()
        {
            var data = new SaveData();

            Assert.IsNull(data.SaveId);
            Assert.IsNull(data.Timestamp);
            Assert.AreEqual(0, data.PlayTimeSeconds);
            Assert.IsNotNull(data.Flags);
            Assert.AreEqual(0, data.Flags.Count);
            Assert.IsNotNull(data.Trust);
            Assert.AreEqual(0, data.Trust.Count);
            Assert.AreEqual(0, data.ErosionValue);
            Assert.IsNotNull(data.BondPoints);
            Assert.AreEqual(0, data.BondPoints.Count);
            Assert.IsNull(data.CurrentScene);
            Assert.IsNull(data.CurrentDialogueId);
            Assert.AreEqual(0, data.ChapterNumber);
        }

        [Test]
        public void SaveData_SetAndGetFlags()
        {
            var data = new SaveData();
            data.Flags["flag_shion_past"] = true;
            data.Flags["flag_carlos_plan"] = false;
            Assert.IsTrue(data.Flags["flag_shion_past"]);
            Assert.IsFalse(data.Flags["flag_carlos_plan"]);
            Assert.AreEqual(2, data.Flags.Count);
        }

        [Test]
        public void SaveData_SetAndGetTrust()
        {
            var data = new SaveData();
            data.Trust["Yuna"] = 75;
            data.Trust["Mio"] = 50;
            Assert.AreEqual(75, data.Trust["Yuna"]);
            Assert.AreEqual(50, data.Trust["Mio"]);
        }

        [Test]
        public void Serialize_ProducesValidJson()
        {
            var data = new SaveData
            {
                SaveId = "save_001",
                Timestamp = "2026-03-31T12:00:00",
                PlayTimeSeconds = 3600,
                ErosionValue = 25,
                CurrentScene = "Chapter1",
                CurrentDialogueId = "dlg_010",
                ChapterNumber = 1
            };
            data.Flags["flag_shion_past"] = true;
            data.Trust["Yuna"] = 80;
            data.BondPoints["Mio"] = 3;

            string json = SaveSerializer.Serialize(data);

            Assert.That(json, Does.Contain("\"saveId\":\"save_001\""));
            Assert.That(json, Does.Contain("\"playTimeSeconds\":3600"));
            Assert.That(json, Does.Contain("\"flag_shion_past\":true"));
            Assert.That(json, Does.Contain("\"Yuna\":80"));
        }

        [Test]
        public void Serialize_NullData_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => SaveSerializer.Serialize(null));
        }

        [Test]
        public void Deserialize_EmptyString_ThrowsException()
        {
            Assert.Throws<ArgumentException>(() => SaveSerializer.Deserialize(""));
            Assert.Throws<ArgumentException>(() => SaveSerializer.Deserialize(null));
        }

        [Test]
        public void RoundTrip_SerializeDeserialize_PreservesAllValues()
        {
            var original = new SaveData
            {
                SaveId = "round_trip_test",
                Timestamp = "2026-03-31T15:30:00",
                PlayTimeSeconds = 7200,
                ErosionValue = 42,
                CurrentScene = "BossRoom",
                CurrentDialogueId = "dlg_final",
                ChapterNumber = 5
            };
            original.Flags["flag_vessel_truth"] = true;
            original.Flags["flag_seal_method"] = false;
            original.Trust["Kaito"] = 60;
            original.Trust["Shion"] = 90;
            original.BondPoints["Yuna"] = 5;

            string json = SaveSerializer.Serialize(original);
            var restored = SaveSerializer.Deserialize(json);

            Assert.AreEqual("round_trip_test", restored.SaveId);
            Assert.AreEqual("2026-03-31T15:30:00", restored.Timestamp);
            Assert.AreEqual(7200, restored.PlayTimeSeconds);
            Assert.AreEqual(42, restored.ErosionValue);
            Assert.AreEqual("BossRoom", restored.CurrentScene);
            Assert.AreEqual("dlg_final", restored.CurrentDialogueId);
            Assert.AreEqual(5, restored.ChapterNumber);
            Assert.IsTrue(restored.Flags["flag_vessel_truth"]);
            Assert.IsFalse(restored.Flags["flag_seal_method"]);
            Assert.AreEqual(60, restored.Trust["Kaito"]);
            Assert.AreEqual(90, restored.Trust["Shion"]);
            Assert.AreEqual(5, restored.BondPoints["Yuna"]);
        }

        [Test]
        public void Deserialize_BasicJson_ReturnsCorrectData()
        {
            var data = new SaveData { SaveId = "test", ChapterNumber = 3 };
            string json = SaveSerializer.Serialize(data);

            var result = SaveSerializer.Deserialize(json);

            Assert.AreEqual("test", result.SaveId);
            Assert.AreEqual(3, result.ChapterNumber);
        }
    }
}
