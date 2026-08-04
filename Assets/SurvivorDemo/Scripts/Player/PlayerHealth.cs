using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家生命组件（挂在 Player 上）。
    /// 职责单一：只处理"受伤"——接触检测、无敌帧、闪烁、调用 PlayerStats.TakeDamage。
    /// 用组件判断（EnemyMovement）识别敌人，不用 Tag（同 Bullet 的做法）。
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        /// <summary>受伤后的无敌时长（秒），期间不再受理伤害</summary>
        public float invincibleDuration = 0.5f;

        /// <summary>无敌期间角色闪烁间隔（秒），每隔这么久切换一次渲染开关</summary>
        public float blinkInterval = 0.1f;

        /// <summary>玩家属性（扣血公式在 PlayerStats.TakeDamage 里，含护甲减伤）</summary>
        private PlayerStats stats;

        /// <summary>玩家渲染器（无敌期间闪烁用）</summary>
        private SpriteRenderer spriteRenderer;

        /// <summary>是否已死亡（死亡后不再受理伤害，结算由 GameOverUI 接管）</summary>
        private bool isDead;

        /// <summary>无敌剩余时间（&gt;0 表示处于无敌状态）</summary>
        private float invincibleTimer;

        /// <summary>闪烁计时器（累计到 blinkInterval 切换一次渲染开关）</summary>
        private float blinkTimer;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            // 死亡后停止无敌与闪烁逻辑
            if (isDead)
                return;

            // 无敌计时倒数
            if (invincibleTimer > 0f)
            {
                invincibleTimer -= Time.deltaTime;

                // 闪烁：每隔 blinkInterval 切换一次 SpriteRenderer.enabled
                blinkTimer += Time.deltaTime;
                if (blinkTimer >= blinkInterval)
                {
                    blinkTimer = 0f;
                    if (spriteRenderer != null)
                        spriteRenderer.enabled = !spriteRenderer.enabled;
                }

                // 无敌结束 → 恢复显示，避免卡在隐藏状态
                if (invincibleTimer <= 0f && spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                }
            }
        }

        /// <summary>
        /// 公共受伤入口（敌人接触伤害和敌人子弹共用同一条链路）。
        /// 判定顺序：死亡 / 无敌 → 直接 return（必须在扣血之前，否则连续掉血）。
        /// 然后调 PlayerStats.TakeDamage（复用护甲减伤公式，不重写扣血逻辑），
        /// 血量归零则标记死亡，否则启动无敌帧 + 闪烁。
        /// 敌人子弹也必须走这里，才能吃无敌帧，防止弹幕秒杀。
        /// </summary>
        public void TakeDamage(float amount)
        {
            // 已死亡 / 处于无敌状态 → 不受理伤害（顺序必须在扣血之前）
            if (isDead || invincibleTimer > 0f)
                return;

            // 属性缺失直接返回
            if (stats == null)
                return;

            // 受伤：调用 PlayerStats.TakeDamage（复用护甲减伤公式，不重写扣血逻辑）
            stats.TakeDamage(amount);

            // 血量归零 → 标记死亡（之后 GameOverUI 每帧轮询到血量 0 会弹结算）
            if (stats.CurrentHP <= 0f)
            {
                isDead = true;
                return;
            }

            // 未死亡：启动无敌计时，开始闪烁
            invincibleTimer = invincibleDuration;
            blinkTimer = 0f;
        }

        /// <summary>
        /// 碰到物体时触发。
        /// 注意：OnTriggerStay2D 每帧触发（不是只触发一次），
        /// 这里只处理"碰到敌人"这一种伤害来源，死亡/无敌判定统一在 TakeDamage 内部。
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            // 只有碰到敌人（带 EnemyMovement 组件）才触发接触伤害
            EnemyMovement enemy = other.GetComponent<EnemyMovement>();
            if (enemy == null)
                return;

            // 冲锋期间只结算冲锋伤害，不结算碰撞伤害
            ChargeAttacker charger = other.GetComponent<ChargeAttacker>();
            if (charger != null && charger.IsCharging)
            {
                TakeDamage(charger.chargeDamage);
                return;
            }

            // 非冲锋：正常接触伤害（伤害值取自敌人）
            TakeDamage(enemy.contactDamage);
        }
    }
}
