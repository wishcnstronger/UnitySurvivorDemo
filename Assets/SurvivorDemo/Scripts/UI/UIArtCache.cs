using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// AI 生成美术资源缓存。
    /// 从 Resources/UI/ 加载像素风 Sprite，未找到时返回 null（调用方用程序化兜底）。
    /// </summary>
    public static class UIArtCache
    {
        /// <summary>面板背景图类型</summary>
        public enum PanelType { Upgrade, StartScreen, GameOver }

        // ======== 升级图标 ========
        private static Sprite[] _upgradeIcons;
        private static bool _iconsLoaded;

        /// <summary>按 UpgradeType 获取升级图标，未加载返回 null</summary>
        public static Sprite GetUpgradeIcon(UpgradeConfig.UpgradeType type)
        {
            if (!_iconsLoaded)
            {
                _iconsLoaded = true;
                _upgradeIcons = new Sprite[UpgradeConfig.TypeCount];
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.FireRate]     = LoadSprite("UI/FireRateIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.BulletCount]  = LoadSprite("UI/BulletCountIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.Penetration]  = LoadSprite("UI/PenetrationIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.Damage]       = LoadSprite("UI/DamageIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.MoveSpeed]    = LoadSprite("UI/MoveSpeedIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.MaxHP]        = LoadSprite("UI/MaxHPIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.Armor]        = LoadSprite("UI/ArmorIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.MagnetRange]  = LoadSprite("UI/MagnetRangeIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.Range]        = LoadSprite("UI/RangeIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.XPBoost]      = LoadSprite("UI/XPBoostIcon");
                _upgradeIcons[(int)UpgradeConfig.UpgradeType.Crit]        = LoadSprite("UI/CritIcon");
            }
            int idx = (int)type;
            if (idx >= 0 && idx < _upgradeIcons.Length)
                return _upgradeIcons[idx];
            return null;
        }

        // ======== 面板背景图 ========
        private static Sprite _upgradePanelBg;
        private static Sprite _startScreenPanelBg;
        private static Sprite _gameOverPanelBg;
        private static Sprite _buttonBg;
        private static bool _panelLoaded;

        private static void EnsurePanels()
        {
            if (_panelLoaded) return;
            _panelLoaded = true;
            _upgradePanelBg     = LoadSprite("UI/UpgradePanelBg");
            _startScreenPanelBg = LoadSprite("UI/StartScreenPanelBg");
            _gameOverPanelBg    = LoadSprite("UI/GameOverPanelBg");
            _buttonBg           = LoadSprite("UI/ButtonBg");
        }

        /// <summary>按类型获取面板背景图</summary>
        public static Sprite GetPanelBg(PanelType type)
        {
            EnsurePanels();
            switch (type)
            {
                case PanelType.Upgrade:     return _upgradePanelBg;
                case PanelType.StartScreen:  return _startScreenPanelBg;
                case PanelType.GameOver:     return _gameOverPanelBg;
                default: return null;
            }
        }

        /// <summary>通用按钮背景图</summary>
        public static Sprite ButtonBg
        {
            get { EnsurePanels(); return _buttonBg; }
        }

        // ======== HUD 图标 ========
        private static Sprite _skullIcon;
        private static Sprite _heartIcon;
        private static Sprite _hourglassIcon;
        private static bool _hudLoaded;

        private static void EnsureHudIcons()
        {
            if (_hudLoaded) return;
            _hudLoaded = true;
            _skullIcon      = LoadSprite("UI/SkullIcon");
            _heartIcon      = LoadSprite("UI/HeartIcon");
            _hourglassIcon  = LoadSprite("UI/HourglassIcon");
        }

        public static Sprite SkullIcon      { get { EnsureHudIcons(); return _skullIcon; } }
        public static Sprite HeartIcon      { get { EnsureHudIcons(); return _heartIcon; } }
        public static Sprite HourglassIcon  { get { EnsureHudIcons(); return _hourglassIcon; } }

        // ======== 工具 ========
        private static Sprite LoadSprite(string path)
        {
            return Resources.Load<Sprite>(path);
        }
    }
}
