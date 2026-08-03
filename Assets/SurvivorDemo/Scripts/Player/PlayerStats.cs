using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家运行时属性数据。
    /// Phase1 仅用 moveSpeed，其余为后续阶段预留。
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField, Tooltip("玩家移动速度")]
        private float moveSpeed = 5f;

        [Header("生命")]
        [SerializeField, Tooltip("最大生命值")]
        private float maxHP = 100f;

        [SerializeField, Tooltip("护甲值，固定减伤（实际伤害=伤害-护甲，最低0）")]
        private int armor = 0;

        [Header("磁铁")]
        [SerializeField, Tooltip("经验宝石吸引范围")]
        private float magnetRange = 2f;

        [Header("经验")]
        [SerializeField, Tooltip("当前经验值")]
        private float currentXP = 0f;

        [SerializeField, Tooltip("升级所需经验值（第一级需求，升级后按 xpStep 递增）")]
        private float xpToNextLevel = 10f;

        [SerializeField, Tooltip("每升一级额外增加的经验需求")]
        private float xpStep = 5f;

        [Header("等级")]
        [SerializeField, Tooltip("当前等级")]
        private int level = 1;

        /// <summary>待处理的升级次数（升级时 +1，三选一选择后 -1）</summary>
        public int pendingLevelUps = 0;

        // --- 运行时状态 ---
        private float currentHP;

        public float MoveSpeed => moveSpeed;
        public float MaxHP => maxHP;
        public float CurrentHP => currentHP;
        public float Armor => armor;
        public float MagnetRange => magnetRange;
        public float CurrentXP => currentXP;
        public float XPToNextLevel => xpToNextLevel;
        public int Level => level;

        private void Awake()
        {
            currentHP = maxHP;
        }

        /// <summary>
        /// 受到伤害，考虑护甲减伤。
        /// 后续阶段会加入无敌帧逻辑。
        /// </summary>
        public void TakeDamage(float amount)
        {
            float reduced = Mathf.Max(1f, amount - armor);
            currentHP -= reduced;
            if (currentHP < 0f) currentHP = 0f;
        }

        /// <summary>
        /// 增加经验值。
        /// 经验满后自动升级，用 while 循环处理一次连升多级的情况。
        /// 每升一级 pendingLevelUps +1，由升级流程系统逐次处理。
        /// </summary>
        public void AddXP(int amount)
        {
            currentXP += amount;

            // 经验满就升级，可能一次连升多级
            while (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                level++;
                xpToNextLevel += xpStep; // 升级经验曲线：从 Inspector 初始值起按 xpStep 递增（10, 15, 20...）
                pendingLevelUps++;
            }
        }

        /// <summary>
        /// 消费一次待处理升级。
        /// 三选一选完后由升级流程调用。
        /// </summary>
        /// <returns>是否还有剩余的待处理升级</returns>
        public bool ConsumePendingLevelUp()
        {
            if (pendingLevelUps <= 0)
                return false;

            pendingLevelUps--;
            return pendingLevelUps > 0;
        }

        /// <summary>移动速度乘法强化（由升级流程调用）</summary>
        public void AddMoveSpeedMultiplier(float factor)
        {
            moveSpeed *= factor;
        }

        /// <summary>最大生命值加法强化，同时恢复等量生命（由升级流程调用）</summary>
        public void AddMaxHP(float amount)
        {
            maxHP += amount;
            currentHP += amount;
        }

        /// <summary>护甲加法强化，上限 30（由升级流程调用）</summary>
        public void AddArmor(float amount)
        {
            armor = Mathf.Min(30, armor + (int)amount);
        }

        /// <summary>护甲是否已到上限（升级抽卡时用于排除零收益选项）</summary>
        public bool IsArmorAtCap()
        {
            return armor >= 30;
        }

        /// <summary>经验拾取范围加法强化（由升级流程调用）</summary>
        public void AddMagnetRange(float amount)
        {
            magnetRange += amount;
        }

        /// <summary>
        /// 回满生命。
        /// </summary>
        public void HealToFull()
        {
            currentHP = maxHP;
        }
    }
}
