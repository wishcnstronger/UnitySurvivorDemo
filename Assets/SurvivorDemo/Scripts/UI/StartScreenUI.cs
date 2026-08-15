using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 开始界面（挂在独立 GameObject 上，不挂 Player）。
    /// 程序化生成：半透明背景面板 + 金色标题 + 副标题 + 地牢风格 Start 按钮。
    /// GameSetup.Awake 把 timeScale 设为 0 停在开始界面，点 Start 后才开始游戏。
    /// Canvas sortingOrder = 120（最高层）；点 Start 后必须隐藏自身 Canvas，
    /// 否则会永久盖住左上角 HUD（sortingOrder 90）。
    /// </summary>
    public class StartScreenUI : MonoBehaviour
    {
        /// <summary>开始界面 Canvas（点 Start 后隐藏）</summary>
        private GameObject canvasObject;

        /// <summary>升级流程（开局初始三选一用，由 GameSetup 注入）</summary>
        public LevelUpManager levelUp;

        private void Start()
        {
            CreateUI();
        }

        /// <summary>点击开始按钮：先隐藏开始界面，再弹出初始构筑三选一（选完才正式开始）</summary>
        private void OnStartClicked()
        {
            // 先隐藏自身 Canvas：sortingOrder=120 最高层，不隐藏会永久盖住左上角 HUD
            if (canvasObject != null)
                canvasObject.SetActive(false);

            // 再弹出初始三选一（复用升级面板与 OnChoiceSelected 回调，选完内部恢复 timeScale=1）
            if (levelUp != null)
            {
                levelUp.ShowInitialChoice();
            }
            else
            {
                // 引用缺失 → 直接恢复时间流速兜底，宁可跳过初始选择也不卡死
                Time.timeScale = 1f;
            }
        }

        /// <summary>程序化创建 Canvas、背景面板、标题、副标题和 Start 按钮</summary>
        private void CreateUI()
        {
            // 1. Canvas（Screen Space Overlay，最高层 120）
            canvasObject = new GameObject("StartCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();

            // 2. EventSystem（场景里没有才创建，重开时保留复用）
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            Font font = UIFont.Get();

            // 3. 全屏半透明遮罩（深色，营造地牢氛围）
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasObject.transform, false);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = UIDungeonTheme.OverlayBg;
            overlayImage.raycastTarget = true; // 拦截背景点击

            // 4. 中央面板（圆角深色底 + 金色边框）
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform, false);

            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(800f, 400f);

            Image panelImage = panel.AddComponent<Image>();
            panelImage.sprite = UIDungeonTheme.CreateBorderSprite(UIDungeonTheme.GoldBorder, UIDungeonTheme.PanelBg, 64, 4);
            panelImage.type = Image.Type.Sliced;
            panelImage.color = Color.white;

            // 5. 大标题（金色 + 描边）
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 80f);
            titleRect.sizeDelta = new Vector2(700f, 100f);

            Text title = titleObj.AddComponent<Text>();
            title.text = "幸存者 Demo";
            title.font = font;
            title.fontSize = 64;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UIDungeonTheme.GoldText;
            UIDungeonTheme.AddOutline(title);

            // 6. 副标题（石灰色 + 描边）
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(panel.transform, false);

            RectTransform subRect = subObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.pivot = new Vector2(0.5f, 0.5f);
            subRect.anchoredPosition = new Vector2(0f, 10f);
            subRect.sizeDelta = new Vector2(700f, 50f);

            Text sub = subObj.AddComponent<Text>();
            sub.text = "WASD 移动 · 自动攻击";
            sub.font = font;
            sub.fontSize = 28;
            sub.alignment = TextAnchor.MiddleCenter;
            sub.color = UIDungeonTheme.StoneText;
            UIDungeonTheme.AddOutline(sub);

            // 7. Start 按钮（深棕底 + 金色边框 + 金色文字 + 悬停效果）
            // 外框（金色边框，比按钮大一圈）
            GameObject btnBorder = new GameObject("StartButtonBorder");
            btnBorder.transform.SetParent(panel.transform, false);

            RectTransform btnBorderRect = btnBorder.AddComponent<RectTransform>();
            btnBorderRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnBorderRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnBorderRect.pivot = new Vector2(0.5f, 0.5f);
            btnBorderRect.anchoredPosition = new Vector2(0f, -80f);
            btnBorderRect.sizeDelta = new Vector2(224f, 74f);

            Image btnBorderImage = btnBorder.AddComponent<Image>();
            btnBorderImage.color = UIDungeonTheme.GoldBorder;
            btnBorderImage.raycastTarget = false;

            // 按钮本体
            GameObject btnObj = new GameObject("StartButton");
            btnObj.transform.SetParent(btnBorder.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = Vector2.zero;
            btnRect.sizeDelta = new Vector2(220f, 70f);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = UIDungeonTheme.BtnNormal;

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(OnStartClicked);
            UIDungeonTheme.StyleButton(button);
            UIDungeonTheme.AddHoverScale(btnObj, 1.06f);

            // 按钮文字（金色）
            GameObject btnTextObj = new GameObject("StartText");
            btnTextObj.transform.SetParent(btnObj.transform, false);

            RectTransform btnTxtRect = btnTextObj.AddComponent<RectTransform>();
            btnTxtRect.anchorMin = Vector2.zero;
            btnTxtRect.anchorMax = Vector2.one;
            btnTxtRect.offsetMin = Vector2.zero;
            btnTxtRect.offsetMax = Vector2.zero;

            Text btnText = btnTextObj.AddComponent<Text>();
            btnText.text = "Start";
            btnText.font = font;
            btnText.fontSize = 36;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = UIDungeonTheme.GoldText;
            btnText.raycastTarget = false;
        }
    }
}
