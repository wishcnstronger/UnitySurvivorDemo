using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家武器组件（挂在 Player 上）。
    /// 每隔一段时间自动寻找最近的敌人，朝它发射一颗子弹。
    /// 本阶段只负责发射和飞行，不处理碰撞伤害。
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        /// <summary>子弹模板（可由 Inspector 拖入，或由 GameSetup 自动赋值）</summary>
        public GameObject bulletPrefab;

        /// <summary>攻击间隔（秒），每过这么久攻击一次</summary>
        public float fireInterval = 1f;

        /// <summary>索敌范围：只攻击这个半径以内的敌人</summary>
        public float searchRadius = 20f;

        /// <summary>武器攻击力，发射子弹时传给子弹</summary>
        public float damage = 1f;

        /// <summary>攻击计时器，累计到 fireInterval 时开火</summary>
        private float timer;

        private void Update()
        {
            // 累加计时
            timer += Time.deltaTime;

            // 达到攻击间隔时开火，然后重置计时器
            if (timer >= fireInterval)
            {
                Fire();
                timer = 0f;
            }
        }

        /// <summary>找到最近的敌人并朝它发射一颗子弹</summary>
        private void Fire()
        {
            // 没有子弹模板就不攻击
            if (bulletPrefab == null)
                return;

            // 找到最近的敌人
            Transform nearestEnemy = FindNearestEnemy();

            // 范围内没有敌人就不攻击
            if (nearestEnemy == null)
                return;

            // 计算发射方向（从玩家指向敌人，归一化）
            Vector2 direction = (nearestEnemy.position - transform.position).normalized;

            // 生成一颗子弹
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

            // 模板是隐藏的，生成后激活它
            bullet.SetActive(true);

            // 把飞行方向和伤害值告诉子弹
            Bullet bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null)
            {
                bulletComp.SetDirection(direction);
                bulletComp.SetDamage(damage);
            }
        }

        /// <summary>
        /// 查找场景中所有敌人，返回距离最近的敌人 Transform。
        /// 超出 searchRadius 范围的敌人不会被选中。
        /// </summary>
        private Transform FindNearestEnemy()
        {
            // 查找场景里所有带 EnemyMovement 组件的物体（即所有敌人）
            EnemyMovement[] enemies = FindObjectsOfType<EnemyMovement>();

            Transform nearest = null;
            float minDist = searchRadius; // 初始值 = 索敌范围，超出范围的忽略

            foreach (EnemyMovement enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);

                // 找到更近的敌人就记录下来
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }
    }
}
