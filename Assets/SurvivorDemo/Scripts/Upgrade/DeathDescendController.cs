using System.Collections;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 死神降临控制器（挂在 Player 上，由 LevelUpManager 通过 AddComponent 创建）。
    /// 每 30 秒释放一次死亡波：清除普通敌人，对 Boss 造成大量伤害。
    /// </summary>
    public class DeathDescendController : MonoBehaviour
    {
        /// <summary>死亡波触发间隔（秒）</summary>
        private const float WaveInterval = 30f;

        /// <summary>死亡波视觉扩张最大半径</summary>
        private const float WaveMaxRadius = 30f;

        /// <summary>死亡波视觉扩张持续时间（秒）</summary>
        private const float WaveExpandDuration = 0.5f;

        /// <summary>死亡波视觉淡出持续时间（秒）</summary>
        private const float WaveFadeDuration = 0.3f;

        /// <summary>玩家武器（读取 damage 作为 Boss 伤害基础）</summary>
        private PlayerWeapon weapon;

        /// <summary>计时器</summary>
        private float timer;

        /// <summary>死亡波颜色</summary>
        private static readonly Color WaveColor = new Color(0.8f, 0.1f, 0.3f, 0.8f);

        private void Awake()
        {
            weapon = GetComponent<PlayerWeapon>();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= WaveInterval)
            {
                TriggerDeathWave();
                timer = 0f;
            }
        }

        /// <summary>触发死亡波</summary>
        private void TriggerDeathWave()
        {
            Vector3 center = transform.position;

            // 遍历所有活跃敌人
            EnemyMovement[] enemies = EnemyMovement.ActiveEnemies.ToArray();
            float bossDamage = (weapon != null ? weapon.damage : 1f) * 10f;

            foreach (EnemyMovement enemy in enemies)
            {
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh == null) continue;

                BossMonster boss = enemy.GetComponent<BossMonster>();
                if (boss != null)
                {
                    // Boss：大量伤害，不秒杀
                    eh.ReceiveDamage(bossDamage, true, gameObject);
                }
                else
                {
                    // 普通敌人：秒杀
                    eh.ReceiveDamage(99999f, false, gameObject);
                }
            }

            // 视觉：扩散圆环
            StartCoroutine(DeathWaveVisual(center));

            // 音效
            AudioManager.Instance?.PlaySFX("death", 0.8f);
        }

        /// <summary>死亡波视觉：从玩家位置向外扩张的圆环，淡出后销毁</summary>
        private IEnumerator DeathWaveVisual(Vector3 center)
        {
            GameObject waveObj = new GameObject("DeathWave");
            waveObj.transform.position = center;

            LineRenderer lr = waveObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder = 20;
            lr.startWidth = 0.5f;
            lr.endWidth = 0.5f;
            lr.startColor = WaveColor;
            lr.endColor = WaveColor;
            lr.loop = true;

            // 设置圆环顶点（64 段）
            const int segments = 64;
            lr.positionCount = segments;

            // 扩张阶段
            float elapsed = 0f;
            while (elapsed < WaveExpandDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / WaveExpandDuration;
                float radius = Mathf.Lerp(0.5f, WaveMaxRadius, t);

                for (int i = 0; i < segments; i++)
                {
                    float angle = (i / (float)segments) * Mathf.PI * 2f;
                    lr.SetPosition(i, center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius);
                }

                lr.startColor = new Color(WaveColor.r, WaveColor.g, WaveColor.b, WaveColor.a);
                lr.endColor = new Color(WaveColor.r, WaveColor.g, WaveColor.b, WaveColor.a);
                yield return null;
            }

            // 淡出阶段
            elapsed = 0f;
            while (elapsed < WaveFadeDuration)
            {
                elapsed += Time.deltaTime;
                float fade = 1f - elapsed / WaveFadeDuration;
                Color c = new Color(WaveColor.r, WaveColor.g, WaveColor.b, WaveColor.a * fade);
                lr.startColor = c;
                lr.endColor = c;
                yield return null;
            }

            Destroy(waveObj);
        }
    }
}
