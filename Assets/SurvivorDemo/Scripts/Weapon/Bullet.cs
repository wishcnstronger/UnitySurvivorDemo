using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 子弹组件。
    /// 负责朝指定方向匀速直线飞行，命中敌人造成伤害后销毁。
    /// RequireComponent 声明依赖组件，代码生成子弹时 Unity 会自动补齐，无需手动添加。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Bullet : MonoBehaviour
    {
        /// <summary>子弹飞行速度（单位/秒）</summary>
        public float speed = 10f;

        /// <summary>子弹存活时间（秒），超时自动销毁</summary>
        public float lifetime = 2f;

        /// <summary>子弹伤害值（由 PlayerWeapon 发射时传入）</summary>
        private float damage;

        /// <summary>飞行方向（由 PlayerWeapon 发射时传入）</summary>
        private Vector2 direction;

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
        /// 设置飞行方向。
        /// 由发射者（PlayerWeapon）在生成子弹后调用。
        /// </summary>
        /// <param name="dir">归一化的方向向量</param>
        public void SetDirection(Vector2 dir)
        {
            direction = dir;
        }

        /// <summary>
        /// 设置伤害值。
        /// 由发射者（PlayerWeapon）在生成子弹后调用。
        /// </summary>
        /// <param name="d">攻击力数值</param>
        public void SetDamage(float d)
        {
            damage = d;
        }

        private void Start()
        {
            // 到达存活时间后自动销毁，防止子弹永远留在场景里
            Destroy(gameObject, lifetime);
        }

        private void Update()
        {
            // 朝固定方向匀速直线飞行
            transform.Translate(direction * speed * Time.deltaTime);
        }

        /// <summary>
        /// 碰到物体时触发。
        /// 只有碰到带 EnemyHealth 的敌人时造成伤害并销毁子弹；
        /// 碰到玩家等其他物体直接忽略，子弹继续飞行。
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            // 尝试获取对方的血量组件
            EnemyHealth enemy = other.GetComponent<EnemyHealth>();

            // 对方没有血量组件（不是敌人，比如玩家）→ 忽略
            if (enemy == null)
                return;

            // 对敌人造成伤害
            enemy.ReceiveDamage(damage);

            // 子弹命中后立即销毁，不穿透
            Destroy(gameObject);
        }
    }
}
