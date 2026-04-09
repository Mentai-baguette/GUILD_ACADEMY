namespace GuildAcademy.Core.Data
{
    public enum ItemType
    {
        Consumable,
        KeyItem,
        Material,
        Equipment
    }

    public enum ItemEffectType
    {
        None,
        HpRestore,
        MpRestore,
        StatusCure,
        Revive,
        BuffAtk,
        BuffDef,
        FullRestore
    }
}
