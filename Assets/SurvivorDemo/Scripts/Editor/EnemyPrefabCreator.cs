using UnityEditor;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 一键创建 Enemy 预制体。
    /// 菜单：SurvivorDemo → Create Enemy Prefab
    /// </summary>
    public static class EnemyPrefabCreator
    {
        private const string PrefabPath = "Assets/SurvivorDemo/Prefabs/Enemy.prefab";
        private const string SpritePath = "Assets/SurvivorDemo/Art/Sprites/EnemySprite.png";

        [MenuItem("SurvivorDemo/Create Enemy Prefab")]
        public static void CreateEnemyPrefab()
        {
            // 1. 创建红色方块 Sprite（如果不存在就创建）
            Sprite enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (enemySprite == null)
            {
                enemySprite = CreateEnemySprite();
            }

            // 2. 创建 Enemy GameObject
            GameObject enemy = new GameObject("Enemy");

            // 设置 Layer
            enemy.layer = LayerMask.NameToLayer("Enemy");

            // SpriteRenderer —— 红色方块
            SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
            sr.sprite = enemySprite;
            sr.color = Color.red;
            sr.sortingOrder = 1;

            // Rigidbody2D —— 无重力，用于碰撞检测
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // CircleCollider2D —— 圆形碰撞体（触发器）
            CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
            col.radius = 0.5f;
            col.isTrigger = true;

            // EnemyMovement —— 朝玩家移动
            enemy.AddComponent<EnemyMovement>();

            // EnemyHealth —— 血量与死亡
            enemy.AddComponent<EnemyHealth>();

            // EnemyHealthBar —— 头顶血条显示
            enemy.AddComponent<EnemyHealthBar>();

            // 3. 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(enemy, PrefabPath);

            // 4. 清理临时对象
            Object.DestroyImmediate(enemy);

            Debug.Log($"敌人预制体已创建：{PrefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        /// <summary>
        /// 创建一个 32x32 的红色圆形 Sprite，并保存到 Assets。
        /// </summary>
        private static Sprite CreateEnemySprite()
        {
            int size = 32;
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

            // 保存为 PNG 文件
            byte[] pngData = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(SpritePath, pngData);
            Object.DestroyImmediate(tex);

            AssetDatabase.Refresh();

            // 设置 Sprite 导入参数
            TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = size;
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        }
    }
}
