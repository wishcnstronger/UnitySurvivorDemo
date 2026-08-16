using UnityEngine;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 玩家常驻 HUD（挂在 Player 上）。
    /// VS 风格布局：顶部居中时间 + 顶部全宽经验条 + 经验条左侧等级 + 右上角击杀计数 + 角色头顶世界空间 HP 条。
    /// 每帧轮询 PlayerStats / GameStats 刷新。
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [Header("屏幕空间布局")]
        /// <summary>经验条高度（像素）</summary>
        public float xpBarHeight = 22f;

        /// <summary>条边框厚度（像素）</summary>
        public float borderWidth = 3f;

        [Header("世界空间 HP 条")]
        /// <summary>HP 条宽度（世界单位）</summary>
        public float hpBarWidth = 1.5f;

        /// <summary>HP 条高度（世界单位）</summary>
        public float hpBarHeight = 0.18f;

        /// <summary>HP 条边框厚度（世界单位）</summary>
        public float hpBorderWidth = 0.03f;

        /// <summary>HP 条距玩家头顶高度（世界单位）</summary>
        public float hpBarOffsetY = 1.2f;

        /// <summary>延迟扣血白条追进速率</summary>
        public float ghostLerpSpeed = 4f;

        /// <summary>玩家属性（每帧轮询）</summary>
        private PlayerStats stats;

        // 经验
        private RectTransform xpFillRect;
        private Image xpFillImage;

        // 等级
        private Text levelText;

        // 时间
        private Text timeText;

        // 击杀
        private Text killsText;

        // 世界空间 HP
        private RectTransform hpGhostRect;
        private RectTransform hpFillRect;
        private Image hpFillImage;

        /// <summary>延迟扣血白条当前显示比例</summary>
        private float ghostRatio = 1f;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
        }

        private void Start()
        {
            CreateTopBarUI();
            CreateWorldSpaceHPBar();
        }

        private void Update()
        {
            if (stats == null)
                return;

            Refresh();
        }

        /// <summary>从 PlayerStats / GameStats 读取状态并刷新显示</summary>
        private void Refresh()
        {
            // ---- 经验条 ----
            float xpRatio = Mathf.Clamp01(stats.CurrentXP / stats.XPToNextLevel);
            if (xpFillRect != null)
                xpFillRect.anchorMax = new Vector2(xpRatio, 1f);

            // 经验条满级变色（蓝 → 金）
            if (xpFillImage != null)
            {
                bool maxLevel = stats.XPToNextLevel <= 0f || stats.Level >= 99;
                Color xpTarget = maxLevel ? UIDungeonTheme.GoldText : UIDungeonTheme.DungeonBlue;
                xpFillImage.color = Color.Lerp(xpFillImage.color, xpTarget, 5f * Time.deltaTime);
            }

            // ---- 等级 ----
            if (levelText != null)
                levelText.text = $"Lv.{stats.Level}";

            // ---- 时间 ----
            if (timeText != null)
            {
                int minutes = Mathf.FloorToInt(GameStats.playTime / 60f);
                int seconds = Mathf.FloorToInt(GameStats.playTime % 60f);
                timeText.text = $"{minutes}:{seconds:D2}";
            }

            // ---- 击杀数 ----
            if (killsText != null)
                killsText.text = GameStats.kills.ToString();

            // ---- 世界空间 HP 条 ----
            float hpRatio = Mathf.Clamp01(stats.CurrentHP / stats.MaxHP);

            if (hpRatio < ghostRatio)
                ghostRatio = Mathf.Max(hpRatio, Mathf.Lerp(ghostRatio, hpRatio, ghostLerpSpeed * Time.deltaTime));
            else
                ghostRatio = hpRatio;

            if (hpGhostRect != null)
                hpGhostRect.anchorMax = new Vector2(ghostRatio, 1f);

            if (hpFillRect != null)
                hpFillRect.anchorMax = new Vector2(hpRatio, 1f);

            if (hpFillImage != null)
            {
                Color target = Color.HSVToRGB(0.33f * hpRatio, 0.9f, 1f);
                hpFillImage.color = Color.Lerp(hpFillImage.color, target, 10f * Time.deltaTime);
            }
        }

        // ==================== 屏幕空间顶部信息栏 ====================

        /// <summary>创建顶部信息栏 Canvas（时间 + 经验条 + 等级 + 击杀数）</summary>
        private void CreateTopBarUI()
        {
            GameObject canvasObj = new GameObject("HUDCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Font font = UIFont.Get();

            // ---- A. 顶部居中时间 ----
            GameObject timeBg = new GameObject("TimeBg");
            timeBg.transform.SetParent(canvasObj.transform, false);
            RectTransform tBgRect = timeBg.AddComponent<RectTransform>();
            tBgRect.anchorMin = new Vector2(0.5f, 1f);
            tBgRect.anchorMax = new Vector2(0.5f, 1f);
            tBgRect.pivot = new Vector2(0.5f, 1f);
            tBgRect.anchoredPosition = new Vector2(0f, -12f);
            tBgRect.sizeDelta = new Vector2(200f, 44f);
            Image tBgImage = timeBg.AddComponent<Image>();
            tBgImage.sprite = UIDungeonTheme.CreateRoundedSprite(UIDungeonTheme.HudBg, 64, 10f);
            tBgImage.type = Image.Type.Sliced;
            tBgImage.color = Color.white;

            timeText = CreateText(timeBg.transform, "TimeText", font, 32, UIDungeonTheme.WarmWhite, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(200f, 44f));
            AddOutline(timeText);
            timeText.text = "0:00";

            // ---- B. 顶部全宽经验条（90% 屏宽居中）----
            GameObject xpBorder = new GameObject("XPBarBorder");
            xpBorder.transform.SetParent(canvasObj.transform, false);
            RectTransform xpBorderRect = xpBorder.AddComponent<RectTransform>();
            xpBorderRect.anchorMin = new Vector2(0.05f, 1f);
            xpBorderRect.anchorMax = new Vector2(0.95f, 1f);
            xpBorderRect.pivot = new Vector2(0.5f, 1f);
            xpBorderRect.anchoredPosition = new Vector2(0f, -56f);
            xpBorderRect.sizeDelta = new Vector2(0f, xpBarHeight + borderWidth * 2f);
            Image xpBorderImage = xpBorder.AddComponent<Image>();
            xpBorderImage.color = UIDungeonTheme.StoneBorder;

            // 经验条填充（anchor 左→右扩展）
            xpFillRect = CreateAnchoredFill(xpBorder.transform, "XPBarFill", borderWidth);
            xpFillImage = xpFillRect.gameObject.AddComponent<Image>();
            xpFillImage.color = UIDungeonTheme.DungeonBlue;

            // 1px 高光线
            CreateHighlight(xpBorder.transform);

            // ---- C. 等级文字（经验条左侧）----
            levelText = CreateText(canvasObj.transform, "LevelText", font, 24, UIDungeonTheme.GoldText, TextAnchor.MiddleRight,
                new Vector2(0.05f, 1f), new Vector2(1f, 0.5f), new Vector2(-8f, -67f), new Vector2(80f, 44f));
            AddOutline(levelText);
            levelText.text = "Lv.1";

            // ---- D. 右上角击杀计数（骷髅图标 + 数字）----
            GameObject killBg = new GameObject("KillBg");
            killBg.transform.SetParent(canvasObj.transform, false);
            RectTransform kBgRect = killBg.AddComponent<RectTransform>();
            kBgRect.anchorMin = new Vector2(1f, 1f);
            kBgRect.anchorMax = new Vector2(1f, 1f);
            kBgRect.pivot = new Vector2(1f, 1f);
            kBgRect.anchoredPosition = new Vector2(-16f, -64f);
            kBgRect.sizeDelta = new Vector2(160f, 44f);
            Image kBgImage = killBg.AddComponent<Image>();
            kBgImage.sprite = UIDungeonTheme.CreateRoundedSprite(UIDungeonTheme.HudBg, 64, 10f);
            kBgImage.type = Image.Type.Sliced;
            kBgImage.color = Color.white;

            // 骷髅图标
            float skullSize = 28f;
            RectTransform skullRect = CreateAnchoredElement(killBg.transform, "SkullIcon",
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, 0f), new Vector2(skullSize, skullSize));
            Image skullImage = skullRect.gameObject.AddComponent<Image>();
            skullImage.preserveAspect = true;
            skullImage.raycastTarget = false;
            Sprite skullSprite = UIArtCache.SkullIcon;
            if (skullSprite != null)
            {
                skullImage.sprite = skullSprite;
                skullImage.color = Color.white;
            }
            else
            {
                skullImage.sprite = CreateSkullSprite();
                skullImage.color = new Color(0.9f, 0.9f, 0.9f);
            }

            // 击杀数字
            killsText = CreateText(killBg.transform, "KillsText", font, 28, UIDungeonTheme.WarmWhite, TextAnchor.MiddleLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f + skullSize + 4f, 0f), new Vector2(110f, 44f));
            AddOutline(killsText);
            killsText.text = "0";
        }

        // ==================== 世界空间 HP 条 ====================

        /// <summary>创建角色头顶世界空间 HP 条（挂在 Player 下，自动跟随移动）</summary>
        private void CreateWorldSpaceHPBar()
        {
            GameObject hpCanvasObj = new GameObject("HPBarCanvas");
            hpCanvasObj.transform.SetParent(transform, false);

            Canvas hpCanvas = hpCanvasObj.AddComponent<Canvas>();
            hpCanvas.renderMode = RenderMode.WorldSpace;
            hpCanvas.sortingOrder = 50;

            RectTransform hpCanvasRect = hpCanvasObj.GetComponent<RectTransform>();
            hpCanvasRect.localPosition = new Vector3(0f, hpBarOffsetY, 0f);
            float totalW = hpBarWidth + hpBorderWidth * 2f;
            float totalH = hpBarHeight + hpBorderWidth * 2f;
            hpCanvasRect.sizeDelta = new Vector2(totalW, totalH);
            hpCanvasRect.localScale = Vector3.one;

            // 边框
            GameObject border = new GameObject("HPBorder");
            border.transform.SetParent(hpCanvasObj.transform, false);
            RectTransform borderRect = border.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            Image borderImg = border.AddComponent<Image>();
            borderImg.color = UIDungeonTheme.StoneBorder;

            // 延迟白条（anchor 左→右扩展）
            hpGhostRect = CreateAnchoredFillWorld(border.transform, "HPGhost", hpBorderWidth);
            Image ghostImg = hpGhostRect.gameObject.AddComponent<Image>();
            ghostImg.color = new Color(1f, 1f, 1f, 0.6f);

            // HP 填充
            hpFillRect = CreateAnchoredFillWorld(border.transform, "HPFill", hpBorderWidth);
            hpFillImage = hpFillRect.gameObject.AddComponent<Image>();
            hpFillImage.color = Color.green;
        }

        // ==================== 工具方法 ====================

        /// <summary>创建锚定填充 RectTransform（anchor 左对齐，通过 anchorMax.x 控制宽度）</summary>
        private static RectTransform CreateAnchoredFill(Transform parent, string name, float inset)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.offsetMin = new Vector2(inset, inset);
            rect.offsetMax = new Vector2(0f, -inset);
            return rect;
        }

        /// <summary>世界空间锚定填充（与屏幕空间版相同逻辑，世界单位 inset）</summary>
        private static RectTransform CreateAnchoredFillWorld(Transform parent, string name, float inset)
        {
            return CreateAnchoredFill(parent, name, inset);
        }

        /// <summary>创建锚定元素（pivot/anchor/pos/size 由调用方指定）</summary>
        private static RectTransform CreateAnchoredElement(Transform parent, string name,
            Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            RectTransform rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>创建 Text 子物体</summary>
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

        /// <summary>给文字加描边+投影组合效果</summary>
        private static void AddOutline(Text text)
        {
            UIDungeonTheme.AddTextEffect(text);
        }

        /// <summary>在条顶部添加 1px 高光线</summary>
        private static void CreateHighlight(Transform parent)
        {
            GameObject hl = new GameObject("Highlight");
            hl.transform.SetParent(parent, false);
            RectTransform rect = hl.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -1f);
            rect.sizeDelta = new Vector2(0f, 1f);
            Image img = hl.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);
            img.raycastTarget = false;
        }

        /// <summary>程序化创建骷髅贴图（AI 图标未加载时的兜底）</summary>
        private static Sprite CreateSkullSprite()
        {
            const int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color w = new Color(0.9f, 0.9f, 0.9f, 1f);
            Color g = new Color(0.5f, 0.5f, 0.5f, 1f);
            Color d = new Color(0.2f, 0.2f, 0.2f, 1f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool filled = false;
                    Color c = w;

                    // 头骨圆形（上半部分）
                    int cy = y - 18;
                    int cx = x - 16;
                    float dist = Mathf.Sqrt(cx * cx + cy * cy);
                    if (y >= 12 && y <= 24 && dist <= 9f)
                    {
                        filled = true;
                        // 眼眶
                        if (dist >= 3f && dist <= 5f && (cx < -2 || cx > 2) && cy < 2)
                            c = d;
                    }

                    // 下颌
                    if (y >= 8 && y <= 12 && x >= 11 && x <= 21)
                    {
                        filled = true;
                        // 牙缝
                        if (x == 14 || x == 16 || x == 18)
                            c = g;
                    }

                    tex.SetPixel(x, y, filled ? c : Color.clear);
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
