using System.Collections.Generic;

namespace GuildAcademy.Core.Dialogue
{
    public interface IDialogueSource
    {
        List<DialogueEntry> LoadEntries(string sourceKey);
    }
}
