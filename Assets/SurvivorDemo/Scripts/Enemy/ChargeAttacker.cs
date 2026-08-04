using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 方块怪冲锋技能组件。
    /// 按冷却间隔朝玩家方向高速冲锋，冲锋期间只结算冲锋伤害，不结算碰撞伤害。
    /// 冲锋伤害由 PlayerHealth.OnTriggerStay2D 读取 isCharging 标志后调用本组件的 chargeDamage。
    /// </summary>
    [RequireComponent(typeof(EnemyMovement))]
    public class ChargeAttacker : MonoBehaviour
    {
        /// <summary>冲锋伤害（初始值，由 EnemySpawner 按时间缩放）</summary>
        public float chargeDamage = 20f;

        /// <summary>冲锋速度</summary>
        public float chargeSpeed = 12f;

        /// <summary>冲锋冷却（秒）</summary>
        public float chargeCooldown = 4f;

        /// <summary>冲锋持续时间（秒）</summary>
        public float chargeDuration = 0.8f;

        private EnemyMovement movement;
        private Transform player;
        private float timer;
        private bool isCharging;
        private Vector2 chargeDir;
        private float chargeTimer;

        private void Start()
        {
            movement = GetComponent<EnemyMovement>();
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        private void Update()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    player = playerObj.transform;
                return;
            }

            if (isCharging)
            {
                // 冲锋中：沿固定方向高速直线移动
                transform.Translate(chargeDir * chargeSpeed * Time.deltaTime, Space.World);

                chargeTimer -= Time.deltaTime;
                if (chargeTimer <= 0f)
                    EndCharge();
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= chargeCooldown)
                    StartCharge();
            }
        }

        private void StartCharge()
        {
            isCharging = true;
            if (movement != null)
                movement.isCharging = true;

            chargeDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            if (chargeDir.sqrMagnitude < 0.0001f)
                chargeDir = Vector2.up;

            chargeTimer = chargeDuration;
            timer = 0f;
        }

        private void EndCharge()
        {
            isCharging = false;
            if (movement != null)
                movement.isCharging = false;
        }

        /// <summary>当前是否正在冲锋（PlayerHealth 读取此标志区分伤害来源）</summary>
        public bool IsCharging => isCharging;
    }
}
