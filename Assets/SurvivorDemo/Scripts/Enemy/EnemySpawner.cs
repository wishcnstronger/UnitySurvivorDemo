using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人管理器。
    /// 按固定间隔在玩家周围随机位置生成敌人。
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        /// <summary>敌人预制体（从 Inspector 拖入）</summary>
        public GameObject enemyPrefab;

        /// <summary>生成间隔（秒）</summary>
        public float spawnInterval = 2f;

        /// <summary>距离玩家的最小生成距离</summary>
        public float spawnMinDistance = 8f;

        /// <summary>距离玩家的最大生成距离</summary>
        public float spawnMaxDistance = 10f;

        /// <summary>玩家 Transform（通过 Tag 查找）</summary>
        private Transform player;

        /// <summary>计时器，累计时间到 spawnInterval 时触发生成</summary>
        private float timer;

        /// <summary>游戏开始时查找玩家</summary>
        private void Start()
        {
            // 通过 Tag 找到玩家
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        /// <summary>每帧更新计时器，到达间隔时生成敌人</summary>
        private void Update()
        {
            // 玩家不存在就不生成
            if (player == null)
                return;

            // 预制体未设置就不生成
            if (enemyPrefab == null)
                return;

            // 累加时间
            timer += Time.deltaTime;

            // 到达生成间隔
            if (timer >= spawnInterval)
            {
                SpawnEnemy();
                timer = 0f;
            }
        }

        /// <summary>
        /// 在玩家周围随机位置生成一个敌人。
        /// 生成位置距离玩家 spawnMinDistance ~ spawnMaxDistance 单位。
        /// </summary>
        private void SpawnEnemy()
        {
            // 随机角度（弧度）
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

            // 随机距离
            float distance = Random.Range(spawnMinDistance, spawnMaxDistance);

            // 计算生成位置
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 spawnPos = (Vector2)player.position + direction * distance;

            // 生成敌人
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }
}
