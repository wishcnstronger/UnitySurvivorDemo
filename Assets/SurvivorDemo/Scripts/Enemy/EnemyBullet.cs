using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人子弹（投掷物）。
    /// 沿固定方向匀速直线飞行，命中玩家造成伤害后销毁，超时自动销毁。
    /// 伤害必须走 PlayerHealth.TakeDamage（吃无敌帧），禁止直接改玩家血量，
    /// 否则弹幕可绕过无敌帧连续秒杀。
    /// RequireComponent 声明依赖组件，预制体创建时 Unity 会自动补齐。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class EnemyBullet : MonoBehaviour
    {
        /// <summary>超时销毁时间（秒）</summary>
        public float lifetime = 5f;

        /// <summary>飞行方向（Setup 传入，单位向量）</summary>
        private Vector2 dir;

        /// <summary>飞行速度（单位/秒）</summary>
        private float speed;

        /// <summary>命中伤害（由 TriShooter 传入）</summary>
        private float damage;

        /// <summary>是否已销毁（Destroy 延迟到帧末，用标记短路同帧后续的碰撞回调）</summary>
        private bool destroyed;

        /// <summary>剩余存活时间（倒数到 0 销毁）</summary>
        private float lifeTimer;

        private void Awake()
        {
            // 刚体设为 Kinematic：不受重力影响，不被物理力量推动
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 碰撞体设为触发器：只用来检测命中，不产生物理反弹
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
        }

        /// <summary>
        /// 初始化子弹（发射后只调用一次）。
        /// </summary>
        /// <param name="direction">飞行方向（单位向量）</param>
        /// <param name="spd">飞行速度</param>
        /// <param name="dmg">命中伤害</param>
        public void Setup(Vector2 direction, float spd, float dmg)
        {
            dir = direction.normalized;
            speed = spd;
            damage = dmg;
        }

        private void Update()
        {
            // 朝固定方向匀速直线飞行
            transform.Translate(dir * speed * Time.deltaTime, Space.World);

            // 超时自动销毁，防止子弹永远留在场景里
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= lifetime)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 碰到物体时触发。
        /// 只有碰到带 PlayerHealth 的物体（玩家）才造成伤害并销毁；
        /// 碰到玩家子弹等其他物体直接忽略，不销毁自身。
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 已销毁（同帧内后续回调）直接忽略，防止重复造成伤害 / 重复销毁
            if (destroyed)
                return;

            // 命中玩家：必须走 PlayerHealth.TakeDamage（吃无敌帧），禁止绕过
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                destroyed = true;
                playerHealth.TakeDamage(damage);
                Destroy(gameObject);
            }
            // 碰到其他物体（玩家子弹等）不销毁，继续飞行
        }
    }
}
