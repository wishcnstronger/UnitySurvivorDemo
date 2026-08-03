using UnityEngine;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家常驻 HUD（挂在 Player 上）。
    /// 左上角：HP 血条+数值 → 等级 → 经验条+数值；右上角：存活时间。
    /// 每帧轮询 PlayerStats / GameStats 刷新（不用事件）。
    /// 全部用 anchor 定位（参考分辨率 1920×1080），不用世界坐标。
    /// 血条/经验条填充：锚点 pivot 在左端，宽度按比例缩放，从右端收缩。
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

        /// <summary>玩家属性（每帧轮询）</summary>
        private PlayerStats stats;

        // HP
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

            // 填充宽度 = 条宽 × 比例（左对齐，从右端收缩）
            if (hpFillRect != null)
                hpFillRect.sizeDelta = new Vector2(barWidth * hpRatio, barHeight);

            // 变色：>50% 绿 / >25% 黄 / 否则红
            if (hpFillImage != null)
            {
                if (hpRatio > 0.5f)
                    hpFillImage.color = Color.green;
                else if (hpRatio > 0.25f)
                    hpFillImage.color = Color.yellow;
                else
                    hpFillImage.color = Color.red;
            }

            if (hpText != null)
                hpText.text = $"{Mathf.RoundToInt(stats.CurrentHP)} / {Mathf.RoundToInt(stats.MaxHP)}";

            // ---- 等级 ----
            if (levelText != null)
                levelText.text = $"等级 {stats.Level}";

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

        /// <summary>程序化创建 HUD Canvas、血条、经验条、等级和时间文字</summary>
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
            rootRect.sizeDelta = new Vector2(360f, 200f);

            Font font = UIFont.Get();

            // ---- HP 血条背景（黑 0.7，贴容器左上）----
            RectTransform hpBg = CreateBarRect(rootRect, "HPBarBG", 0f, 0f, barWidth, barHeight);
            Image hpBgImage = hpBg.gameObject.AddComponent<Image>();
            hpBgImage.color = new Color(0f, 0f, 0f, 0.7f);

            // ---- HP 血条填充（同位置，宽度按比例，变色）----
            hpFillRect = CreateBarRect(rootRect, "HPBarFill", 0f, 0f, barWidth, barHeight);
            hpFillImage = hpFillRect.gameObject.AddComponent<Image>();
            hpFillImage.color = Color.green;

            // ---- HP 数值（与血条重叠，右对齐偏右；容器宽 360 / 条宽 320，右缘取 -46 对齐条右端）----
            hpText = CreateText(rootRect, "HPValueText", font, 26, Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-46f, -18f), new Vector2(140f, 36f));

            // ---- 等级（血条下方）----
            levelText = CreateText(rootRect, "LevelText", font, 28, Color.white, TextAnchor.MiddleLeft,
                new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -48f), new Vector2(160f, 40f));

            // ---- 经验条背景（等级下方，黑 0.7）----
            RectTransform xpBg = CreateBarRect(rootRect, "XPBarBG", 0f, -96f, barWidth, xpBarHeight);
            Image xpBgImage = xpBg.gameObject.AddComponent<Image>();
            xpBgImage.color = new Color(0f, 0f, 0f, 0.7f);

            // ---- 经验条填充（青色，同背景位置）----
            xpFillRect = CreateBarRect(rootRect, "XPBarFill", 0f, -96f, barWidth, xpBarHeight);
            Image xpFillImage = xpFillRect.gameObject.AddComponent<Image>();
            xpFillImage.color = Color.cyan;

            // ---- 经验数值（经验条内右侧，右缘同样对齐条右端）----
            xpText = CreateText(rootRect, "XPValueText", font, 20, Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 0.5f), new Vector2(-46f, -105f), new Vector2(140f, xpBarHeight));

            // ---- 右上角时间（独立于左上容器，锚定屏幕右上角）----
            timeText = CreateText(canvasObj.transform, "TimeText", font, 32, Color.white, TextAnchor.MiddleRight,
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(200f, 60f));

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
    }
}
