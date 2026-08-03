using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人管理器。
    /// 按权重（50%圆形/30%三角/20%方块）在可移动区域内生成敌人。
    /// 生成间隔随时间递减，生成数量随时间递增，敌人血量和伤害随分钟递增。
    /// 整数分钟时生成首领怪物（高额血量与伤害，矩形伤害+弹幕技能）。
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        /// <summary>敌人预制体列表（顺序：圆形/三角/方块）</summary>
        public List<GameObject> enemyPrefabs;

        /// <summary>首领预制体</summary>
        public GameObject bossPrefab;

        /// <summary>基础生成间隔（秒）</summary>
        public float spawnInterval = 2f;

        /// <summary>最小生成间隔（秒），随时间递减到此值</summary>
        public float minSpawnInterval = 0.4f;

        /// <summary>可移动区域半宽（由 GameSetup 注入，与玩家移动区域相同）</summary>
        [HideInInspector]
        public float boundX = 18f;

        /// <summary>可移动区域半高（由 GameSetup 注入，与玩家移动区域相同）</summary>
        [HideInInspector]
        public float boundY = 18f;

        /// <summary>三种敌人权重：圆形50 / 三角30 / 方块20</summary>
        private static readonly int[] spawnWeights = { 50, 30, 20 };

        private Transform player;
        private float timer;
        private float elapsedTime;
        private int lastBossMinute;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        private void Update()
        {
            // 玩家引用失效（游戏重开时旧玩家被销毁）→ 重新查找并重置计时
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    timer = 0f;
                    elapsedTime = 0f;
                    lastBossMinute = 0;
                }
                return;
            }

            if (enemyPrefabs == null || enemyPrefabs.Count == 0)
                return;

            elapsedTime += Time.deltaTime;

            // 整数分钟生成首领
            int currentMinute = Mathf.FloorToInt(elapsedTime / 60f);
            if (currentMinute > lastBossMinute && bossPrefab != null)
            {
                SpawnBoss(currentMinute);
                lastBossMinute = currentMinute;
            }

            // 普通敌人生成
            timer += Time.deltaTime;
            float currentInterval = GetCurrentSpawnInterval();

            if (timer >= currentInterval)
            {
                int count = GetCurrentSpawnCount();
                for (int i = 0; i < count; i++)
                    SpawnEnemy();
                timer = 0f;
            }
        }

        /// <summary>生成间隔随时间递减：每秒减少 0.01s，最小 0.4s</summary>
        private float GetCurrentSpawnInterval()
        {
            return Mathf.Max(minSpawnInterval, spawnInterval - elapsedTime * 0.01f);
        }

        /// <summary>生成数量随时间递增：每 20 秒 +1，最多 8 个</summary>
        private int GetCurrentSpawnCount()
        {
            return Mathf.Min(8, 1 + Mathf.FloorToInt(elapsedTime / 20f));
        }

        /// <summary>按权重随机选敌人类型并生成，位置限定在可移动区域内</summary>
        private void SpawnEnemy()
        {
            int total = 0;
            for (int i = 0; i < spawnWeights.Length && i < enemyPrefabs.Count; i++)
                total += spawnWeights[i];

            int roll = Random.Range(0, total);
            int cumulative = 0;
            int selectedIndex = 0;

            for (int i = 0; i < spawnWeights.Length && i < enemyPrefabs.Count; i++)
            {
                cumulative += spawnWeights[i];
                if (roll < cumulative)
                {
                    selectedIndex = i;
                    break;
                }
            }

            // 在玩家周围生成，但钳制到可移动区域内
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Random.Range(6f, 10f);
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPos = (Vector2)player.position + dir * distance;

            spawnPos.x = Mathf.Clamp(spawnPos.x, -boundX, boundX);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -boundY, boundY);

            GameObject enemy = Instantiate(enemyPrefabs[selectedIndex], spawnPos, Quaternion.identity);

            // 血量和伤害随分钟缩放：每分钟 ×2
            int minute = Mathf.FloorToInt(elapsedTime / 60f);
            float hpMultiplier = 1f + minute * 1.0f;
            float dmgMultiplier = 1f + minute * 1.0f;

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
                health.ScaleMaxHP(hpMultiplier);

            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            if (movement != null)
                movement.contactDamage *= dmgMultiplier;

            // 三角怪子弹伤害也同步缩放
            TriShooter shooter = enemy.GetComponent<TriShooter>();
            if (shooter != null)
                shooter.damage *= dmgMultiplier;
        }

        /// <summary>在可移动区域边缘生成首领，血量与技能伤害按分钟缩放</summary>
        private void SpawnBoss(int minute)
        {
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float distance = Mathf.Min(boundX, boundY) - 2f;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPos = (Vector2)player.position + dir * distance;

            spawnPos.x = Mathf.Clamp(spawnPos.x, -boundX, boundX);
            spawnPos.y = Mathf.Clamp(spawnPos.y, -boundY, boundY);

            GameObject boss = Instantiate(bossPrefab, spawnPos, Quaternion.identity);

            // 首领血量：每分钟 ×3
            EnemyHealth health = boss.GetComponent<EnemyHealth>();
            if (health != null)
                health.ScaleMaxHP(1f + minute * 2f);

            // 首领接触伤害同步缩放
            EnemyMovement movement = boss.GetComponent<EnemyMovement>();
            if (movement != null)
                movement.contactDamage *= 1f + minute * 1.0f;

            // 首领技能伤害按时间缩放
            BossMonster bossMonster = boss.GetComponent<BossMonster>();
            if (bossMonster != null)
                bossMonster.ScaleWithTime(minute);
        }
    }
}
