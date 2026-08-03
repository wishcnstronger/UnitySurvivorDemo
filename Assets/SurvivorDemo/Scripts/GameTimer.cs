using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 游玩计时器（挂在 Player 上）。
    /// 职责单一：每帧把 Time.deltaTime 累加进 GameStats.playTime。
    /// 用 deltaTime 累加（而不是 timeSinceLevelLoad 差值）：重开不重载场景，
    /// timeSinceLevelLoad 不会归零；且 timeScale=0（开始界面/升级/结算）时
    /// deltaTime≈0，计时天然暂停，结算时间 = HUD 最后显示的时间。
    /// </summary>
    public class GameTimer : MonoBehaviour
    {
        private void Update()
        {
            GameStats.playTime += Time.deltaTime;
        }
    }
}
