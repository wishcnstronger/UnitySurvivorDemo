using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 灵魂实体组件。
    /// 自动飞向最近敌人，命中造成伤害后消失或连锁跳向下一个敌人。
    /// SoulCurse 激活时穿透（可命中同一敌人多次）+ 每次生成扣玩家 1 HP。
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Soul : MonoBehaviour
    {
        /// <summary>飞行速度</summary>
        private const float Speed = 8f;

        /// <summary>伤害</summary>
        private float damage;

        /// <summary>剩余连锁次数</summary>
        private int chainCount;

        /// <summary>是否穿透（可命中同一敌人多次）</summary>
        private bool penetrate;

        /// <summary>是否激活诅咒（每次命中扣玩家 1 HP）</summary>
        private bool curseActive;

        /// <summary>伤害来源（Player，吸血用）</summary>
        private GameObject owner;

        /// <summary>玩家属性（诅咒掉血用）</summary>
        private PlayerStats ownerStats;

        /// <summary>已命中的敌人集合（非穿透模式下防止重复命中）</summary>
        private HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();

        /// <summary>是否已销毁</summary>
        private bool destroyed;

        /// <summary>当前追踪目标</summary>
        private Transform target;

        /// <summary>灵魂精灵渲染器（淡出用）</summary>
        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        /// <summary>初始化灵魂参数</summary>
        public void Initialize(GameObject owner, float damage, int chainCount,
            bool penetrate, bool curseActive, PlayerStats ownerStats)
        {
            this.owner = owner;
            this.damage = damage;
            this.chainCount = chainCount;
            this.penetrate = penetrate;
            this.curseActive = curseActive;
            this.ownerStats = ownerStats;

            // 诅咒代价：生成时扣 1 HP
            if (curseActive && ownerStats != null)
                ownerStats.TakeDamage(1f);

            FindNewTarget(null);
        }

        private void Update()
        {
            if (destroyed) return;

            // 目标失效或已死 → 找新目标
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                if (chainCount > 0)
                {
                    FindNewTarget(hitEnemies);
                    if (target == null)
                    {
                        // 没有新目标了 → 消散
                        Destroy(gameObject);
                        return;
                    }
                }
                else
                {
                    Destroy(gameObject);
                    return;
                }
            }

            // 飞向目标
            Vector2 dir = ((Vector2)target.position - (Vector2)transform.position).normalized;
            transform.Translate(dir * Speed * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (destroyed) return;

            EnemyHealth enemy = other.GetComponent<EnemyHealth>();
            if (enemy == null) return;
            if (hitEnemies.Contains(enemy) && !penetrate) return;

            hitEnemies.Add(enemy);
            enemy.ReceiveDamage(damage, false, owner);

            // 连锁：命中后跳向下一个敌人
            if (chainCount > 0)
            {
                chainCount--;
                // 连锁伤害递减 20%
                damage *= 0.8f;
                FindNewTarget(hitEnemies);
                if (target == null)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                destroyed = true;
                Destroy(gameObject);
            }
        }

        /// <summary>寻找最近的未命中敌人作为目标</summary>
        private void FindNewTarget(HashSet<EnemyHealth> exclude)
        {
            var enemies = EnemyMovement.ActiveEnemies;
            Transform nearest = null;
            float minDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                // 排除已命中的（非穿透模式）和已死亡的
                if (health != null && exclude != null && exclude.Contains(health))
                    continue;

                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }

            target = nearest;
        }

        private void OnDestroy()
        {
            destroyed = true;
        }
    }
}
