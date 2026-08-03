namespace SurvivorDemo
{
    /// <summary>
    /// 全局游戏统计（静态类）。
    /// 注意：static 变量在游戏重开（ResetGame）时不会自动清空，必须显式归零。
    /// </summary>
    public static class GameStats
    {
        /// <summary>总击杀数（敌人死亡时 +1，GameSetup.ResetGame 时归零）</summary>
        public static int kills = 0;

        /// <summary>
        /// 本局游玩秒数（GameTimer 每帧累加 deltaTime）。
        /// 用 deltaTime 累加而不是 timeSinceLevelLoad 差值：重开不重载场景，
        /// timeSinceLevelLoad 不会归零，差值法会把上一局的时间也算进来；
        /// 且 timeScale=0（开始界面/升级/结算）时 deltaTime≈0，计时天然暂停。
        /// ResetGame 时归零。
        /// </summary>
        public static float playTime = 0f;
    }
}
