using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家运行时属性数据。
    /// 包含移动/生命/护甲/磁铁/经验/等级/诅咒/吸血等全部属性。
    /// 诅咒值：每点降低 1% 最大生命，增加 5% 敌人生成速度。
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        [Header("移动")]
        [SerializeField, Tooltip("玩家移动速度")]
        private float moveSpeed = 5f;

        [Header("生命")]
        [SerializeField, Tooltip("最大生命值（基础值，不受诅咒影响）")]
        private float maxHP = 100f;

        [SerializeField, Tooltip("护甲值，固定减伤（实际伤害=伤害-护甲，最低0）")]
        private int armor = 0;

        [Header("磁铁")]
        [SerializeField, Tooltip("经验宝石吸引范围")]
        private float magnetRange = 2f;

        [Header("经验")]
        [SerializeField, Tooltip("当前经验值")]
        private float currentXP = 0f;

        [SerializeField, Tooltip("升级所需经验值")]
        private float xpToNextLevel = 10f;

        [SerializeField, Tooltip("每升一级经验需求的增长倍率")]
        private float xpGrowth = 1.15f;

        [Header("等级")]
        [SerializeField, Tooltip("当前等级")]
        private int level = 1;

        [Header("构筑")]
        [SerializeField, Tooltip("经验获取倍率")]
        private float xpRate = 1f;

        [Header("诅咒")]
        [Tooltip("诅咒值（阶段式 Debuff：20+ 生成加速，60+ 敌人加速，80+ 敌人增伤）")]
        public int curseValue = 0;

        [Header("吸血")]
        [Tooltip("吸血率（所有伤害的百分比转化为生命）")]
        public float lifestealRate = 0f;

        /// <summary>是否免疫终焉状态（逆命升级激活）</summary>
        public bool curseImmune = false;

        /// <summary>待处理的升级次数</summary>
        public int pendingLevelUps = 0;

        /// <summary>每种升级类型被选中的次数</summary>
        private int[] pickCounts = new int[UpgradeConfig.TypeCount];

        private float currentHP;

        public float MoveSpeed => moveSpeed;
        /// <summary>实际最大生命（诅咒不再影响 MaxHP，由阶段式 Debuff 替代）</summary>
        public float MaxHP => maxHP;
        public float CurrentHP => currentHP;
        public float Armor => armor;
        public float MagnetRange => magnetRange;
        public float CurrentXP => currentXP;
        public float XPToNextLevel => xpToNextLevel;
        public int Level => level;
        public float XPRate => xpRate;
        /// <summary>诅咒阶段：生成速度加速系数（0=无加速，0.1=+10%等）</summary>
        public float CurseSpawnBoost
        {
            get
            {
                if (curseValue >= 80) return 0.4f;
                if (curseValue >= 60) return 0.3f;
                if (curseValue >= 40) return 0.2f;
                if (curseValue >= 20) return 0.1f;
                return 0f;
            }
        }

        /// <summary>诅咒阶段：敌人移速加速系数（0=无加速，0.1=+10%）</summary>
        public float CurseEnemySpeedBoost
        {
            get
            {
                if (curseValue >= 60) return 0.1f;
                return 0f;
            }
        }

        /// <summary>诅咒阶段：敌人伤害加成系数（0=无加成，0.2=+20%）</summary>
        public float CurseEnemyDamageBoost
        {
            get
            {
                if (curseValue >= 80) return 0.2f;
                return 0f;
            }
        }

        /// <summary>是否进入终焉状态（诅咒≥100）</summary>
        public bool IsCurseFinal => curseValue >= 100;

        private void Awake()
        {
            currentHP = maxHP;
        }

        public void TakeDamage(float amount)
        {
            float reduced = Mathf.Max(1f, amount - armor);
            currentHP -= reduced;
            if (currentHP < 0f) currentHP = 0f;
        }

        public void AddXP(int amount)
        {
            currentXP += amount * xpRate;
            while (currentXP >= xpToNextLevel)
            {
                currentXP -= xpToNextLevel;
                level++;
                xpToNextLevel = Mathf.Ceil(xpToNextLevel * xpGrowth);
                pendingLevelUps++;
            }
        }

        public bool ConsumePendingLevelUp()
        {
            if (pendingLevelUps <= 0) return false;
            pendingLevelUps--;
            return pendingLevelUps > 0;
        }

        // ======== 属性升级方法 ========

        public void AddMoveSpeedMultiplier(float factor) { moveSpeed *= factor; }
        public void AddMaxHP(float amount) { maxHP += amount; currentHP += amount; }
        public void AddArmor(float amount) { armor = Mathf.Min(30, armor + (int)amount); }
        public void AddMagnetRange(float amount) { magnetRange += amount; }
        public void AddXPRate(float amount) { xpRate += amount; }

        public bool IsArmorAtCap() { return armor >= 30; }

        // ======== 诅咒与吸血 ========

        public void AddCurse(int amount) { curseValue += amount; }
        public void AddLifesteal(float rate) { lifestealRate += rate; }

        /// <summary>治疗（不超过实际最大生命）</summary>
        public void Heal(float amount)
        {
            currentHP = Mathf.Min(MaxHP, currentHP + amount);
        }

        public void HealToFull() { currentHP = MaxHP; }

        // ======== 构筑统计 ========

        public void RecordPick(UpgradeConfig.UpgradeType type) { pickCounts[(int)type]++; }
        public int GetPickCount(UpgradeConfig.UpgradeType type) { return pickCounts[(int)type]; }
    }
}
