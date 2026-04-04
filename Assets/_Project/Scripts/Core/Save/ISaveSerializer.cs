namespace GuildAcademy.Core.Save
{
    public interface ISaveSerializer
    {
        string Serialize(SaveData data);
        SaveData Deserialize(string json);
    }
}
