using UnityEditor;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 一键创建经验宝石预制体。
    /// 菜单：SurvivorDemo → Create XP Orb Prefab
    /// </summary>
    public static class XPOrbPrefabCreator
    {
        private const string PrefabPath = "Assets/SurvivorDemo/Prefabs/XPOrb.prefab";
        private const string SpritePath = "Assets/SurvivorDemo/Art/Sprites/XPOrbSprite.png";

        [MenuItem("SurvivorDemo/Create XP Orb Prefab")]
        public static void CreateXPOrbPrefab()
        {
            // 1. 创建绿色菱形 Sprite（如果不存在就创建）
            Sprite orbSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
            if (orbSprite == null)
            {
                orbSprite = CreateOrbSprite();
            }

            // 2. 创建 XPOrb GameObject
            GameObject orb = new GameObject("XPOrb");

            // 设置 Layer
            orb.layer = LayerMask.NameToLayer("Pickup");

            // SpriteRenderer —— 绿色菱形
            SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
            sr.sprite = orbSprite;
            sr.sortingOrder = 1;

            // Rigidbody2D —— Kinematic，不受重力
            Rigidbody2D rb = orb.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // CircleCollider2D —— 触发器，用于拾取检测
            CircleCollider2D col = orb.AddComponent<CircleCollider2D>();
            col.isTrigger = true;

            







































































































































































            // XPOrb —— 磁铁吸引 + 拾取
            XPOrb xpOrb = orb.AddComponent<XPOrb>();
            xpOrb.xpValue = 10;

            // 3. 保存为预制体
            PrefabUtility.SaveAsPrefabAsset(orb, PrefabPath);

            // 4. 清理临时对象
            Object.DestroyImmediate(orb);

            Debug.Log($"经验宝石预制体已创建：{PrefabPath}");
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        }

        /// <summary>
        /// 创建 32x32 的绿色菱形 Sprite，并保存到 Assets。
        /// 菱形判定：|x-center| + |y-center| &lt;= radius。
        /// </summary>
        private static Sprite CreateOrbSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;
            float radius = center - 1f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Abs(x - center) + Mathf.Abs(y - center);
                    tex.SetPixel(x, y, dist <= radius ? Color.green : Color.clear);
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
