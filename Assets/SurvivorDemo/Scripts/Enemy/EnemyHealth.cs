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

        /// <summary>掉落经验值（Inspector 可调）</summary>
        public int xpDropAmount = 5;

        /// <summary>经验宝石预制体（Inspector 拖入）</summary>
        public GameObject xpOrbPrefab;

        /// <summary>当前生命值</summary>
        private float currentHP;

        /// <summary>是否已死亡（防止同帧被多颗子弹命中时重复掉落经验宝石）</summary>
        private bool isDead;

        /// <summary>血条引用（同一 GameObject 上的 EnemyHealthBar）</summary>
        private EnemyHealthBar healthBar;

        private void Awake()
        {
            currentHP = maxHP;
            healthBar = GetComponent<EnemyHealthBar>();
        }

        /// <summary>
        /// 生成后由 EnemySpawner 调用，按时间倍率缩放血量。
        /// Awake 已把 currentHP 设为原始 maxHP，这里重设 maxHP 并同步 currentHP。
        /// </summary>
        public void ScaleMaxHP(float multiplier)
        {
            maxHP *= multiplier;
            currentHP = maxHP;
        }

        /// <summary>
        /// 受到伤害。
        /// 扣血后更新血条，生命值小于等于 0 时调用 Die() 死亡。
        /// </summary>
        /// <param name="amount">伤害数值</param>
        public void ReceiveDamage(float amount)
        {
            // 已死亡就不再受理伤害，防止同帧多颗子弹命中导致重复掉落
            if (isDead)
                return;

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
        /// 在原地掉落经验宝石，然后从场景中销毁。
        /// </summary>
        private void Die()
        {
            // 先置死亡标记，再执行掉落与销毁
            isDead = true;
            GameStats.kills++; // 计入全局击杀数（结算界面用，重开时归零）
            Debug.Log($"{gameObject.name} 死亡");

            // 掉落经验宝石，并把经验值设置到宝石上
            if (xpOrbPrefab != null)
            {
                GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
                


























































                XPOrb xpOrb = orb.GetComponent<XPOrb>();
                if (xpOrb != null)
                {
                    xpOrb.SetXP(xpDropAmount);
                }
            }

            Destroy(gameObject);
        }
    }
}
