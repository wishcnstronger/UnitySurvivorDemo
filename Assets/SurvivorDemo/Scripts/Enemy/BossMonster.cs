using System.Collections;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 首领怪物组件。
    /// 两个技能：矩形伤害（在玩家位置生成矩形区域，预警后造成伤害）和弹幕扇形射击。
    /// 自身仍用 EnemyMovement 追玩家，本组件负责技能轮换。
    /// </summary>
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyHealth))]
    public class BossMonster : MonoBehaviour
    {
        [Header("矩形伤害技能")]
        public float rectAttackInterval = 5f;
        public float rectWidth = 4f;
        public float rectHeight = 2f;
        public float rectDamage = 30f;
        public float rectTelegraphTime = 1f;
        public float rectActiveTime = 0.5f;

        [Header("弹幕技能")]
        public float bulletAttackInterval = 3f;
        public int bulletCount = 8;
        public float bulletSpreadAngle = 60f;
        public float bulletDamage = 15f;
        public float bulletSpeed = 5f;

        [Header("子弹预制体")]
        public GameObject bulletPrefab;

        private Transform player;
        private float rectTimer;
        private float bulletTimer;

        private void Start()
        {
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

            rectTimer += Time.deltaTime;
            bulletTimer += Time.deltaTime;

            if (rectTimer >= rectAttackInterval)
            {
                StartCoroutine(RectAttack());
                rectTimer = 0f;
            }

            if (bulletTimer >= bulletAttackInterval && bulletPrefab != null)
            {
                BulletFanAttack();
                bulletTimer = 0f;
            }
        }

        /// <summary>按分钟缩放技能伤害和子弹数量</summary>
        public void ScaleWithTime(int minute)
        {
            rectDamage *= 1f + minute * 0.8f;
            bulletDamage *= 1f + minute * 0.5f;
            bulletCount += Mathf.FloorToInt(minute * 0.5f);
        }

        /// <summary>矩形伤害技能：在玩家位置生成矩形区域，预警后造成伤害</summary>
        private IEnumerator RectAttack()
        {
            Vector2 center = player.position;

            GameObject zone = new GameObject("BossRectZone");
            zone.transform.position = center;
            zone.transform.localScale = new Vector3(rectWidth, rectHeight, 1f);

            SpriteRenderer sr = zone.AddComponent<SpriteRenderer>();
            sr.sprite = CreateRectSprite();
            // 预警阶段：浅红色
            sr.color = new Color(1f, 0.6f, 0.6f, 0.3f);
            sr.sortingOrder = 0;

            // 安全销毁：即使协程被中断也能清理
            Destroy(zone, rectTelegraphTime + rectActiveTime + 0.1f);

            yield return new WaitForSeconds(rectTelegraphTime);

            // 激活阶段：深红色
            sr.color = new Color(0.6f, 0f, 0f, 0.7f);

            if (player != null)
            {
                Vector2 playerPos = player.position;
                float halfW = rectWidth / 2f;
                float halfH = rectHeight / 2f;

                if (Mathf.Abs(playerPos.x - center.x) <= halfW &&
                    Mathf.Abs(playerPos.y - center.y) <= halfH)
                {
                    PlayerHealth ph = player.GetComponent<PlayerHealth>();
                    if (ph != null)
                        ph.TakeDamage(rectDamage);
                }
            }

            yield return new WaitForSeconds(rectActiveTime);

            if (zone != null)
                Destroy(zone);
        }

        /// <summary>弹幕扇形射击：朝玩家方向发射多颗子弹</summary>
        private void BulletFanAttack()
        {
            Vector2 baseDir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            if (baseDir.sqrMagnitude < 0.0001f)
                baseDir = Vector2.up;

            for (int i = 0; i < bulletCount; i++)
            {
                float offset = (i - (bulletCount - 1) / 2f) * (bulletSpreadAngle / Mathf.Max(1, bulletCount - 1));
                Vector2 dir = Rotate(baseDir, offset * Mathf.Deg2Rad);

                Vector2 spawnPos = (Vector2)transform.position + dir * 1.6f;
                GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

                EnemyBullet eb = bullet.GetComponent<EnemyBullet>();
                if (eb != null)
                    eb.Setup(dir, bulletSpeed, bulletDamage);
            }
        }

        private Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }

        private static Sprite CreateRectSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
    }
}
