using System.Collections;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 镰刀近战控制器（挂在 Player 上，由 LevelUpManager 通过 AddComponent 创建）。
    /// 定时对前方扇形范围内敌人造成近战伤害，可升级范围/伤害/攻速。
    /// 挥砍时生成白色弧形特效（程序化 Sprite，不依赖外部资源）。
    /// </summary>
    public class ScytheController : MonoBehaviour
    {
        private float interval = 1.5f;
        private float range = 3.0f;
        private float damageMultiplier = 1.2f;
        private const float ArcAngle = 180f;

        private float timer;
        private PlayerWeapon weapon;
        private PlayerStats stats;
        private DeathDescendController deathDescend;

        /// <summary>镰刀击杀减技能CD秒数（0=未升级）</summary>
        private float scytheCooldownReduction;

        /// <summary>挥砍特效预制体（程序化生成，首次挥砍时创建）</summary>
        private static Sprite slashSprite;

        private void Awake()
        {
            weapon = GetComponent<PlayerWeapon>();
            stats = GetComponent<PlayerStats>();
            deathDescend = GetComponent<DeathDescendController>();
        }

        private void Update()
        {
            if (stats != null && stats.CurrentHP <= 0f) return;

            timer += Time.deltaTime;
            if (timer >= interval)
            {
                Swing();
                timer = 0f;
            }
        }

        private void Swing()
        {
            Transform nearest = FindNearestEnemy();
            Vector2 swingDir;
            if (nearest != null)
                swingDir = (nearest.position - transform.position).normalized;
            else
                swingDir = Vector2.up;

            var enemies = EnemyMovement.ActiveEnemies.ToArray();
            float baseDamage = weapon != null ? weapon.damage : 1f;
            float scytheDamage = baseDamage * damageMultiplier;

            bool isCrit = weapon != null && Random.value < weapon.critChance;
            if (isCrit) scytheDamage *= weapon.critMultiplier;

            bool hitAny = false;

            foreach (var enemy in enemies)
            {
                Vector2 toEnemy = enemy.transform.position - transform.position;
                float dist = toEnemy.magnitude;
                if (dist > range) continue;

                float angle = Vector2.Angle(swingDir, toEnemy);
                if (angle > ArcAngle * 0.5f) continue;

                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.ReceiveDamage(scytheDamage, isCrit, gameObject);
                    hitAny = true;

                    // 镰刀击杀减技能CD
                    if (scytheCooldownReduction > 0f && health.IsDead && deathDescend != null)
                        deathDescend.ReduceCooldown(scytheCooldownReduction);
                }
            }

            // 程序化挥砍特效
            SpawnSlashEffect(swingDir, hitAny);
        }

        /// <summary>生成白色弧形挥砍特效（程序化 Sprite + 淡出动画）</summary>
        private void SpawnSlashEffect(Vector2 dir, bool hitAny)
        {
            if (slashSprite == null)
                slashSprite = CreateSlashSprite();

            GameObject slashObj = new GameObject("ScytheSlash");
            slashObj.transform.position = transform.position;
            slashObj.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

            var sr = slashObj.AddComponent<SpriteRenderer>();
            sr.sprite = slashSprite;
            sr.sortingOrder = 15;
            sr.color = new Color(1f, 1f, 1f, 0.85f);
            sr.sortingLayerName = "Default";

            // 缩放：宽度=扇形半径，高度=弧长
            float arcLength = range * Mathf.PI; // 半圆弧长
            slashObj.transform.localScale = new Vector3(range * 0.6f, range * 0.8f, 1f);

            // 音效
            AudioManager.Instance?.PlaySFX("hit", 0.3f);

            StartCoroutine(SlashAnim(slashObj));
        }

        /// <summary>挥砍动画：快速放大+淡出，0.15s 销毁</summary>
        private IEnumerator SlashAnim(GameObject slashObj)
        {
            float duration = 0.15f;
            float elapsed = 0f;
            Vector3 startScale = slashObj.transform.localScale;
            var sr = slashObj.GetComponent<SpriteRenderer>();
            if (sr == null) { Destroy(slashObj); yield break; }

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                slashObj.transform.localScale = startScale * (1f + t * 0.3f);
                sr.color = new Color(1f, 1f, 1f, 0.85f * (1f - t));
                yield return null;
            }

            Destroy(slashObj);
        }

        /// <summary>程序化生成白色弧形挥砍 Sprite（月牙形）</summary>
        private static Sprite CreateSlashSprite()
        {
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float innerRadius = size * 0.2f;
            float outerRadius = size * 0.45f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    // 环形：内径到外径之间填充，只保留右半圆（弧形）
                    bool inRing = dist >= innerRadius && dist <= outerRadius;
                    bool isRightHalf = x >= center;

                    if (inRing && isRightHalf)
                    {
                        // 边缘渐隐
                        float edgeFade = 1f;
                        if (dist > outerRadius - 2f) edgeFade = (outerRadius - dist) / 2f;
                        if (dist < innerRadius + 2f) edgeFade = (dist - innerRadius) / 2f;
                        edgeFade = Mathf.Clamp01(edgeFade);

                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, edgeFade * 0.9f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }

            tex.filterMode = FilterMode.Point;
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.3f, 0.5f), 32);
        }

        private Transform FindNearestEnemy()
        {
            var enemies = EnemyMovement.ActiveEnemies.ToArray();
            Transform nearest = null;
            float minDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }
            return nearest;
        }

        // ======== 升级方法 ========

        public void SetRangeLevel(int level) { range = 3.0f + 0.5f * level; }
        public void SetDamageLevel(int level) { damageMultiplier = 1.2f + 0.3f * level; }
        public void SetSpeedLevel(int level) { interval = Mathf.Max(0.3f, 1.5f * (1f - 0.1f * level)); }
        public void SetCooldownReduction(float amount) { scytheCooldownReduction = amount; }
    }
}
