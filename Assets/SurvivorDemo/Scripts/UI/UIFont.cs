using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 全局统一字体工具。
    /// 微软雅黑优先（Windows 常见中文字体），静态缓存保证全项目共用同一个实例。
    /// PlayerHUD / GameOverUI / UpgradeUI 都通过它取字体。
    /// </summary>
    public static class UIFont
    {
        /// <summary>缓存的字体实例（只创建一次）</summary>
        private static Font cachedFont;

        /// <summary>
        /// 获取全局字体。
        /// 优先级：项目内 ZPix 像素字体 → 微软雅黑 → Unity 内置 LegacyRuntime → 内置 Arial。
        /// </summary>
        public static Font Get()
        {
            if (cachedFont != null)
                return cachedFont;

            // 1. 项目内 ZPix 像素字体（Resources/Fonts/ZPix）
            cachedFont = Resources.Load<Font>("Fonts/ZPix");

            // 2. 兜底：系统微软雅黑
            if (cachedFont == null)
                cachedFont = Font.CreateDynamicFontFromOSFont("Microsoft YaHei", 16);

            // 3. 兜底：Unity 内置字体（新老版本兼容）
            if (cachedFont == null)
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (cachedFont == null)
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return cachedFont;
        }
    }
}
