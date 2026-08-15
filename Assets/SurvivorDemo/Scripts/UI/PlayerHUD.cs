using UnityEngine;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家常驻 HUD（挂在 Player 上）。
    /// 左上角：HP 血条（边框+延迟扣血白条+平滑变色）→ 等级徽章 → 经验条；右上角：存活时间。
    /// 每帧轮询 PlayerStats / GameStats 刷新（不用事件）。
    /// 全部用 anchor 定位（参考分辨率 1920×1080），不用世界坐标。
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("布局")]
        /// <summary>条宽度（参考分辨率 1920×1080 下的像素）</summary>
        public float barWidth = 320f;

        /// <summary>HP 条高度（像素）</summary>
        public float barHeight = 36f;

        /// <summary>经验条高度（像素）</summary>
        public float xpBarHeight = 18f;

        /// <summary>左上角容器相对屏幕左上角的偏移（像素）</summary>
        public Vector2 rootOffset = new Vector2(30f, -30f);

        /// <summary>条边框厚度（像素，边框就是比填充大一圈的黑底）</summary>
        public float borderWidth = 3f;

        /// <summary>延迟扣血白条追进速率（指数衰减系数，越大追得越快，4 ≈ 0.4 秒内追完大半）</summary>
        public float ghostLerpSpeed = 4f;

        /// <summary>玩家属性（每帧轮询）</summary>
        private PlayerStats stats;

        // HP
        private RectTransform hpGhostRect;   // 延迟扣血白条（HP 填充下层）
        private RectTransform hpFillRect;
        private Image hpFillImage;
        private Text hpText;

        // 等级
        private Text levelText;

        // 经验
        private RectTransform xpFillRect;
        private Text xpText;

        // 时间
        private Text timeText;

        /// <summary>延迟扣血白条当前显示比例（受伤时白条停在原值慢慢追到真实血量）</summary>
        private float ghostRatio = 1f;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            // 每帧轮询刷新（属性缺失时跳过）
            if (stats == null)
                return;

            Refresh();
        }

        /// <summary>从 PlayerStats / GameStats 读取状态并刷新显示</summary>
        private void Refresh()
        {
            // ---- HP 条 + 数值 ----
            float hpRatio = Mathf.Clamp01(stats.CurrentHP / stats.MaxHP);

            // 延迟扣血白条：受伤时白条停留原值指数衰减追上；回血（治疗）时白条直接跟上
            if (hpRatio < ghostRatio)
                ghostRatio = Mathf.Max(hpRatio, Mathf.Lerp(ghostRatio, hpRatio, ghostLerpSpeed * Time.deltaTime));
            else
                ghostRatio = hpRatio;

            if (hpGhostRect != null)
                hpGhostRect.sizeDelta = new Vector2(barWidth * ghostRatio, barHeight);

            // 填充宽度 = 条宽 × 比例（左对齐，从右端收缩）
            if (hpFillRect != null)
                hpFillRect.sizeDelta = new Vector2(barWidth * hpRatio, barHeight);

            // 平滑变色：满血绿 → 半血黄 → 残血红（HSV 色相线性渐变，每帧向目标色过渡）
            if (hpFillImage != null)
            {
                Color target = Color.HSVToRGB(0.33f * hpRatio, 0.9f, 1f);
                hpFillImage.color = Color.Lerp(hpFillImage.color, target, 10f * Time.deltaTime);
            }

            if (hpText != null)
                hpText.text = $"{Mathf.RoundToInt(stats.CurrentHP)} / {Mathf.RoundToInt(stats.MaxHP)}";

            // ---- 等级徽章 ----
            if (levelText != null)
                levelText.text = $"Lv.{stats.Level}";

            // ---- 经验条 + 数值（需求值随等级递增：10 → 15 → 20）----
            float xpRatio = Mathf.Clamp01(stats.CurrentXP / stats.XPToNextLevel);
            if (xpFillRect != null)
                xpFillRect.sizeDelta = new Vector2(barWidth * xpRatio, xpBarHeight);

            if (xpText != null)
                xpText.text = $"{Mathf.RoundToInt(stats.CurrentXP)} / {Mathf.RoundToInt(stats.XPToNextLevel)}";

            // ---- 右上角时间（分:秒，秒补零）----
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(GameStats.playTime / 60f);
                int seconds = Mathf.FloorToInt(GameStats.playTime % 60f);
                timeText.text = $"{minutes}:{seconds:D2}";
            }
        }

        /// <summary>程序化创建 HUD Canvas、血条、经验条、等级徽章和时间文字</summary>
        private void CreateUI()
        {
            // 1. Canvas（Screen Space Overlay，sortingOrder 90：低于升级 100 / 结算 110 / 开始 120）
            GameObject canvasObj = new GameObject("HUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            // 2. 左上角容器（锚定屏幕左上角，pivot 在左上）
            GameObject root = new GameObject("HUDRoot");
            root.transform.SetParent(canvasObj.transform, false);

            RectTransform rootRect = root.AddComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = rootOffset;
            rootRect.sizeDelta = new Vector2(400f, 240f);

            Font font = UIFont.Get();

            // ---- HP 血条：边框（大一圈的黑底）+ 白条（延迟扣血）+ 填充（变色）----
            // 边框：barWidth+2*borderWidth 的黑底，填充缩在里面形成边框效果
            RectTransform hpBorder = CreateBarRect(rootRect, "HPBarBorder", 0f, 0f, barWidth + borderWidth * 2f, barHeight + borderWidth * 2f);
            Image hpBorderImage = hpBorder.gameObject.AddComponent<Image>();
            hpBorderImage.color = UIDungeonTheme.StoneBorder;

            // 白条：在边框内部（左移 borderWidth），白色半透明，HP 填充下层
            hpGhostRect = CreateBarRect(rootRect, "HPBarGhost", borderWidth, -borderWidth, barWidth, barHeight);
            Image hpGhostImage = hpGhostRect.gameObject.AddComponent<Image>();
            hpGhostImage.color = new Color(1f, 1f, 1f, 0.75f);

            // 填充：最上层，平滑变色
            hpFillRect = CreateBarRect(rootRect, "HPBarFill", borderWidth, -borderWidth, barWidth, barHeight);
            hpFillImage = hpFillRect.gameObject.AddComponent<Image>();
            hpFillImage.color = Color.green;

            // ---- HP 数值（与血条重叠，右对齐偏右）----
            hpText = CreateText(rootRect, "HPValueText", font, 26, Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-46f, -borderWidth - barHeight / 2f), new Vector2(140f, 36f));
            AddOutline(hpText);

            // ---- 等级徽章：金色圆形底 + Lv.N 白字（血条下方）----
            RectTransform badge = CreateBadgeRect(rootRect, "LevelBadge", new Vector2(25f, -46f - 23f), 46f);
            Image badgeImage = badge.gameObject.AddComponent<Image>();
            badgeImage.sprite = CreateCircleSprite();
            badgeImage.color = new Color(1f, 0.78f, 0.25f); // 亮金

            levelText = CreateText(rootRect, "LevelText", font, 24, Color.white, TextAnchor.MiddleCenter,
                new Vector2(0f, 1f), new Vector2(0.5f, 0.5f), new Vector2(25f, -46f - 23f), new Vector2(46f, 46f));
            AddOutline(levelText);

            // ---- 经验条：边框 + 青色填充（徽章下方）----
            float xpTop = -46f - 46f - 8f; // 徽章底部再留 8px
            RectTransform xpBorder = CreateBarRect(rootRect, "XPBarBorder", 0f, xpTop, barWidth + borderWidth * 2f, xpBarHeight + borderWidth * 2f);
            Image xpBorderImage = xpBorder.gameObject.AddComponent<Image>();
            xpBorderImage.color = UIDungeonTheme.StoneBorder;

            xpFillRect = CreateBarRect(rootRect, "XPBarFill", borderWidth, xpTop - borderWidth, barWidth, xpBarHeight);
            Image xpFillImage = xpFillRect.gameObject.AddComponent<Image>();
            xpFillImage.color = UIDungeonTheme.DungeonBlue;

            // ---- 经验数值（经验条内右侧，右缘对齐条右端）----
            xpText = CreateText(rootRect, "XPValueText", font, 20, Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-46f, xpTop - borderWidth - xpBarHeight / 2f), new Vector2(140f, xpBarHeight));
            AddOutline(xpText);

            // ---- 右上角时间（独立于左上容器，锚定屏幕右上角）----
            timeText = CreateText(canvasObj.transform, "TimeText", font, 32, UIDungeonTheme.WarmWhite, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(200f, 60f));
            AddOutline(timeText);

            timeText.text = "0:00";
        }

        /// <summary>
        /// 创建左上对齐（anchor/pivot 都在左端）的条状 RectTransform。
        /// 填充条复用此方法，宽度变化时从右端收缩。
        /// </summary>
        private static RectTransform CreateBarRect(RectTransform parent, string name, float x, float y, float w, float h)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(w, h);
            return rect;
        }

        /// <summary>创建中心锚定的圆形徽章底（pivot 居中，方便文字叠上去）</summary>
        private static RectTransform CreateBadgeRect(RectTransform parent, string name, Vector2 center, float diameter)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = center;
            rect.sizeDelta = new Vector2(diameter, diameter);
            return rect;
        }

        /// <summary>创建 Text 子物体（anchor 与 pivot 由调用方指定）</summary>
        private static Text CreateText(Transform parent, string name, Font font, int size, Color color, TextAnchor align,
            Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);

            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;

            Text text = go.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = align;
            text.color = color;
            return text;
        }

        /// <summary>给文字加黑色描边，保证在战场背景下可读</summary>
        private static void AddOutline(Text text)
        {
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(2f, -2f);
        }

        /// <summary>程序化创建白色圆形贴图（UI Image 用它显示徽章圆形底）</summary>
        private static Sprite CreateCircleSprite()
        {
            const int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= center - 1f ? Color.white : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
