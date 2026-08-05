using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 顿帧控制器（挂在摄像机上）。
    /// 击中时短暂降低 Time.timeScale 制造打击感，结束后恢复。
    /// 使用 Time.unscaledDeltaTime 计时，不受自身降速影响。
    /// </summary>
    public class HitStopController : MonoBehaviour
    {
        /// <summary>顿帧时的 timeScale</summary>
        private const float StopTimeScale = 0.05f;

        /// <summary>顿帧剩余时间</summary>
        private float stopTimer;

        /// <summary>是否正在顿帧</summary>
        private bool isStopped;

        /// <summary>
        /// 触发顿帧。如果已经在顿帧中取较长时长。
        /// 游戏暂停时（timeScale<=0）不激活。
        /// </summary>
        public void Stop(float duration)
        {
            if (Time.timeScale <= 0f)
                return;

            if (isStopped)
            {
                stopTimer = Mathf.Max(stopTimer, duration);
                return;
            }

            isStopped = true;
            stopTimer = duration;
            Time.timeScale = StopTimeScale;
        }

        private void Update()
        {
            if (!isStopped)
                return;

            stopTimer -= Time.unscaledDeltaTime;
            if (stopTimer <= 0f)
            {
                // 仅在非暂停状态下恢复 timeScale。
                // 若游戏结束/暂停时被外部设为 0，不覆盖。
                if (Time.timeScale > 0f)
                    Time.timeScale = 1f;
                isStopped = false;
                stopTimer = 0f;
            }
        }

        private void OnDisable()
        {
            if (isStopped)
            {
                Time.timeScale = 1f;
                isStopped = false;
            }
        }
    }
}
