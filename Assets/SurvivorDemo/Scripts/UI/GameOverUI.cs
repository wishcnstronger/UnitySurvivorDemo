using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 游戏结束界面（挂在 Player 上）。
    /// 每帧轮询玩家血量：归零 → 弹出结算面板（暂停游戏 + 弹出动画）→ 点击重新开始调用 GameSetup.ResetGame()。
    /// 和升级界面一样用 timeScale = 0 暂停，用 isShown 标志防止重复弹窗。
    /// 视觉：金色边框 + 深红标题条 + 分级配色统计 + 悬停变色按钮。
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        /// <summary>场景搭建者（点击重开时调用它的 ResetGame，引用由 GameSetup 注入）</summary>
        public GameSetup gameSetup;

        /// <summary>玩家属性（每帧轮询血量）</summary>
        private PlayerStats stats;

        /// <summary>玩家武器（结算构筑属性展示用）</summary>
        private PlayerWeapon weapon;

        /// <summary>升级界面（结算时隐藏，防止升级卡在游戏结束后仍可点击）</summary>
        private UpgradeUI upgradeUI;

        /// <summary>结算面板根节点（初始隐藏）</summary>
        private GameObject panel;

        /// <summary>统计文字</summary>
        private Text timeText;
        private Text levelText;
        private Text killsText;

        /// <summary>构筑属性小字（伤害/攻速/子弹/穿透/暴击/经验，纯展示）</summary>
        private Text buildText;

        /// <summary>是否已显示（防止重复弹窗）</summary>
        private bool isShown;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            weapon = GetComponent<PlayerWeapon>();
            upgradeUI = GetComponent<UpgradeUI>();
        }

        private void Start()
        {
            CreateUI();
        }

        private void Update()
        {
            // 属性缺失 / 已显示 → 跳过
            if (stats == null || isShown)
                return;

            // 血量归零 → 显示结算
            if (stats.CurrentHP <= 0f)
            {
                Show();
            }
        }

        /// <summary>显示结算面板：暂停游戏、填入统计、播放弹出动画</summary>
        private void Show()
        {
            // 防止重复弹窗
            isShown = true;

            // 关闭升级面板：若玩家在升级暂停期间死亡，不能让升级卡在结算后仍可点击恢复游戏
            if (upgradeUI != null)
                upgradeUI.Hide();

            // 暂停游戏（和升级界面一样的暂停方式，timeScale=0 其他脚本天然静止）
            Time.timeScale = 0f;

            // 存活时间：直接读 GameStats.playTime（GameTimer 累加，死亡时 timeScale=0 自然冻结）。
            // 不能用 timeSinceLevelLoad 差值：开始界面/升级暂停期间它仍在走，会多算等待时间。
            float survivalTime = GameStats.playTime;
            int minutes = Mathf.FloorToInt(survivalTime / 60f);
            int seconds = Mathf.FloorToInt(survivalTime % 60f);

            timeText.text = $"存活时间：{minutes}:{seconds:D2}";
            levelText.text = $"等级：{stats.Level}";
            killsText.text = $"击杀数：{GameStats.kills}";
            buildText.text = BuildSummaryText();

            // 外框与面板同显
            panel.SetActive(true);
            StartCoroutine(ScaleIn());
        }

        /// <summary>弹出动画：0.85 → 1.0 缓动放大（0.2 秒，ease-out cubic）</summary>
        private IEnumerator ScaleIn()
        {
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                t = 1f - Mathf.Pow(1f - t, 3f); // ease-out cubic：先快后慢
                panel.transform.localScale = new Vector3(0.85f + 0.15f * t, 0.85f + 0.15f * t, 1f);
                yield return null;
            }

            panel.transform.localScale = Vector3.one;
        }

        /// <summary>点击重新开始：调用 GameSetup.ResetGame() 清空并重建整个游戏</summary>
        private void OnRestartClicked()
        {
            // 引用缺失时直接返回（正常流程 GameSetup 会注入）
            if (gameSetup == null)
            {
                Debug.LogError("GameOverUI 缺少 GameSetup 引用！");
                return;
            }

            gameSetup.ResetGame();
        }

        // ======== 程序化生成 UI ========

        /// <summary>创建 Canvas、结算面板、统计文字和重新开始按钮</summary>
        private void CreateUI()
        {
            // 1. Canvas（Screen Space Overlay，最高层 110）
            GameObject canvasObj = new GameObject("GameOverCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 110;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. EventSystem（场景里没有才创建，重开时保留复用）
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 3. AI 背景图面板（含金色边框 + 石质底纹 + 角饰）
            panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(canvasObj.transform, false);

            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(600f, 650f);
            pRect.anchoredPosition = Vector2.zero;

            Image pImage = panel.AddComponent<Image>();
            Sprite panelBg = UIArtCache.GetPanelBg(UIArtCache.PanelType.GameOver);
            if (panelBg != null)
            {
                pImage.sprite = panelBg;
                pImage.type = Image.Type.Simple;
                pImage.color = Color.white;
            }
            else
            {
                pImage.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.10f, 0.08f, 0.14f, 0.95f), new Color(0.05f, 0.05f, 0.09f, 0.95f), 64, 4);
                pImage.type = Image.Type.Sliced;
                pImage.color = Color.white;
            }

            Font font = UIFont.Get();

            // 5. 标题（面板顶部，浅红大字，直接放在 AI 背景上）
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -30f);
            titleRect.sizeDelta = new Vector2(500f, 70f);
            Text title = titleObj.AddComponent<Text>();
            title.text = "游戏结束";
            title.font = font;
            title.fontSize = 36;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = new Color(1f, 0.35f, 0.35f);
            AddOutline(title);

            // 6. 三行统计（标题下方，分级配色）
            timeText = CreateStatLine(panel.transform, font, "存活时间：--:--", 0.62f, Color.white);
            levelText = CreateStatLine(panel.transform, font, "等级：-", 0.50f, new Color(0.5f, 0.9f, 1f));
            killsText = CreateStatLine(panel.transform, font, "击杀数：-", 0.38f, new Color(1f, 0.85f, 0.3f));

            // 7. 构筑属性小字
            buildText = CreateBuildText(panel.transform, font);

            // 9. 重新开始按钮（AI 按钮背景图 + 金色文字 + 悬停效果）
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(panel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0f, 50f);
            btnRect.sizeDelta = new Vector2(240f, 80f);

            Image btnImage = btnObj.AddComponent<Image>();
            Sprite btnBg = UIArtCache.ButtonBg;
            if (btnBg != null)
            {
                btnImage.sprite = btnBg;
                btnImage.type = Image.Type.Sliced;
                btnImage.color = Color.white;
            }
            else
            {
                btnImage.color = UIDungeonTheme.BtnNormal;
            }

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(OnRestartClicked);
            UIDungeonTheme.StyleButton(button);
            UIDungeonTheme.AddHoverScale(btnObj, 1.06f);

            // 按钮文字
            GameObject btnTextObj = new GameObject("RestartText");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTxtRect = btnTextObj.AddComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.offsetMin = Vector2.zero;
            btnTxtRect.offsetMax = Vector2.zero;

            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "重新开始";
            btnText.font = font;
            btnText.fontSize = 28;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = UIDungeonTheme.GoldText;
            btnText.raycastTarget = false;

            // 初始隐藏
            panel.SetActive(false);
        }

        /// <summary>创建一行统计文字。</summary>
        private Text CreateStatLine(Transform parent, Font font, string content, float anchorY, Color color)
        {
            GameObject textObj = new GameObject("StatText");
            textObj.transform.SetParent(parent, false);

            RectTransform txtRect = textObj.AddComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0.5f, anchorY);
            txtRect.anchorMax = new Vector2(0.5f, anchorY);
            txtRect.pivot = new Vector2(0.5f, 0.5f);
            txtRect.anchoredPosition = Vector2.zero;
            txtRect.sizeDelta = new Vector2(500f, 60f);

            Text text = textObj.AddComponent<Text>();
            text.text = content;
            text.font = font;
            text.fontSize = 24;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            AddOutline(text);

            return text;
        }

        /// <summary>创建构筑属性小字（击杀数下方，白 24 号，允许换行）</summary>
        private Text CreateBuildText(Transform parent, Font font)
        {
            GameObject textObj = new GameObject("BuildText");
            textObj.transform.SetParent(parent, false);

            RectTransform txtRect = textObj.AddComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0.5f, 0.2f);
            txtRect.anchorMax = new Vector2(0.5f, 0.2f);
            txtRect.pivot = new Vector2(0.5f, 0.5f);
            txtRect.anchoredPosition = Vector2.zero;
            txtRect.sizeDelta = new Vector2(540f, 60f);

            Text text = textObj.AddComponent<Text>();
            text.text = "";
            text.font = font;
            text.fontSize = 18;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            AddOutline(text);

            return text;
        }

        /// <summary>给文字加描边+投影组合效果，保证在深色背景上可读</summary>
        private static void AddOutline(Text text)
        {
            UIDungeonTheme.AddTextEffect(text);
        }

        /// <summary>拼装本局最终强化属性字符串</summary>
        private string BuildSummaryText()
        {
            if (weapon == null || stats == null)
                return "";

            // 攻速用 1/攻击间隔 表示每秒攻击次数
            float fireRate = weapon.fireInterval > 0f ? 1f / weapon.fireInterval : 0f;
            return $"伤害 {weapon.damage:0} · 攻速 ×{fireRate:0.0} · 暴击 {weapon.critChance * 100f:0}% · 经验 ×{stats.XPRate:0.0} · 诅咒 {stats.curseValue} · 吸血 {stats.lifestealRate * 100f:0}%";
        }
    }
}
