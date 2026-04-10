using NUnit.Framework;
using GuildAcademy.Core.UI;

namespace GuildAcademy.Tests.EditMode.UI
{
    [TestFixture]
    public class MenuTabControllerTests
    {
        private MenuTabController _controller;

        [SetUp]
        public void SetUp()
        {
            _controller = new MenuTabController();
        }

        // --- 初期状態 ---

        [Test]
        public void InitialTab_IsStatus()
        {
            Assert.AreEqual(MenuTab.Status, _controller.CurrentTab);
            Assert.AreEqual(0, _controller.CurrentIndex);
        }

        // --- MoveRight ---

        [Test]
        public void MoveRight_FromStatus_GoesToEquipment()
        {
            _controller.MoveRight();
            Assert.AreEqual(MenuTab.Equipment, _controller.CurrentTab);
        }

        [Test]
        public void MoveRight_FromSaveLo_ad_WrapsToStatus()
        {
            _controller.SetTab(MenuTab.SaveLoad);
            _controller.MoveRight();
            Assert.AreEqual(MenuTab.Status, _controller.CurrentTab);
        }

        [Test]
        public void MoveRight_FiresOnTabChanged()
        {
            MenuTab? received = null;
            _controller.OnTabChanged += tab => received = tab;

            _controller.MoveRight();

            Assert.IsNotNull(received);
            Assert.AreEqual(MenuTab.Equipment, received.Value);
        }

        // --- MoveLeft ---

        [Test]
        public void MoveLeft_FromStatus_WrapsToSaveLoad()
        {
            _controller.MoveLeft();
            Assert.AreEqual(MenuTab.SaveLoad, _controller.CurrentTab);
        }

        [Test]
        public void MoveLeft_FromEquipment_GoesToStatus()
        {
            _controller.SetTab(MenuTab.Equipment);
            _controller.MoveLeft();
            Assert.AreEqual(MenuTab.Status, _controller.CurrentTab);
        }

        // --- SetTab ---

        [Test]
        public void SetTab_ChangesToSpecifiedTab()
        {
            _controller.SetTab(MenuTab.Config);
            Assert.AreEqual(MenuTab.Config, _controller.CurrentTab);
            Assert.AreEqual(8, _controller.CurrentIndex);
        }

        [Test]
        public void SetTab_SameTab_DoesNotFireEvent()
        {
            _controller.SetTab(MenuTab.Item);

            int fireCount = 0;
            _controller.OnTabChanged += _ => fireCount++;

            _controller.SetTab(MenuTab.Item); // same tab
            Assert.AreEqual(0, fireCount);
        }

        [Test]
        public void SetTab_DifferentTab_FiresEvent()
        {
            int fireCount = 0;
            _controller.OnTabChanged += _ => fireCount++;

            _controller.SetTab(MenuTab.Party);
            Assert.AreEqual(1, fireCount);
        }

        // --- TabCount ---

        [Test]
        public void TabCount_Is10()
        {
            Assert.AreEqual(10, MenuTabController.TabCount);
        }

        // --- 全タブ巡回 ---

        [Test]
        public void MoveRight_10Times_ReturnsToStatus()
        {
            for (int i = 0; i < 10; i++)
            {
                _controller.MoveRight();
            }
            Assert.AreEqual(MenuTab.Status, _controller.CurrentTab);
        }

        [Test]
        public void MoveLeft_10Times_ReturnsToStatus()
        {
            for (int i = 0; i < 10; i++)
            {
                _controller.MoveLeft();
            }
            Assert.AreEqual(MenuTab.Status, _controller.CurrentTab);
        }

        // --- 各タブのインデックス値確認 ---

        [TestCase(MenuTab.Status, 0)]
        [TestCase(MenuTab.Equipment, 1)]
        [TestCase(MenuTab.SkillTree, 2)]
        [TestCase(MenuTab.Item, 3)]
        [TestCase(MenuTab.Party, 4)]
        [TestCase(MenuTab.Curriculum, 5)]
        [TestCase(MenuTab.Note, 6)]
        [TestCase(MenuTab.Encyclopedia, 7)]
        [TestCase(MenuTab.Config, 8)]
        [TestCase(MenuTab.SaveLoad, 9)]
        public void TabIndex_MatchesExpected(MenuTab tab, int expectedIndex)
        {
            _controller.SetTab(tab);
            Assert.AreEqual(expectedIndex, _controller.CurrentIndex);
        }
    }
}
