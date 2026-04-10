using System;

namespace GuildAcademy.Core.UI
{
    /// <summary>
    /// メニュー画面のタブ種別。
    /// </summary>
    public enum MenuTab
    {
        Status = 0,
        Equipment,
        SkillTree,
        Item,
        Party,
        Curriculum,
        Note,
        Encyclopedia,
        Config,
        SaveLoad,
    }

    /// <summary>
    /// メニューのタブ切替ロジック（Pure C#）。
    /// MonoBehaviourに依存しないため EditMode テスト可能。
    /// </summary>
    public class MenuTabController
    {
        public const int TabCount = 10;

        private MenuTab _currentTab;
        public MenuTab CurrentTab => _currentTab;
        public int CurrentIndex => (int)_currentTab;

        /// <summary>
        /// タブが変更されたときに発火するイベント。
        /// </summary>
        public event Action<MenuTab> OnTabChanged;

        public MenuTabController()
        {
            _currentTab = MenuTab.Status;
        }

        /// <summary>
        /// 指定タブに直接切替。
        /// </summary>
        public void SetTab(MenuTab tab)
        {
            if (_currentTab == tab) return;
            _currentTab = tab;
            OnTabChanged?.Invoke(_currentTab);
        }

        /// <summary>
        /// 右方向にタブ移動（ループ）。
        /// </summary>
        public void MoveRight()
        {
            int next = ((int)_currentTab + 1) % TabCount;
            _currentTab = (MenuTab)next;
            OnTabChanged?.Invoke(_currentTab);
        }

        /// <summary>
        /// 左方向にタブ移動（ループ）。
        /// </summary>
        public void MoveLeft()
        {
            int next = ((int)_currentTab - 1 + TabCount) % TabCount;
            _currentTab = (MenuTab)next;
            OnTabChanged?.Invoke(_currentTab);
        }
    }
}
