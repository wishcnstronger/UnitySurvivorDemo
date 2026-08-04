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

        /// <summary>每轮发射的子弹数量</summary>
        public int bulletCount = 1;

        /// <summary>子弹可穿透的敌人数（0 = 命中第一个敌人就销毁）</summary>
        public int penetration = 0;

        /// <summary>多颗子弹之间的散射角度（度）</summary>
        public float spreadAngle = 15f;

        /// <summary>攻速上限（最小攻击间隔秒），攻速再高也不会低于此值</summary>
        public float minFireInterval = 0.1f;

        /// <summary>子弹寿命（秒），即射程——子弹飞行这么久后自动销毁（射程卡提升）</summary>
        public float bulletLifetime = 2f;

        /// <summary>暴击率（小数，0.05 = 5%），每颗子弹独立判定</summary>
        public float critChance = 0.05f;

        /// <summary>暴击倍率（暴击时伤害 × 此值）</summary>
        public float critMultiplier = 2f;

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

        /// <summary>找到最近的敌人，朝它发射一圈子弹</summary>
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

            // 基准方向：从玩家指向最近敌人，归一化
            Vector2 baseDirection = (nearestEnemy.position - transform.position).normalized;

            // 敌人精确叠在玩家身上时差值为零向量，退化为朝上发射，避免产生静止不动的子弹
            if (baseDirection.sqrMagnitude < 0.0001f)
                baseDirection = Vector2.up;

            // 循环发射 bulletCount 颗子弹，各自绕基准方向散射
            for (int i = 0; i < bulletCount; i++)
            {
                // 计算这一颗的角度偏移（让多颗子弹对称散开）
                float offset = (i - (bulletCount - 1) / 2f) * spreadAngle;
                Vector2 dir = Rotate(baseDirection, offset * Mathf.Deg2Rad);

                // 生成一颗子弹
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

                // 模板是隐藏的，生成后激活它
                bullet.SetActive(true);

                // 每颗子弹独立判定暴击：暴击只加倍伤害，不影响穿透/子弹数
                float dmg = Random.value < critChance ? damage * critMultiplier : damage;

                // 把方向、伤害（含暴击）、穿透、寿命告诉子弹。
                // SetLifetime 在 Instantiate 之后、Start 之前同帧调用，先于子弹的 Destroy(gameObject, lifetime)
                Bullet bulletComp = bullet.GetComponent<Bullet>();
                if (bulletComp != null)
                {
                    bulletComp.SetDirection(dir);
                    bulletComp.SetDamage(dmg);
                    bulletComp.SetPenetration(penetration);
                    bulletComp.SetLifetime(bulletLifetime);
                }
            }
        }

        /// <summary>
        /// 将向量 v 绕原点旋转 radians 弧度。
        /// 用于计算多颗子弹的散射方向。
        /// </summary>
        private Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        // ======== 升级方法（由 LevelUpManager 调用） ========

        /// <summary>攻击力加法强化</summary>
        public void AddDamage(float amount)
        {
            damage += amount;
        }

        /// <summary>攻速乘法强化（factor > 1 表示攻速变快），等价于攻击间隔除以 factor</summary>
        public void AddFireRateMultiplier(float factor)
        {
            fireInterval /= factor;
            if (fireInterval < minFireInterval)
                fireInterval = minFireInterval;
        }

        /// <summary>攻速是否已到上限（此时攻速卡零收益，升级抽卡时用于排除）</summary>
        public bool IsFireRateAtCap()
        {
            return fireInterval <= minFireInterval;
        }

        /// <summary>子弹数量加法强化</summary>
        public void AddBulletCount(int count)
        {
            bulletCount += count;
        }

        /// <summary>穿透次数加法强化</summary>
        public void AddPenetration(int count)
        {
            penetration += count;
        }

        /// <summary>射程乘法强化：子弹寿命 × factor（越大飞得越远）</summary>
        public void AddBulletRange(float factor)
        {
            bulletLifetime *= factor;
        }

        /// <summary>暴击率加法强化，上限 60%（暴击卡调用）</summary>
        public void AddCritChance(float amount)
        {
            critChance = Mathf.Min(0.6f, critChance + amount);
        }

        /// <summary>暴击率是否已到上限（抽卡时用于排除零收益暴击卡）</summary>
        public bool IsCritAtCap()
        {
            return critChance >= 0.6f;
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
