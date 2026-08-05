using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 战斗特效单例。负责生成击中光圈和死亡爆炸粒子。
    /// 所有特效用程序化 Sprite + 协程驱动，无外部资源依赖。
    /// 使用 unscaledDeltaTime 播放，不受顿帧影响（增强打击感）。
    /// </summary>
    public class CombatVFX : MonoBehaviour
    {
        public static CombatVFX Instance { get; private set; }

        /// <summary>缓存的圆形 Sprite（特效通用）</summary>
        private Sprite circleSprite;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            circleSprite = CreateCircleSprite(16);
        }

        /// <summary>
        /// 生成击中光圈：在指定位置快速放大并淡出的圆形。
        /// </summary>
        public void SpawnHitRing(Vector2 position, Color color)
        {
            GameObject ring = new GameObject("HitRing");
            ring.transform.position = position;

            SpriteRenderer sr = ring.AddComponent<SpriteRenderer>();
            sr.sprite = circleSprite;
            sr.color = new Color(color.r, color.g, color.b, 0.8f);
            sr.sortingOrder = 5;

            StartCoroutine(AnimateHitRing(ring, sr));
        }

        /// <summary>
        /// 生成死亡爆炸粒子：从指定位置向各方向飞散的小圆形，移动+缩小+淡出。
        /// </summary>
        public void SpawnDeathParticles(Vector2 position, Color color, int count = 10)
        {
            for (int i = 0; i < count; i++)
            {
                GameObject particle = new GameObject("DeathParticle");
                particle.transform.position = position;

                SpriteRenderer sr = particle.AddComponent<SpriteRenderer>();
                sr.sprite = circleSprite;
                sr.color = color;
                sr.sortingOrder = 4;

                float angle = (float)i / count * Mathf.PI * 2f + Random.Range(-0.3f, 0.3f);
                Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                float speed = Random.Range(2f, 5f);
                float size = Random.Range(0.12f, 0.22f);
                particle.transform.localScale = new Vector3(size, size, 1f);

                StartCoroutine(AnimateDeathParticle(particle, sr, dir, speed));
            }
        }

        /// <summary>
        /// 生成伤害数字：在击中位置上方飘起并淡出。
        /// 暴击数字更大、黄色，并带有轻微水平偏移。
        /// </summary>
        public void SpawnDamageNumber(Vector2 position, float damage, bool isCrit)
        {
            GameObject textObj = new GameObject("DamageNumber");
            textObj.transform.position = new Vector3(position.x, position.y + 0.5f, 0f);

            TextMesh tm = textObj.AddComponent<TextMesh>();
            tm.text = Mathf.RoundToInt(damage).ToString();
            tm.fontSize = isCrit ? 28 : 18;
            tm.color = isCrit ? Color.yellow : Color.white;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;

            textObj.GetComponent<MeshRenderer>().sortingOrder = 20;

            StartCoroutine(AnimateDamageNumber(textObj, tm, isCrit));
        }

        private IEnumerator AnimateHitRing(GameObject go, SpriteRenderer sr)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            Vector3 startScale = new Vector3(0.3f, 0.3f, 1f);
            Vector3 endScale = new Vector3(0.9f, 0.9f, 1f);
            Color startColor = sr.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                go.transform.localScale = Vector3.Lerp(startScale, endScale, t);
                sr.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            Destroy(go);
        }

        private IEnumerator AnimateDeathParticle(GameObject go, SpriteRenderer sr, Vector2 dir, float speed)
        {
            float duration = 0.4f;
            float elapsed = 0f;
            Vector3 startPos = go.transform.position;
            Vector3 startScale = go.transform.localScale;
            Color startColor = sr.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                go.transform.position = startPos + (Vector3)(dir * speed * elapsed);
                go.transform.localScale = startScale * (1f - t);
                sr.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            Destroy(go);
        }

        private Sprite CreateCircleSprite(int resolution)
        {
            Texture2D tex = new Texture2D(resolution, resolution);
            float center = resolution / 2f;
            float radius = center - 1f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        }

        private IEnumerator AnimateDamageNumber(GameObject go, TextMesh tm, bool isCrit)
        {
            float duration = 0.3f;
            float elapsed = 0f;
            Vector3 startPos = go.transform.position;
            float floatSpeed = isCrit ? 1.0f : 0.8f;
            float xDrift = Random.Range(-0.3f, 0.3f);
            Color startColor = tm.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;

                go.transform.position = startPos + new Vector3(xDrift * t, floatSpeed * t, 0f);

                // 暴击数字先放大再缩小；普通数字直接淡出
                float scale = isCrit
                    ? 1f + 0.3f * Mathf.Sin(t * Mathf.PI)
                    : 1f - t * 0.2f;
                go.transform.localScale = new Vector3(scale, scale, 1f);

                // 后半段淡出
                tm.color = t < 0.5f ? startColor : Color.Lerp(startColor, endColor, (t - 0.5f) * 2f);

                yield return null;
            }

            Destroy(go);
        }
    }
}
