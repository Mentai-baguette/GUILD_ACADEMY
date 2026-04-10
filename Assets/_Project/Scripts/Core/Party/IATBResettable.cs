using GuildAcademy.Core.Data;

namespace GuildAcademy.Core.Party
{
    /// <summary>
    /// ATBゲージリセット用インターフェース。
    /// テスト時にモック可能にするため抽象化。
    /// </summary>
    public interface IATBResettable
    {
        void ResetGauge(CharacterStats stats);
    }
}
