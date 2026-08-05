using System.Collections;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人血量组件。
    /// 职责单一：只管血量与死亡，不涉及移动和生成。
    /// 受伤时通知 EnemyHealthBar 更新显示，并触发击中特效/音效/震动/顿帧。
    /// </summary>
    public class EnemyHealth : MonoBehaviour
    {
        /// <summary>最大生命值（Inspector 可调）</summary>
        public float maxHP = 3f;

        /// <summary>掉落经验值（Inspector 可调）</summary>
        public int xpDropAmount = 5;

        /// <summary>经验宝珠预制体（Inspector 拖入）</summary>
        public GameObject xpOrbPrefab;

        /// <summary>当前生命值</summary>
        private float currentHP;

        /// <summary>是否已死亡（防止同帧被多颗子弹命中时重复掉落经验宝珠）</summary>
        private bool isDead;

        /// <summary>血条引用（同一 GameObject 上的 EnemyHealthBar）</summary>
        private EnemyHealthBar healthBar;

        // ======== 战斗手感相关 ========

        
























































































        /// <summary>精灵渲染器（用于击中闪白、死亡淡出）</summary>
        private SpriteRenderer spriteRenderer;

        /// <summary>原始颜色（闪白后恢复用）</summary>
        private Color originalColor;

        /// <summary>原始缩放（击中缩放恢复用）</summary>
        private Vector3 originalScale;

        /// <summary>闪白协程引用（防止重叠）</summary>
        private Coroutine flashCoroutine;

        /// <summary>击中缩放协程引用（防止与死亡消散冲突）</summary>
        private Coroutine punchCoroutine;


        private void Awake()
        {
            currentHP = maxHP;
            healthBar = GetComponent<EnemyHealthBar>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                originalColor = spriteRenderer.color;
            originalScale = transform.localScale;
        }

        /// <summary>
        /// 生成后由 EnemySpawner 调用，按时间倍率缩放血量。
        /// Awake 已把 currentHP 设为初始 maxHP，这里重设 maxHP 并同步 currentHP。
        /// </summary>
        public void ScaleMaxHP(float multiplier)
        {
            maxHP *= multiplier;
            currentHP = maxHP;
        }

        /// <summary>
        /// 受到伤害。
        /// 扣血后更新血条，生命值小于等于 0 时调用 Die() 死亡。
        /// 同时触发击中闪白、光圈、音效、震动、顿帧。
        /// </summary>
        /// <param name="amount">伤害数值</param>
        /// <param name="isCrit">是否暴击（暴击特效更强）</param>
        public void ReceiveDamage(float amount, bool isCrit = false)
        {
            // 已死亡就不再受理伤害，防止同帧多颗子弹命中导致重复掉落
            if (isDead)
                return;

            currentHP -= amount;

            // 更新血条显示
            if (healthBar != null)
            {
                healthBar.UpdateBar(currentHP, maxHP);
            }

            // A1: 击中闪白
            if (spriteRenderer != null)
            {
                if (flashCoroutine != null)
                    StopCoroutine(flashCoroutine);
                flashCoroutine = StartCoroutine(FlashWhite());
            }

            












































































































            // A3: 击中光圈（暴击黄色）
            CombatVFX.Instance?.SpawnHitRing(transform.position, isCrit ? Color.yellow : Color.white);

            // F1: 伤害数字弹出（暴击更大更黄）
            CombatVFX.Instance?.SpawnDamageNumber(transform.position, amount, isCrit);

            // F6: 击中缩放反馈（punch zoom）
            if (punchCoroutine != null)
                StopCoroutine(punchCoroutine);
            punchCoroutine = StartCoroutine(HitPunch(isCrit));

            // C2: 击中音效
            AudioManager.Instance?.PlaySFX(isCrit ? "crit_hit" : "hit", 0.5f);

            // E1: 顿帧（暴击更长）
            HitStopController hitStop = Camera.main != null ? Camera.main.GetComponent<HitStopController>() : null;
            hitStop?.Stop(isCrit ? 0.08f : 0.04f);

            // 血量归零 → 死亡
            if (currentHP <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// 敌人死亡。
        /// 掉落经验宝珠，播放死亡特效/音效/震动，然后缩放消散后销毁。
        /// </summary>
        private void Die()
        {
            // 先置死亡标记，再执行掉落与销毁
            isDead = true;
            GameStats.kills++; // 计入全局击杀数（结算界面用，重开时归零）

            // 掉落经验宝珠，并把经验值设置到宝珠上
            if (xpOrbPrefab != null)
            {
                GameObject orb = Instantiate(xpOrbPrefab, transform.position, Quaternion.identity);
                XPOrb xpOrb = orb.GetComponent<XPOrb>();
                if (xpOrb != null)
                {
                    xpOrb.SetXP(xpDropAmount);
                }
            }

            // B2: 死亡爆炸粒子
            Color deathColor = spriteRenderer != null ? spriteRenderer.color : Color.red;
            CombatVFX.Instance?.SpawnDeathParticles(transform.position, deathColor, 10);

            // C2: 死亡音效
            AudioManager.Instance?.PlaySFX("death", 0.6f);

            






















































































































































































































































            // 禁用所有行为脚本（移动、攻击等），保留自身用于协程
            foreach (var comp in GetComponents<MonoBehaviour>())
                if (comp != this)
                    comp.enabled = false;

            // 隐藏血条子物体
            Transform bg = transform.Find("HealthBarBG");
            Transform fill = transform.Find("HealthBarFill");
            if (bg != null) bg.gameObject.SetActive(false);
            if (fill != null) fill.gameObject.SetActive(false);

            // 停止击中缩放协程并恢复原始缩放，避免与死亡消散冲突
            if (punchCoroutine != null)
            {
                StopCoroutine(punchCoroutine);
                punchCoroutine = null;
                transform.localScale = originalScale;
            }

            // 禁用碰撞体
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            // B1: 死亡缩放消散
            StartCoroutine(DeathDissolve());
        }

        /// <summary>击中闪白：瞬间变白，0.06s 后恢复原色</summary>
        private IEnumerator FlashWhite()
        {
            if (spriteRenderer == null)
                yield break;

            spriteRenderer.color = Color.white;
            yield return new WaitForSecondsRealtime(0.06f);

            // 如果已经死亡（消散中），不恢复颜色
            if (!isDead && spriteRenderer != null)
                spriteRenderer.color = originalColor;

            flashCoroutine = null;
        }

        /// <summary>
        /// 击中缩放反馈（punch zoom）：
        /// 瞬间缩小到 0.8 倍，再用 0.12s 弹回原大小。
        /// 暴击缩小更猛（0.7 倍），弹回也更猛。
        /// 使用 unscaledDeltaTime，在顿帧期间也能播放。
        /// </summary>
        private IEnumerator HitPunch(bool isCrit)
        {
            float minScale = isCrit ? 0.65f : 0.75f;
            float duration = 0.12f;
            float elapsed = 0f;

            // 瞬间缩小
            transform.localScale = originalScale * minScale;

            // 弹回原大小
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float scale = Mathf.Lerp(minScale, 1f, t);
                transform.localScale = originalScale * scale;
                yield return null;
            }

            transform.localScale = originalScale;
            punchCoroutine = null;
        }

        /// <summary>死亡消散：缩小 + 淡出，0.25s 后销毁</summary>
        private IEnumerator DeathDissolve()
        {
            float duration = 0.25f;
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                transform.localScale = startScale * (1f - t);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f - t;
                    spriteRenderer.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
