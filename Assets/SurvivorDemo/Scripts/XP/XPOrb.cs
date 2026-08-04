using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 经验宝石组件。
    /// 敌人死亡时掉落；玩家进入磁铁范围后自动被吸向玩家，碰到玩家被拾取加经验。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class XPOrb : MonoBehaviour
    {
        /// <summary>经验值（敌人掉落时设置）</summary>
        public int xpValue = 5;

        /// <summary>磁铁范围内的基础飞行速度</summary>
        public float baseSpeed = 4f;

        /// <summary>速度递增系数：越靠近玩家速度越快</summary>
        public float speedBoost = 6f;

        /// <summary>玩家 Transform</summary>
        private Transform player;

        /// <summary>玩家属性（拾取时调用 AddXP）</summary>
        private PlayerStats playerStats;

        private void Awake()
        {
            // 刚体设为 Kinematic：不受重力影响，不被物理推动
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 碰撞体设为触发器：只用来检测拾取
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            // 通过 Tag 找到玩家
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerStats = playerObj.GetComponent<PlayerStats>();
            }
        }

        private void Update()
        {
            // 玩家不存在就不动
            if (player == null)
                return;

            // 玩家属性不存在就不动
            if (playerStats == null)
                return;

            // 计算与玩家的距离
            float distance = Vector2.Distance(transform.position, player.position);

            // 在磁铁范围外 → 宝石静止不动
            if (distance > playerStats.MagnetRange)
                return;

            // 进入磁铁范围：朝玩家移动，越近越快
            float speed = baseSpeed + speedBoost * (1f - distance / playerStats.MagnetRange);
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }

        /// <summary>
        /// 碰到玩家时触发拾取：加经验 → 销毁自身。
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 只有碰到玩家才拾取
            if (!other.CompareTag("Player"))
                return;

            // 给玩家加经验
            PlayerStats stats = other.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddXP(xpValue);
            }

            // 销毁宝石
            Destroy(gameObject);
        }

        /// <summary>
        /// 设置经验值并根据数值调整宝石外观（大小 + 颜色）。
        /// 由 EnemyHealth.Die() 在掉落时调用。
        /// </summary>
        public void SetXP(int value)
        {
            xpValue = value;

            float scale;
            Color color;

            










































            if (value <= 20)
            {
                // 小宝石：绿色（圆形怪 15 XP）
                scale = 0.5f;
                color = Color.green;
            }
            else if (value <= 35)
            {
                // 中宝石：青色（三角怪 25 XP）
                scale = 0.7f;
                color = Color.cyan;
            }
            else if (value <= 100)
            {
                // 大宝石：紫色（方块怪 50 XP）
                scale = 1.0f;
                color = new Color(0.69f, 0.3f, 1f);
            }
            else
            {
                // 首领宝石：金色（首领 200 XP）
                scale = 1.5f;
                color = new Color(1f, 0.84f, 0f);
            }

            transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                sr.color = color;
        }
    }
}
