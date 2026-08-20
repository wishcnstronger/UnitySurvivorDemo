using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 灵魂流控制器（挂在 Player 上，由 LevelUpManager 通过 AddComponent 创建）。
    /// 击杀敌人时按概率生成灵魂实体，灵魂自动飞向最近敌人造成伤害。
    /// 可通过升级提升概率/伤害/连锁/上限，SoulCurse 激活穿透+掉血代价。
    /// </summary>
    public class SoulController : MonoBehaviour
    {
        /// <summary>灵魂预制体（运行时程序化生成，无需 Inspector 拖入）</summary>
        private GameObject soulPrefab;

        /// <summary>当前同时存在的灵魂列表</summary>
        private List<GameObject> activeSouls = new List<GameObject>();

        // ======== 升级等级 ========
        private int harvestLevel;   // SoulHarvest 等级
        private int powerLevel;      // SoulPower 等级
        private int chainLevel;      // SoulChain 等级
        private int swarmLevel;      // SoulSwarm 等级
        private bool curseActive;    // SoulCurse 是否激活

        // ======== 派生属性 ========
        private float SpawnChance => 0.15f + 0.05f * harvestLevel;
        private int MaxSouls => 3 + 3 * swarmLevel;
        private float DamageMultiplier => 0.5f + 0.3f * powerLevel;
        private int ChainCount => 3 + 2 * chainLevel;
        private bool SoulPenetrate => curseActive;

        /// <summary>玩家武器（读取 damage 作为灵魂基础伤害）</summary>
        private PlayerWeapon weapon;

        private void Awake()
        {
            weapon = GetComponent<PlayerWeapon>();
            CreateSoulPrefab();
        }

        private void Update()
        {
            // 清理已销毁的灵魂引用
            activeSouls.RemoveAll(s => s == null);
        }

        /// <summary>程序化创建灵魂预制体（小光球）</summary>
        private void CreateSoulPrefab()
        {
            soulPrefab = new GameObject("Soul");
            soulPrefab.SetActive(false);

            var sr = soulPrefab.AddComponent<SpriteRenderer>();
            // 程序化生成 16x16 蓝白色光球纹理
            const int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    float alpha = dist <= center ? 1f - dist / center * 0.5f : 0f;
                    tex.SetPixel(x, y, new Color(0.5f, 0.7f, 1f, alpha));
                }
            }
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 16);

            soulPrefab.AddComponent<Soul>();
            // Soul 的 [RequireComponent] 已自动添加 Rigidbody2D 和 CircleCollider2D
            var col = soulPrefab.GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;
            var rb = soulPrefab.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        /// <summary>敌人死亡时调用：按概率生成灵魂</summary>
        public void OnEnemyKilled(Vector3 position)
        {
            if (harvestLevel <= 0) return;
            if (Random.value > SpawnChance) return;
            if (activeSouls.Count >= MaxSouls) return;

            SpawnSoul(position);
        }

        private void SpawnSoul(Vector3 position)
        {
            GameObject soul = Instantiate(soulPrefab, position, Quaternion.identity);
            soul.SetActive(true);
            activeSouls.Add(soul);

            float baseDamage = weapon != null ? weapon.damage : 1f;
            float soulDamage = baseDamage * DamageMultiplier;

            var soulComp = soul.GetComponent<Soul>();
            soulComp.Initialize(
                owner: gameObject,
                damage: soulDamage,
                chainCount: ChainCount,
                penetrate: SoulPenetrate,
                curseActive: curseActive,
                ownerStats: GetComponent<PlayerStats>()
            );
        }

        // ======== 升级方法 ========

        public void SetLevel(int level) { harvestLevel = level; }
        public void SetPowerLevel(int level) { powerLevel = level; }
        public void SetChainLevel(int level) { chainLevel = level; }
        public void SetSwarmLevel(int level) { swarmLevel = level; }

        public void ActivateCurse()
        {
            curseActive = true;
        }
    }
}
