using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人管理器（VS 风格：大量低血怪物蜂拥而至）。
    /// 生成节奏快、圆形怪占绝对主力、远程/冲锋怪稀有。
    /// 怪物 HP/伤害/速度三项随时间缩放（HP 指数、伤害线性、速度缓增有上限）。
    /// 整数分钟时生成首领怪物。
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        /// <summary>敌人预制体列表（顺序：圆形/三角/方块）</summary>
        public List<GameObject> enemyPrefabs;

        /// <summary>首领预制体</summary>
        public GameObject bossPrefab;

        /// <summary>基础生成间隔（秒）</summary>
        public float spawnInterval = 1.2f;

        /// <summary>最小生成间隔（秒），随时间递减到此值</summary>
        public float minSpawnInterval = 0.15f;

        /// <summary>最大同屏怪物数（防止帧率崩溃）</summary>
        public int maxEnemies = 150;

        /// <summary>可移动区域半宽（由 GameSetup 注入，与玩家移动区域相同）</summary>
        [HideInInspector]
        public float boundX = 18f;

        /// <summary>可移动区域半高（由 GameSetup 注入，与玩家移动区域相同）</summary>
        [HideInInspector]
        public float boundY = 18f;

        /// <summary>同屏怪物计数器（静态，EnemyHealth.Die 递减，GameSetup.ResetGame 重置）</summary>
        public static int ActiveEnemyCount;

        private Transform player;
        private float timer;
        private float elapsedTime;
        private int lastBossMinute;

        /// <summary>缓存的诅咒加速系数（每帧从 PlayerStats 读取）</summary>
        private float cachedCurseBoost;

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

            // 刷新诅咒加速系数
            PlayerStats ps = player != null ? player.GetComponent<PlayerStats>() : null;
            cachedCurseBoost = ps != null ? ps.CurseSpawnBoost : 0f;

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

        /// <summary>生成间隔随时间递减：每秒减少 0.02s，最小 0.15s，诅咒值额外加速</summary>
        private float GetCurrentSpawnInterval()
        {
            float baseInterval = Mathf.Max(minSpawnInterval, spawnInterval - elapsedTime * 0.02f);
            // 诅咒值加速生成：每点诅咒 +5% 速度（间隔 ×(1 - curse×0.05)）
            float curseMult = 1f - cachedCurseBoost;
            return baseInterval * curseMult;
        }

        /// <summary>生成数量随时间递增：每 15 秒 +1，最多 12 个</summary>
        private int GetCurrentSpawnCount()
        {
            return Mathf.Min(12, 2 + Mathf.FloorToInt(elapsedTime / 15f));
        }

        /// <summary>随时间动态调整敌人类型权重</summary>
        private int[] GetSpawnWeights(float minute)
        {
            if (minute < 3f)  return new[] { 75, 12, 13 };
            if (minute < 6f)  return new[] { 68, 17, 15 };
            if (minute < 10f) return new[] { 60, 22, 18 };
            return new[] { 55, 25, 20 };
        }

        /// <summary>按动态权重随机选敌人类型并生成，位置限定在可移动区域内</summary>
        private void SpawnEnemy()
        {
            // 同屏上限检查
            if (ActiveEnemyCount >= maxEnemies)
                return;

            float minute = elapsedTime / 60f;
            int[] weights = GetSpawnWeights(minute);

            int total = 0;
            for (int i = 0; i < weights.Length && i < enemyPrefabs.Count; i++)
                total += weights[i];

            int roll = Random.Range(0, total);
            int cumulative = 0;
            int selectedIndex = 0;

            for (int i = 0; i < weights.Length && i < enemyPrefabs.Count; i++)
            {
                cumulative += weights[i];
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
            ActiveEnemyCount++;

            // 三项随时间缩放：HP 指数、伤害线性、速度缓增有上限
            int min = Mathf.FloorToInt(minute);
            float hpMult = Mathf.Pow(1.15f, min);
            float dmgMult = 1f + min * 0.3f;
            float spdMult = Mathf.Min(2.5f, 1f + min * 0.08f);

            EnemyHealth health = enemy.GetComponent<EnemyHealth>();
            if (health != null)
                health.ScaleMaxHP(hpMult);

            EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.contactDamage *= dmgMult;
                movement.ScaleSpeed(spdMult);
            }

            // 三角怪子弹伤害也同步缩放
            TriShooter shooter = enemy.GetComponent<TriShooter>();
            if (shooter != null)
                shooter.damage *= dmgMult;

            // 方块怪冲锋伤害也同步缩放
            ChargeAttacker charger = enemy.GetComponent<ChargeAttacker>();
            if (charger != null)
                charger.chargeDamage *= dmgMult;
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
            ActiveEnemyCount++;

            // 首领血量：每分钟 ×0.7（温和增长，避免后期血条离谱）
            EnemyHealth health = boss.GetComponent<EnemyHealth>();
            if (health != null)
                health.ScaleMaxHP(1f + minute * 0.7f);

            // 首领接触伤害同步缩放
            EnemyMovement movement = boss.GetComponent<EnemyMovement>();
            if (movement != null)
                movement.contactDamage *= 1f + minute * 0.5f;

            // 首领技能伤害按时间缩放
            BossMonster bossMonster = boss.GetComponent<BossMonster>();
            if (bossMonster != null)
                bossMonster.ScaleWithTime(minute);
        }
    }
}
