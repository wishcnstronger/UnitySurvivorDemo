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

        [SerializeField, Tooltip("护甲值，减少受到的伤害百分比")]
        private float armor = 0f;

        [Header("磁铁")]
        [SerializeField, Tooltip("经验宝石吸引范围")]
        private float magnetRange = 5f;

        [Header("经验")]
        [SerializeField, Tooltip("当前经验值")]
        private float currentXP = 0f;

        [SerializeField, Tooltip("升级所需经验值")]
        private float xpToNextLevel = 10f;

        [Header("等级")]
        [SerializeField, Tooltip("当前等级")]
        private int level = 1;

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
            float reduced = amount * (1f - armor / 100f);
            currentHP -= reduced;
            if (currentHP < 0f) currentHP = 0f;
        }

        /// <summary>
        /// 增加经验值。后续阶段会接入升级检测。
        /// </summary>
        public void AddXP(int amount)
        {
            currentXP += amount;
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
