using UnityEditor;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 一键创建三种敌人预制体（圆形/三角/方块）+ 敌人子弹预制体。
    /// 菜单：SurvivorDemo → Create XXX Prefab
    /// 共用 CreateEnemy 通用方法，用不同的 Sprite / 参数 / 组件组合出不同敌人。
    /// </summary>
    public static class EnemyPrefabCreator
    {
        // ===== 预制体路径 =====
        private const string EnemyPrefabPath = "Assets/SurvivorDemo/Prefabs/Enemy.prefab";
        private const string TriangleEnemyPrefabPath = "Assets/SurvivorDemo/Prefabs/TriangleEnemy.prefab";
        private const string SquareEnemyPrefabPath = "Assets/SurvivorDemo/Prefabs/SquareEnemy.prefab";
        private const string EnemyBulletPrefabPath = "Assets/SurvivorDemo/Prefabs/EnemyBullet.prefab";
        private const string BossPrefabPath = "Assets/SurvivorDemo/Prefabs/BossEnemy.prefab";

        // ===== Sprite 路径 =====
        private const string EnemySpritePath = "Assets/SurvivorDemo/Art/Sprites/EnemySprite.png";
        private const string TriangleSpritePath = "Assets/SurvivorDemo/Art/Sprites/TriangleSprite.png";
        private const string SquareSpritePath = "Assets/SurvivorDemo/Art/Sprites/SquareSprite.png";
        private const string EnemyBulletSpritePath = "Assets/SurvivorDemo/Art/Sprites/EnemyBulletSprite.png";
        private const string BossSpritePath = "Assets/SurvivorDemo/Art/Sprites/BossSprite.png";

        private const string XpOrbPrefabPath = "Assets/SurvivorDemo/Prefabs/XPOrb.prefab";

        /// <summary>保留现有菜单：红色圆形敌人（无射击）</summary>
        [MenuItem("SurvivorDemo/Create Enemy Prefab")]
        public static void CreateEnemyPrefab()
        {
            Sprite sprite = LoadOrCreateSprite(EnemySpritePath, () => CreateCircleTexture(32));
            CreateEnemy(EnemyPrefabPath, sprite, Color.red, 3f, 3f, 5, false, null, false);
        }

        /// <summary>三角怪：橙色，会发射子弹</summary>
        [MenuItem("SurvivorDemo/Create Triangle Enemy Prefab")]
        public static void CreateTriangleEnemyPrefab()
        {
            Sprite sprite = LoadOrCreateSprite(TriangleSpritePath, () => CreateTriangleTexture(32));
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyBulletPrefabPath);
            // 橙色，移动 2.5，maxHP 3，xp 5，挂 TriShooter
            CreateEnemy(TriangleEnemyPrefabPath, sprite, new Color(1f, 0.55f, 0.2f), 2.5f, 3f, 5, true, bulletPrefab, false);
        }

        /// <summary>方块怪：紫色，移动慢、血厚（重装）</summary>
        [MenuItem("SurvivorDemo/Create Square Enemy Prefab")]
        public static void CreateSquareEnemyPrefab()
        {
            Sprite sprite = LoadOrCreateSprite(SquareSpritePath, () => CreateSquareTexture(32));
            // 紫色，移动 1.5，maxHP 15，xp 10，BoxCollider2D
            CreateEnemy(SquareEnemyPrefabPath, sprite, new Color(0.6f, 0.2f, 0.8f), 1.5f, 15f, 10, false, null, true);
        }

        /// <summary>首领怪物：大圆、深红色、高血量、慢速、矩形伤害+弹幕技能</summary>
        [MenuItem("SurvivorDemo/Create Boss Prefab")]
        public static void CreateBossPrefab()
        {
            Sprite sprite = LoadOrCreateSprite(BossSpritePath, () => CreateCircleTexture(64), 32f);
            GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyBulletPrefabPath);

            GameObject boss = new GameObject("BossEnemy");
            boss.layer = LayerMask.NameToLayer("Enemy");

            SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.5f, 0f, 0f); // 深红
            sr.sortingOrder = 5;

            Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CircleCollider2D col = boss.AddComponent<CircleCollider2D>();
            col.radius = 1f;
            col.isTrigger = true;

            EnemyMovement movement = boss.AddComponent<EnemyMovement>();
            







































































































            movement.moveSpeed = 1.5f;
            movement.contactDamage = 20f;

            EnemyHealth health = boss.AddComponent<EnemyHealth>();
            health.maxHP = 100f;
            health.xpDropAmount = 50;

            GameObject xpOrbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XpOrbPrefabPath);
            if (xpOrbPrefab != null)
                health.xpOrbPrefab = xpOrbPrefab;

            EnemyHealthBar healthBar = boss.AddComponent<EnemyHealthBar>();
            healthBar.barWidth = 2f;
            healthBar.barOffsetY = 1.3f;

            BossMonster bossMonster = boss.AddComponent<BossMonster>();
            if (bulletPrefab != null)
                bossMonster.bulletPrefab = bulletPrefab;
            else
                Debug.LogWarning("未找到 EnemyBullet.prefab，首领将不会发射子弹。请先执行菜单：SurvivorDemo → Create Enemy Bullet Prefab");

            PrefabUtility.SaveAsPrefabAsset(boss, BossPrefabPath);
            Object.DestroyImmediate(boss);

            Debug.Log($"首领预制体已创建：{BossPrefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(BossPrefabPath);
        }

        /// <summary>敌人子弹：小圆、深红色，EnemyBullet + Kinematic RB + trigger 碰撞体</summary>
        [MenuItem("SurvivorDemo/Create Enemy Bullet Prefab")]
        public static void CreateEnemyBulletPrefab()
        {
            // 16×16 圆，pixelsPerUnit=32 → 渲染约 0.5 世界单位（小弹丸），与 trigger 碰撞体 0.15 半径大致匹配
            Sprite sprite = LoadOrCreateSprite(EnemyBulletSpritePath, () => CreateCircleTexture(16), 32f);

            GameObject bullet = new GameObject("EnemyBullet");
            bullet.layer = LayerMask.NameToLayer("Enemy");

            SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(0.6f, 0f, 0f); // 深红色
            sr.sortingOrder = 2;

            Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 不受重力，不被物理推动
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
            col.radius = 0.15f;
            col.isTrigger = true;

            bullet.AddComponent<EnemyBullet>();

            PrefabUtility.SaveAsPrefabAsset(bullet, EnemyBulletPrefabPath);
            Object.DestroyImmediate(bullet);

            Debug.Log($"敌人子弹预制体已创建：{EnemyBulletPrefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyBulletPrefabPath);
        }

        /// <summary>
        /// 通用敌人创建：搭好 Layer / 渲染 / 刚体 / 碰撞体 / 移动 / 血量 / 血条，保存为预制体。
        /// </summary>
        /// <param name="prefabPath">保存路径</param>
        /// <param name="sprite">敌人 Sprite（白色，用 color 染色）</param>
        /// <param name="color">SpriteRenderer 颜色</param>
        /// <param name="moveSpeed">移动速度</param>
        /// <param name="maxHP">最大血量</param>
        /// <param name="xpDrop">掉落经验</param>
        /// <param name="withShooter">是否挂 TriShooter（三角怪）</param>
        /// <param name="bulletPrefab">敌人子弹预制体（TriShooter 用）</param>
        /// <param name="useBoxCollider">方块怪用 BoxCollider2D，其余用 CircleCollider2D</param>
        private static void CreateEnemy(string prefabPath, Sprite sprite, Color color, float moveSpeed, float maxHP, int xpDrop, bool withShooter, GameObject bulletPrefab, bool useBoxCollider)
        {
            GameObject enemy = new GameObject(System.IO.Path.GetFileNameWithoutExtension(prefabPath));

            // Layer
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // 渲染：白 Sprite + 染色
            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 1;

            // 刚体：无重力，用于碰撞检测
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // 碰撞体：方块用 Box(size 1×1)，其余用 Circle(radius 0.5)，都是触发器
            if (useBoxCollider)
            {
                BoxCollider2D box = enemy.AddComponent<BoxCollider2D>();
                box.size = new Vector2(1f, 1f);
                box.isTrigger = true;
            }
            else
            {
                CircleCollider2D circle = enemy.AddComponent<CircleCollider2D>();
                circle.radius = 0.5f;
                circle.isTrigger = true;
            }

            // 移动：朝玩家
            EnemyMovement movement = enemy.AddComponent<EnemyMovement>();
            movement.moveSpeed = moveSpeed;

            // 血量与掉落
            EnemyHealth health = enemy.AddComponent<EnemyHealth>();
            health.maxHP = maxHP;
            health.xpDropAmount = xpDrop;

            // 经验宝石引用
            GameObject xpOrbPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XpOrbPrefabPath);
            if (xpOrbPrefab != null)
            {
                health.xpOrbPrefab = xpOrbPrefab;
            }
            else
            {
                Debug.LogWarning("未找到 XPOrb.prefab，请先执行菜单：SurvivorDemo → Create XP Orb Prefab");
            }

            // 头顶血条
            enemy.AddComponent<EnemyHealthBar>();

            // 三角怪：额外挂射击组件
            if (withShooter)
            {
                TriShooter shooter = enemy.AddComponent<TriShooter>();
                if (bulletPrefab != null)
                {
                    shooter.bulletPrefab = bulletPrefab;
                }
                else
                {
                    Debug.LogWarning("未找到 EnemyBullet.prefab，三角怪将不会射击。请先执行菜单：SurvivorDemo → Create Enemy Bullet Prefab，然后重新运行本菜单");
                }
            }

            // 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(enemy, prefabPath);

            // 清理临时对象
            Object.DestroyImmediate(enemy);

            Debug.Log($"敌人预制体已创建：{prefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // ======== Sprite 加载 / 创建 ========

        /// <summary>加载已有 Sprite；不存在则用 createTexture 创建纹理并保存为 PNG 后返回</summary>
        private static Sprite LoadOrCreateSprite(string spritePath, System.Func<Texture2D> createTexture, float pixelsPerUnit = 32f)
        {
            Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            if (existing != null)
                return existing;

            Texture2D tex = createTexture();
            return SaveSprite(spritePath, tex, pixelsPerUnit);
        }

        /// <summary>把纹理编码为 PNG 保存到 Assets，配置 Sprite 导入参数，返回 Sprite 资源</summary>
        private static Sprite SaveSprite(string path, Texture2D tex, float pixelsPerUnit)
        {
            // 保存为 PNG 文件
            byte[] pngData = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, pngData);
            Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();

            // 设置 Sprite 导入参数
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = pixelsPerUnit;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        /// <summary>创建圆形纹理（白色，供染色用）</summary>
        private static Texture2D CreateCircleTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>创建三角形纹理（白色，逐像素判断是否在三角形内）</summary>
        private static Texture2D CreateTriangleTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            // 三角形三个顶点：底边左角 / 底边右角 / 顶部尖角
            Vector2 a = new Vector2(4f, 4f);
            Vector2 b = new Vector2(28f, 4f);
            Vector2 c = new Vector2(16f, 28f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 用像素中心点判断，边缘更平滑
                    bool inside = PointInTriangle(new Vector2(x + 0.5f, y + 0.5f), a, b, c);
                    tex.SetPixel(x, y, inside ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>创建方形纹理（白色实心）</summary>
        private static Texture2D CreateSquareTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    tex.SetPixel(x, y, Color.white);
                }
            }
            tex.Apply();
            return tex;
        }

        /// <summary>判断点 p 是否在三角形 abc 内（叉积符号法：全正或全负则在内部）</summary>
        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Cross(p, a, b);
            float d2 = Cross(p, b, c);
            float d3 = Cross(p, c, a);
            bool hasNeg = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPos = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNeg && hasPos);
        }

        /// <summary>二维叉积的 z 分量：用于点在三角形内的符号判断</summary>
        private static float Cross(Vector2 p, Vector2 a, Vector2 b)
        {
            return (p.x - b.x) * (a.y - b.y) - (a.x - b.x) * (p.y - b.y);
        }
    }
}
