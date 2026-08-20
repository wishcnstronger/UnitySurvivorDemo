using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 开始界面（挂在独立 GameObject 上，不挂 Player）。
    /// AI 背景图面板 + 金色标题 + 副标题 + AI 按钮背景图 Start 按钮。
    /// Canvas sortingOrder = 120（最高层）；点 Start 后必须隐藏自身 Canvas。
    /// </summary>
    public class StartScreenUI : MonoBehaviour
    {
        private GameObject canvasObject;
        public LevelUpManager levelUp;

        private void Start()
        {
            CreateUI();
        }

        private void OnStartClicked()
        {
            if (canvasObject != null)
                canvasObject.SetActive(false);

            if (levelUp != null)
                levelUp.ShowInitialChoice();
            else
                Time.timeScale = 1f;
        }

        private void CreateUI()
        {
            // 1. Canvas
            canvasObject = new GameObject("StartCanvas");
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 120;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObject.AddComponent<GraphicRaycaster>();

            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            Font font = UIFont.Get();

            // 2. 全屏遮罩
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasObject.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = UIDungeonTheme.OverlayBg;
            overlayImage.raycastTarget = true;

            // 3. AI 背景图面板（含金色边框 + 石质底纹 + 角饰）
            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(800f, 400f);

            Image panelImage = panel.AddComponent<Image>();
            Sprite panelBg = UIArtCache.GetPanelBg(UIArtCache.PanelType.StartScreen);
            if (panelBg != null)
            {
                panelImage.sprite = panelBg;
                panelImage.type = Image.Type.Simple;
                panelImage.color = Color.white;
            }
            else
            {
                // 程序化兜底
                panelImage.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.12f, 0.10f, 0.16f, 0.92f), new Color(0.06f, 0.06f, 0.10f, 0.92f), 64, 4);
                panelImage.type = Image.Type.Sliced;
                panelImage.color = Color.white;
            }

            // 4. 标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(panel.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 80f);
            titleRect.sizeDelta = new Vector2(700f, 100f);
            Text title = titleObj.AddComponent<Text>();
            title.text = "地牢幸存者 Demo";
            title.font = font;
            title.fontSize = 40;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UIDungeonTheme.GoldText;
            UIDungeonTheme.AddTextEffect(title);

            // 5. 分隔线
            GameObject divider = new GameObject("Divider");
            divider.transform.SetParent(panel.transform, false);
            RectTransform divRect = divider.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 0.5f);
            divRect.anchorMax = new Vector2(0.5f, 0.5f);
            divRect.pivot = new Vector2(0.5f, 0.5f);
            divRect.anchoredPosition = new Vector2(0f, 40f);
            divRect.sizeDelta = new Vector2(500f, 3f);
            Image divImage = divider.AddComponent<Image>();
            divImage.sprite = UIDungeonTheme.CreateDividerSprite(UIDungeonTheme.GoldBorder);
            divImage.color = Color.white;
            divImage.raycastTarget = false;

            // 6. 副标题
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
            sub.fontSize = 20;
            sub.alignment = TextAnchor.MiddleCenter;
            sub.color = UIDungeonTheme.StoneText;
            UIDungeonTheme.AddShadow(sub);

            // 7. Start 按钮（AI 按钮背景图 + 金色文字 + 悬停效果）
            GameObject btnObj = new GameObject("StartButton");
            btnObj.transform.SetParent(panel.transform, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0f, -80f);
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
            button.onClick.AddListener(OnStartClicked);
            UIDungeonTheme.StyleButton(button);
            UIDungeonTheme.AddHoverScale(btnObj, 1.08f);

            // 悬停变色：使用 EventTrigger 在 hover 时给 Image 加暖色 tint
            AddHoverColor(btnObj, btnImage);

            // 按钮文字
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
            btnText.fontSize = 28;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = UIDungeonTheme.GoldText;
            btnText.raycastTarget = false;
        }

        /// <summary>悬停时给按钮 Image 叠加暖色 tint，离开时恢复白色</summary>
        private static void AddHoverColor(GameObject go, Image img)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = go.AddComponent<EventTrigger>();

            EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            enter.callback.AddListener(_ => { img.color = new Color(1.2f, 1.0f, 0.7f); });
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            exit.callback.AddListener(_ => { img.color = Color.white; });
            trigger.triggers.Add(exit);
        }
    }
}
