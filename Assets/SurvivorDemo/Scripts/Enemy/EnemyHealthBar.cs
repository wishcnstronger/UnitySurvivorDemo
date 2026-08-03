using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 敌人血条组件。
    /// 在敌人头顶显示血条（背景 + 填充），由 EnemyHealth 在受伤时调用更新。
    /// 使用 Start 创建血条，避免编辑模式下预制体保存时产生多余子物体。
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        /// <summary>血条宽度</summary>
        public float barWidth = 1f;

        /// <summary>血条高度</summary>
        public float barHeight = 0.15f;

        /// <summary>血条距离敌人中心的垂直偏移</summary>
        public float barOffsetY = 0.7f;

        /// <summary>填充条 Transform（左对齐，通过缩放宽度表示血量比例）</summary>
        private Transform barFill;

        /// <summary>填充条 SpriteRenderer，用于动态变色</summary>
        private SpriteRenderer fillRenderer;

        /// <summary>缓存的方形 Sprite（背景用）</summary>
        private static Sprite squareSprite;

        /// <summary>缓存的填充方形 Sprite（左对齐 pivot，所有敌人共用一份）</summary>
        private static Sprite fillSprite;

        /// <summary>
        /// 在 Start 中创建血条，避免编辑模式预制体保存时产生子物体。
        /// </summary>
        private void Start()
        {
            CreateBar();
        }

        /// <summary>
        /// 创建背景和填充两个 SpriteRenderer 子物体。
        /// 背景居中，填充左对齐（pivot 在左侧），缩放 x 表示血量比例。
        /// </summary>
        private void CreateBar()
        {
            if (squareSprite == null)
            {
                squareSprite = CreateSquareSprite();
            }

            // 背景：暗红色，居中
            GameObject bgObj = new GameObject("HealthBarBG");
            bgObj.transform.SetParent(transform, false);
            bgObj.transform.localPosition = new Vector3(0f, barOffsetY, 0f);
            bgObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            SpriteRenderer bgSr = bgObj.AddComponent<SpriteRenderer>();
            bgSr.sprite = squareSprite;
            bgSr.color = new Color(0.2f, 0f, 0f, 0.8f);
            bgSr.sortingOrder = 10;

            // 填充：左对齐，初始满血
            GameObject fillObj = new GameObject("HealthBarFill");
            fillObj.transform.SetParent(transform, false);
            fillObj.transform.localPosition = new Vector3(-barWidth * 0.5f, barOffsetY, 0f);
            fillObj.transform.localScale = new Vector3(barWidth, barHeight, 1f);

            barFill = fillObj.transform;
            fillRenderer = fillObj.AddComponent<SpriteRenderer>();
            if (fillSprite == null)
            {
                fillSprite = CreateFillSprite();
            }
            fillRenderer.sprite = fillSprite;
            fillRenderer.color = Color.green;
            fillRenderer.sortingOrder = 11;
        }

        /// <summary>
        /// 根据当前血量更新血条。
        /// 由 EnemyHealth.ReceiveDamage 调用。
        /// </summary>
        /// <param name="currentHP">当前血量</param>
        /// <param name="maxHP">最大血量</param>
        public void UpdateBar(float currentHP, float maxHP)
        {
            if (barFill == null)
                return;

            float ratio = Mathf.Clamp01(currentHP / maxHP);

            // 缩放填充宽度
            barFill.localScale = new Vector3(barWidth * ratio, barHeight, 1f);

            // 根据血量比例变色
            if (fillRenderer != null)
            {
                if (ratio > 0.5f)
                    fillRenderer.color = Color.green;
                else if (ratio > 0.25f)
                    fillRenderer.color = Color.yellow;
                else
                    fillRenderer.color = Color.red;
            }
        }

        /// <summary>创建 1x1 白色方形 Sprite（背景用，居中 pivot）</summary>
        private static Sprite CreateSquareSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        /// <summary>创建 1x1 白色方形 Sprite（填充用，左侧 pivot，缩放时从左向右收缩）</summary>
        private static Sprite CreateFillSprite()
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0f, 0.5f), 1f);
        }
    }
}
