using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人移动组件。
    /// 每帧朝玩家方向移动，使用最简单的 MoveTowards 方式。
    /// 提供静态注册表替代 FindObjectsOfType，供 PlayerWeapon/Soul/ScytheController 查找敌人。
    /// </summary>
    public class EnemyMovement : MonoBehaviour
    {
        /// <summary>移动速度，单位/秒</summary>
        public float moveSpeed = 3f;

        /// <summary>接触伤害（碰到玩家时造成的基础伤害，由 EnemySpawner 按时间缩放）</summary>
        public float contactDamage = 10f;

        /// <summary>是否正在冲锋（冲锋期间跳过正常移动，由 ChargeAttacker 设置）</summary>
        [HideInInspector]
        public bool isCharging;

        /// <summary>玩家 Transform 引用</summary>
        private Transform player;

        // ======== 静态注册表（替代 FindObjectsOfType） ========
        private static readonly List<EnemyMovement> activeEnemies = new List<EnemyMovement>();

        /// <summary>获取所有活跃敌人列表（只读，不拷贝）</summary>
        public static List<EnemyMovement> ActiveEnemies => activeEnemies;

        private void OnEnable()
        {
            if (!activeEnemies.Contains(this))
                activeEnemies.Add(this);
        }

        private void OnDisable()
        {
            activeEnemies.Remove(this);
        }

        private void OnDestroy()
        {
            activeEnemies.Remove(this);
        }

        /// <summary>由 EnemySpawner 调用，按时间倍率缩放移动速度</summary>
        public void ScaleSpeed(float multiplier)
        {
            moveSpeed *= multiplier;
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        private void Update()
        {
            if (player == null)
                return;
            if (isCharging)
                return;

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                moveSpeed * Time.deltaTime
            );
        }
    }
}
