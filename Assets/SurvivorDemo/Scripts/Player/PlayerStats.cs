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

        [SerializeField, Tooltip("升级所需经验值（第一级需求，升级后按 xpGrowth 倍率增长）")]
        private float xpToNextLevel = 10f;

        [SerializeField, Tooltip("每升一级经验需求的增长倍率（乘法曲线：10 → 12 → 13 → 15...）")]
        private float xpGrowth = 1.15f;

        [Header("等级")]
        [SerializeField, Tooltip("当前等级")]
        private int level = 1;

        [Header("构筑")]
        [SerializeField, Tooltip("经验获取倍率（经验加成卡提升，1 = 无加成）")]
        private float xpRate = 1f;

        /// <summary>待处理的升级次数（升级时 +1，三选一选择后 -1）</summary>
        public int pendingLevelUps = 0;

        /// <summary>每种升级类型被选中的次数（构筑倾向，实例字段：重开新玩家自动清零，无需 static）</summary>
        private int[] pickCounts = new int[UpgradeConfig.TypeCount];

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
        public float XPRate => xpRate;

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
            // 经验加成倍率只作用于实际获得值，宝石外观/拾取逻辑不动
            currentXP += amount * xpRate;

            // 经验满就升级，可能一次连升多级
            while (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                level++;
                xpToNextLevel = Mathf.Ceil(xpToNextLevel * xpGrowth); // 乘法曲线：10 → 12 → 13 → 15...
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

        /// <summary>记录一次升级选择（构筑倾向统计，由升级流程在应用强化后调用）</summary>
        public void RecordPick(UpgradeConfig.UpgradeType type)
        {
            pickCounts[(int)type]++;
        }

        /// <summary>查询某类型已被选择的次数（抽卡加权用：选过的流派更容易再出）</summary>
        public int GetPickCount(UpgradeConfig.UpgradeType type)
        {
            return pickCounts[(int)type];
        }

        /// <summary>经验获取倍率加法强化（经验加成卡调用）</summary>
        public void AddXPRate(float amount)
        {
            xpRate += amount;
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
