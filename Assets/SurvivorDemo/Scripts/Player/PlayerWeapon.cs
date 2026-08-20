using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家武器组件（挂在 Player 上）。
    /// 每隔一段时间自动寻找最近的敌人，朝它发射一颗子弹。
    /// 本阶段只负责发射和飞行，不处理碰撞伤害。
    /// </summary>
    public class PlayerWeapon : MonoBehaviour
    {
        /// <summary>子弹模板（可由 Inspector 拖入，或由 GameSetup 自动赋值）</summary>
        public GameObject bulletPrefab;

        /// <summary>攻击间隔（秒），每过这么久攻击一次</summary>
        public float fireInterval = 1f;

        /// <summary>索敌范围：只攻击这个半径以内的敌人</summary>
        public float searchRadius = 20f;

        /// <summary>武器攻击力，发射子弹时传给子弹</summary>
        public float damage = 1f;

        /// <summary>每轮发射的子弹数量</summary>
        public int bulletCount = 1;

        /// <summary>子弹可穿透的敌人数（0 = 命中第一个敌人就销毁）</summary>
        public int penetration = 0;

        /// <summary>多颗子弹之间的散射角度（度）</summary>
        public float spreadAngle = 15f;

        /// <summary>攻速上限（最小攻击间隔秒），攻速再高也不会低于此值</summary>
        public float minFireInterval = 0.1f;

        /// <summary>子弹寿命（秒），即射程——子弹飞行这么久后自动销毁（射程卡提升）</summary>
        public float bulletLifetime = 0.8f;

        /// <summary>暴击率（小数，0.05 = 5%），每颗子弹独立判定</summary>
        public float critChance = 0.05f;

        /// <summary>暴击倍率（暴击时伤害 × 此值）</summary>
        public float critMultiplier = 2f;

        /// <summary>死神之光是否已解锁（解锁后普通攻击切换为光束）</summary>
        private bool isDeathLightActive;

        /// <summary>光束 LineRenderer 池（按需创建，复用）</summary>
        private List<LineRenderer> beamLines = new List<LineRenderer>();

        /// <summary>当前激活光束的终点列表（每次开火时清空重建）</summary>
        private List<Vector3> beamEndPositions = new List<Vector3>();

        /// <summary>光束显示剩余时间（秒），归零后隐藏</summary>
        private float beamDisplayTimer;

        /// <summary>光束总持续时间（用于淡出比例计算）</summary>
        private float beamMaxDisplayTime;

        /// <summary>光束命中半径（光束宽度内的敌人都会被命中），可被升级强化</summary>
        public float beamRadius = 0.5f;

        /// <summary>光束数量（默认 1，可被升级强化）</summary>
        public int beamCount = 1;

        /// <summary>光束最大折射次数（默认 0，可被升级强化）</summary>
        public int beamRefraction = 0;

        /// <summary>光束基础起始色：青白核心</summary>
        private static readonly Color BeamStartColor = new Color(0.6f, 1f, 0.95f, 0.9f);

        /// <summary>光束基础结束色：紫色尾端</summary>
        private static readonly Color BeamEndColor = new Color(0.4f, 0.2f, 0.6f, 0.3f);

        /// <summary>攻击计时器，累计到 fireInterval 时开火</summary>
        private float timer;

        /// <summary>玩家属性（死亡判定用）</summary>
        private PlayerStats stats;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        private void Update()
        {
            // 死亡后停止攻击
            if (stats != null && stats.CurrentHP <= 0f)
                return;

            // 累加计时
            timer += Time.deltaTime;

            // 光束淡出 + 起点跟随（多光束）
            if (isDeathLightActive && beamDisplayTimer > 0f)
            {
                beamDisplayTimer -= Time.deltaTime;

                float fadeRatio = Mathf.Clamp01(beamDisplayTimer / beamMaxDisplayTime);
                Color startC = new Color(BeamStartColor.r, BeamStartColor.g, BeamStartColor.b, BeamStartColor.a * fadeRatio);
                Color endC = new Color(BeamEndColor.r, BeamEndColor.g, BeamEndColor.b, BeamEndColor.a * fadeRatio);

                for (int i = 0; i < beamEndPositions.Count; i++)
                {
                    if (i < beamLines.Count && beamLines[i] != null && beamLines[i].enabled)
                    {
                        beamLines[i].SetPosition(0, transform.position);
                        beamLines[i].SetPosition(1, beamEndPositions[i]);
                        beamLines[i].startColor = startC;
                        beamLines[i].endColor = endC;
                    }
                }

                if (beamDisplayTimer <= 0f)
                {
                    foreach (var bl in beamLines)
                        if (bl != null) bl.enabled = false;
                    beamEndPositions.Clear();
                }
            }

            // 达到攻击间隔时开火，然后重置计时器
            if (timer >= fireInterval)
            {
                Fire();
                timer = 0f;
            }
        }

        /// <summary>找到最近的敌人，朝它发射一圈子弹</summary>
        private void Fire()
        {
            // 死神之光：光束攻击
            if (isDeathLightActive)
            {
                FireDeathLight();
                return;
            }

            // 没有子弹模板就不攻击
            if (bulletPrefab == null)
                return;

            // 找到最近的敌人
            Transform nearestEnemy = FindNearestEnemy();

            // 范围内没有敌人就不攻击
            if (nearestEnemy == null)
                return;

            // 基准方向：从玩家指向最近敌人，归一化
            Vector2 baseDirection = (nearestEnemy.position - transform.position).normalized;

            // 敌人精确叠在玩家身上时差值为零向量，退化为朝上发射，避免产生静止不动的子弹
            if (baseDirection.sqrMagnitude < 0.0001f)
                baseDirection = Vector2.up;

            // 循环发射 bulletCount 颗子弹，各自绕基准方向散射
            for (int i = 0; i < bulletCount; i++)
            {
                // 计算这一颗的角度偏移（让多颗子弹对称散开）
                float offset = (i - (bulletCount - 1) / 2f) * spreadAngle;
                Vector2 dir = Rotate(baseDirection, offset * Mathf.Deg2Rad);

                // 生成一颗子弹
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

                // 模板是隐藏的，生成后激活它
                bullet.SetActive(true);

                // 每颗子弹独立判定暴击：暴击只加倍伤害，不影响穿透/子弹数
                bool isCrit = Random.value < critChance;
                float dmg = isCrit ? damage * critMultiplier : damage;

                // 把方向、伤害（含暴击）、穿透、寿命告诉子弹。
                // SetLifetime 在 Instantiate 之后、Start 之前同帧调用，先于子弹的 Destroy(gameObject, lifetime)
                Bullet bulletComp = bullet.GetComponent<Bullet>();
                if (bulletComp != null)
                {
                    bulletComp.SetDirection(dir);
                    bulletComp.SetDamage(dmg);
                    bulletComp.SetPenetration(penetration);
                    bulletComp.SetLifetime(bulletLifetime);
                    bulletComp.SetCrit(isCrit);
                    bulletComp.SetSource(gameObject);
                }
            }

            // C2: 射击音效
            AudioManager.Instance?.PlaySFX("shoot", 0.3f);
        }

        /// <summary>死神之光：穿透光束攻击（多光束，各命中半径内所有敌人）</summary>
        private void FireDeathLight()
        {
            EnemyMovement[] enemies = EnemyMovement.ActiveEnemies.ToArray();
            if (enemies.Length == 0)
                return;

            // 按距离排序，近的优先分配光束
            System.Array.Sort(enemies, (a, b) =>
                Vector2.Distance(transform.position, a.transform.position)
                .CompareTo(Vector2.Distance(transform.position, b.transform.position)));

            Vector2 beamStart = transform.position;

            // 基准方向（指向最近敌人）
            Vector2 baseDir = ((Vector2)enemies[0].transform.position - beamStart).normalized;
            if (baseDir.sqrMagnitude < 0.0001f)
                baseDir = Vector2.up;

            // 为每条光束确定方向
            Vector2[] dirs = new Vector2[beamCount];
            for (int i = 0; i < beamCount; i++)
            {
                if (i < enemies.Length)
                {
                    // 直接瞄准不同敌人
                    Vector2 d = ((Vector2)enemies[i].transform.position - beamStart).normalized;
                    dirs[i] = d.sqrMagnitude < 0.0001f ? baseDir : d;
                }
                else
                {
                    // 敌人不够时剩余光束扇形分散
                    int extra = beamCount - enemies.Length;
                    float fanAngle = (i - enemies.Length - (extra - 1) / 2f) * 20f;
                    dirs[i] = Rotate(baseDir, fanAngle * Mathf.Deg2Rad);
                }
            }

            // 逐条光束：伤害半径内敌人 + 配置 LineRenderer
            beamEndPositions.Clear();
            var flashed = new HashSet<Transform>();

            for (int i = 0; i < beamCount; i++)
            {
                Vector2 beamEnd = beamStart + dirs[i] * searchRadius;

                // 伤害该光束半径内所有敌人
                foreach (EnemyMovement enemy in enemies)
                {
                    float distToBeam = DistanceFromPointToSegment(enemy.transform.position, beamStart, beamEnd);
                    if (distToBeam <= beamRadius)
                    {
                        bool isCrit = Random.value < critChance;
                        float dmg = isCrit ? damage * critMultiplier : damage;
                        EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                        if (eh != null)
                        {
                            eh.ReceiveDamage(dmg, isCrit, gameObject);
                            // 同帧同敌人只闪光一次
                            if (flashed.Add(enemy.transform))
                                StartCoroutine(FlashHit(enemy.transform));
                        }
                    }
                }

                // 确保 LineRenderer 池足够
                while (beamLines.Count <= i)
                {
                    LineRenderer lr = gameObject.AddComponent<LineRenderer>();
                    lr.material = new Material(Shader.Find("Sprites/Default"));
                    lr.sortingOrder = 15;
                    lr.enabled = false;
                    beamLines.Add(lr);
                }

                beamLines[i].startWidth = beamRadius * 2f;
                beamLines[i].endWidth = beamRadius * 1.2f;
                beamLines[i].startColor = BeamStartColor;
                beamLines[i].endColor = BeamEndColor;
                beamLines[i].enabled = true;
                beamLines[i].SetPosition(0, transform.position);
                beamLines[i].SetPosition(1, beamEnd);
                beamEndPositions.Add(beamEnd);
            }

            beamMaxDisplayTime = Mathf.Min(0.2f, fireInterval * 0.5f);
            beamDisplayTimer = beamMaxDisplayTime;

            // 光束折射：主光束击杀敌人后，有概率向附近存活敌人折射
            if (beamRefraction > 0)
                ProcessRefraction(enemies);

            AudioManager.Instance?.PlaySFX("hit", 0.15f);
        }

        /// <summary>计算点 P 到线段 AB 的最短距离</summary>
        private float DistanceFromPointToSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float abSqrMag = ab.sqrMagnitude;
            if (abSqrMag < 0.0001f)
                return Vector2.Distance(p, a);

            float t = Vector2.Dot(p - a, ab) / abSqrMag;
            t = Mathf.Clamp01(t);
            Vector2 closest = a + ab * t;
            return Vector2.Distance(p, closest);
        }

        /// <summary>光束折射：从被击杀敌人位置向附近存活敌人折射</summary>
        private void ProcessRefraction(EnemyMovement[] allEnemies)
        {
            int remaining = beamRefraction;
            var pendingPositions = new List<Vector3>();

            // 收集被主光束击杀的敌人位置
            foreach (EnemyMovement enemy in allEnemies)
            {
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null && eh.IsDead)
                    pendingPositions.Add(enemy.transform.position);
            }

            var flashed = new HashSet<Transform>();

            while (remaining > 0 && pendingPositions.Count > 0)
            {
                var nextPositions = new List<Vector3>();

                foreach (Vector3 pos in pendingPositions)
                {
                    if (remaining <= 0)
                        break;

                    // 50% 概率折射
                    if (Random.value > 0.5f)
                        continue;

                    // 从死亡位置搜索半径内最近的存活敌人
                    Transform target = FindNearestLivingEnemyFrom(pos, searchRadius * 0.5f);
                    if (target == null)
                        continue;

                    // 造成伤害
                    bool isCrit = Random.value < critChance;
                    float dmg = isCrit ? damage * critMultiplier : damage;
                    EnemyHealth eh = target.GetComponent<EnemyHealth>();
                    if (eh == null)
                        continue;

                    eh.ReceiveDamage(dmg, isCrit, gameObject);
                    if (flashed.Add(target))
                        StartCoroutine(FlashHit(target));

                    // 折射光束视觉
                    StartCoroutine(ShowRefractionBeam(pos, target.position));

                    remaining--;

                    // 目标也死亡 → 继续链式折射
                    if (eh.IsDead)
                        nextPositions.Add(target.position);
                }

                pendingPositions = nextPositions;
            }
        }

        /// <summary>从指定位置搜索半径内最近的存活敌人</summary>
        private Transform FindNearestLivingEnemyFrom(Vector2 from, float radius)
        {
            EnemyMovement[] enemies = EnemyMovement.ActiveEnemies.ToArray();
            Transform nearest = null;
            float minDist = radius;

            foreach (EnemyMovement enemy in enemies)
            {
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null && eh.IsDead)
                    continue;

                float dist = Vector2.Distance(from, enemy.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }

        /// <summary>折射光束视觉：临时 LineRenderer 从起点到终点，0.15s 淡出后销毁</summary>
        private IEnumerator ShowRefractionBeam(Vector3 from, Vector3 to)
        {
            GameObject beamObj = new GameObject("RefractionBeam");
            LineRenderer lr = beamObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.sortingOrder = 14;
            lr.startWidth = beamRadius * 1.5f;
            lr.endWidth = beamRadius * 0.8f;
            lr.SetPosition(0, from);
            lr.SetPosition(1, to);

            float duration = 0.15f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float fade = 1f - elapsed / duration;
                lr.startColor = new Color(BeamStartColor.r, BeamStartColor.g, BeamStartColor.b, BeamStartColor.a * fade);
                lr.endColor = new Color(BeamEndColor.r, BeamEndColor.g, BeamEndColor.b, BeamEndColor.a * fade);
                yield return null;
            }

            Destroy(beamObj);
        }

        /// <summary>命中闪光协程：SpriteRenderer 短暂变白后恢复</summary>
        private IEnumerator FlashHit(Transform target)
        {
            SpriteRenderer sr = target.GetComponent<SpriteRenderer>();
            if (sr == null)
                yield break;

            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            sr.color = original;
        }

        /// <summary>解锁死神之光（由 LevelUpManager 调用）</summary>
        public void UnlockDeathLight()
        {
            isDeathLightActive = true;
        }

        /// <summary>设置光束数量（由 LevelUpManager 调用）</summary>
        public void SetBeamCount(int count)
        {
            beamCount = Mathf.Max(1, count);
        }

        /// <summary>设置光束命中半径（由 LevelUpManager 调用）</summary>
        public void SetBeamRadius(float radius)
        {
            beamRadius = Mathf.Max(0.1f, radius);
        }

        /// <summary>设置光束最大折射次数（由 LevelUpManager 调用）</summary>
        public void SetBeamRefraction(int count)
        {
            beamRefraction = Mathf.Max(0, count);
        }

        /// <summary>伤害百分比加成（percent > 0 表示增伤，如 0.5 = +50%）</summary>
        public void AddDamagePercent(float percent)
        {
            damage *= (1f + percent);
        }

        /// <summary>
        /// 将向量 v 绕原点旋转 radians 弧度。
        /// 用于计算多颗子弹的散射方向。
        /// </summary>
        private Vector2 Rotate(Vector2 v, float radians)
        {
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(
                v.x * cos - v.y * sin,
                v.x * sin + v.y * cos
            );
        }

        // ======== 升级方法（由 LevelUpManager 调用） ========

        /// <summary>攻击力加法强化</summary>
        public void AddDamage(float amount)
        {
            damage += amount;
        }

        /// <summary>攻速乘法强化（factor > 1 表示攻速变快），等价于攻击间隔除以 factor</summary>
        public void AddFireRateMultiplier(float factor)
        {
            fireInterval /= factor;
            if (fireInterval < minFireInterval)
                fireInterval = minFireInterval;
        }

        /// <summary>攻速是否已到上限（此时攻速卡零收益，升级抽卡时用于排除）</summary>
        public bool IsFireRateAtCap()
        {
            return fireInterval <= minFireInterval;
        }

        /// <summary>暴击率加法强化，上限 60%</summary>
        public void AddCritChance(float amount)
        {
            critChance = Mathf.Min(0.6f, critChance + amount);
        }

        /// <summary>暴击率是否已到上限</summary>
        public bool IsCritAtCap()
        {
            return critChance >= 0.6f;
        }

        /// <summary>
        /// 查找场景中所有敌人，返回距离最近的敌人 Transform。
        /// 超出 searchRadius 范围的敌人不会被选中。
        /// </summary>
        private Transform FindNearestEnemy()
        {
            // 查找场景里所有带 EnemyMovement 组件的物体（即所有敌人）
            EnemyMovement[] enemies = EnemyMovement.ActiveEnemies.ToArray();

            Transform nearest = null;
            float minDist = searchRadius; // 初始值 = 索敌范围，超出范围的忽略

            foreach (EnemyMovement enemy in enemies)
            {
                float dist = Vector2.Distance(transform.position, enemy.transform.position);

                // 找到更近的敌人就记录下来
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = enemy.transform;
                }
            }

            return nearest;
        }
    }
}
