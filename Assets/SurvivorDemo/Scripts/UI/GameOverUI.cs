using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 游戏结束界面（挂在 Player 上）。
    /// 每帧轮询玩家血量：归零 → 弹出结算面板（暂停游戏）→ 点击重新开始调用 GameSetup.ResetGame()。
    /// 和升级界面一样用 timeScale = 0 暂停，用 isShown 标志防止重复弹窗。
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        /// <summary>场景搭建者（点击重开时调用它的 ResetGame，引用由 GameSetup 注入）</summary>
        public GameSetup gameSetup;

        /// <summary>玩家属性（每帧轮询血量）</summary>
        private PlayerStats stats;

        /// <summary>升级界面（结算时隐藏，防止升级卡在游戏结束后仍可点击）</summary>
        private UpgradeUI upgradeUI;

        /// <summary>结算面板根节点（初始隐藏）</summary>
        private GameObject panel;

        /// <summary>三行统计文字</summary>
        private Text timeText;
        private Text levelText;
        private Text killsText;

        /// <summary>是否已显示（防止重复弹窗）</summary>
        private bool isShown;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
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

        /// <summary>显示结算面板：暂停游戏、填入统计、显示面板</summary>
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

            panel.SetActive(true);
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

            // 3. 居中半透明面板
            panel = new GameObject("GameOverPanel");
            panel.transform.SetParent(canvasObj.transform, false);

            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(600f, 500f);
            pRect.anchoredPosition = Vector2.zero;

            Image pImage = panel.AddComponent<Image>();
            pImage.color = new Color(0f, 0f, 0f, 0.85f); // 深色背景

            Font font = UIFont.Get();

            // 4. 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -40f);
            titleRect.sizeDelta = new Vector2(500f, 80f);

            Text title = titleObj.AddComponent<Text>();
            title.text = "游戏结束";
            title.font = font;
            title.fontSize = 60;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;

            // 5. 三行统计
            timeText = CreateStatLine(panel.transform, font, "存活时间：--:--", 0.6f);
            levelText = CreateStatLine(panel.transform, font, "等级：-", 0.45f);
            killsText = CreateStatLine(panel.transform, font, "击杀数：-", 0.3f);

            // 6. 重新开始按钮（白底黑字）
            GameObject btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(panel.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0f);
            btnRect.anchorMax = new Vector2(0.5f, 0f);
            btnRect.pivot = new Vector2(0.5f, 0f);
            btnRect.anchoredPosition = new Vector2(0f, 40f);
            btnRect.sizeDelta = new Vector2(240f, 80f);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = Color.white;

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(OnRestartClicked);

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
            btnText.fontSize = 40;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.black;

            // 初始隐藏
            panel.SetActive(false);
        }

        /// <summary>
        /// 创建一行统计文字。
        /// anchorY 指定文字在面板内的垂直位置（0 底部 ~ 1 顶部）。
        /// </summary>
        private Text CreateStatLine(Transform parent, Font font, string content, float anchorY)
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
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return text;
        }
    }
}
