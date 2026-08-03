using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 三角怪射击组件（挂在三角怪上）。
    /// 三角怪本身仍用 EnemyMovement 正常追玩家，本组件额外负责：
    /// 玩家进入攻击范围后，按固定间隔朝玩家发射一颗 EnemyBullet。
    /// 边走边射，不做停止/射击状态机。
    /// 计时器用 deltaTime 累加：timeScale=0（开始界面/升级/结算）时天然暂停。
    /// </summary>
    public class TriShooter : MonoBehaviour
    {
        /// <summary>射击间隔（秒）</summary>
        public float attackInterval = 2f;

        /// <summary>攻击范围：玩家进入此距离内才射击</summary>
        public float attackRange = 6f;

        /// <summary>子弹伤害（传给 EnemyBullet）</summary>
        public float damage = 5f;

        /// <summary>子弹飞行速度（传给 EnemyBullet）</summary>
        public float bulletSpeed = 6f;

        /// <summary>敌人子弹预制体（Inspector 拖入）</summary>
        public GameObject bulletPrefab;

        /// <summary>射击计时器，累计到 attackInterval 时开火</summary>
        private float timer;

        /// <summary>玩家 Transform（Start 缓存，失效时重找）</summary>
        private Transform player;

        private void Start()
        {
            // 通过 Tag 查找玩家（同 EnemyMovement 的做法）
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        private void Update()
        {
            // 玩家引用失效（游戏重开时旧玩家被销毁，缓存引用变成 null）→ 重新查找
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
                return;
            }

            // 计时器累计（暂停时 deltaTime≈0，天然静止）
            timer += Time.deltaTime;

            // 达到间隔 且 玩家在攻击范围内 且 有子弹模板 → 射击
            if (timer >= attackInterval && bulletPrefab != null
                && Vector2.Distance(transform.position, player.position) <= attackRange)
            {
                Shoot();
                timer = 0f;
            }
        }

        /// <summary>朝玩家方向发射一颗敌人子弹</summary>
        private void Shoot()
        {
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;

            // 在自身位置沿飞行方向偏移一点再生成：
            // 若精确生成在自身位置，子弹的 trigger 碰撞体立刻与三角怪自己的碰撞体重叠，
            // 会被 EnemyBullet.OnTriggerEnter2D 判定为命中而瞬间销毁。
            Vector2 spawnPos = (Vector2)transform.position + dir * 0.8f;

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            // 把方向 / 速度 / 伤害告诉子弹
            EnemyBullet enemyBullet = bullet.GetComponent<EnemyBullet>();
            if (enemyBullet != null)
                enemyBullet.Setup(dir, bulletSpeed, damage);
        }
    }
}
