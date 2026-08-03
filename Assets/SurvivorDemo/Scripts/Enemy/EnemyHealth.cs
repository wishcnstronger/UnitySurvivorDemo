using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人血量组件。
    /// 职责单一：只管血量与死亡，不涉及移动和生成。
    /// 受伤时通知 EnemyHealthBar 更新显示。
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        /// <summary>最大生命值（Inspector 可调）</summary>
        public float maxHP = 3f;

        /// <summary>当前生命值</summary>
        private float currentHP;

        /// <summary>血条引用（同一 GameObject 上的 EnemyHealthBar）</summary>
        private EnemyHealthBar healthBar;

        private void Awake()
        {
            currentHP = maxHP;
            healthBar = GetComponent<EnemyHealthBar>();
        }

        /// <summary>
        /// 受到伤害。
        /// 扣血后更新血条，生命值小于等于 0 时调用 Die() 死亡。
        /// </summary>
        /// <param name="amount">伤害数值</param>
        public void ReceiveDamage(float amount)
        {
            currentHP -= amount;

            // 更新血条显示
            if (healthBar != null)
            {
                healthBar.UpdateBar(currentHP, maxHP);
            }

            // 血量归零 → 死亡
            if (currentHP <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// 敌人死亡。
        /// 从场景中销毁，并打印日志方便运行时观察。
        /// </summary>
        private void Die()
        {
            Debug.Log($"{gameObject.name} 死亡");
            Destroy(gameObject);
        }
    }
}
