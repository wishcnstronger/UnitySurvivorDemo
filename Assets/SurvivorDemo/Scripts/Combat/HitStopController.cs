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

        /// <summary>最小触发间隔（秒）：防止高频命中连续顿帧，把游戏长时间钉在慢动作</summary>
        public float minTriggerInterval = 0.15f;

        /// <summary>距上次允许触发顿帧的冷却时间（用 unscaledDeltaTime 倒数，不受 timeScale 影响）</summary>
        private float triggerCooldown;

        /// <summary>
        /// 触发顿帧。如果已经在顿帧中取较长时长。
        /// 游戏暂停时（timeScale<=0）不激活；冷却期内（距上次触发不足 minTriggerInterval）直接忽略。
        /// </summary>
        public void Stop(float duration)
        {
            if (Time.timeScale <= 0f)
                return;

            // 冷却未到 → 直接返回：高命中率下不再每次命中都延长顿帧，
            // 否则 timeScale 会被持续钉在 StopTimeScale，一半真实时间都在慢动作
            if (triggerCooldown > 0f)
                return;

            // 开始冷却（真实时间，不受顿帧降速影响）
            triggerCooldown = minTriggerInterval;

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
            // 冷却计时：真实时间累减，不受顿帧降速影响
            if (triggerCooldown > 0f)
                triggerCooldown -= Time.unscaledDeltaTime;

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
            // 只在当前 timeScale 确实是顿帧值时才恢复为 1。
            // 若顿帧进行中组件被禁用/销毁，而游戏又处于升级/结算暂停（timeScale=0），
            // 直接改成 1 会顶掉暂停；仅当等于顿帧值才恢复，避免覆盖外部暂停。
            if (isStopped && Time.timeScale == StopTimeScale)
            {
                Time.timeScale = 1f;
            }
            isStopped = false;
            stopTimer = 0f;
        }
    }
}
