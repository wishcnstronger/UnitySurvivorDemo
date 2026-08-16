using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 升级界面（三选一卡片）。
    /// 运行时程序化生成 UGUI，不依赖手动搭建的 Canvas。
    /// 卡片：深色底 + 稀有度色顶部色条 + 稀有度色边框 + 白色文字 + 悬停放大。
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        /// <summary>升级面板根节点（初始隐藏）</summary>
        private GameObject panel;

        /// <summary>3 张卡片按钮</summary>
        private List<Button> cardButtons = new List<Button>();

        /// <summary>3 张卡片的标题文字</summary>
        private List<Text> cardTitleTexts = new List<Text>();

        /// <summary>3 张卡片的描述文字</summary>
        private List<Text> cardDescTexts = new List<Text>();

        /// <summary>3 张卡片的图标 Image</summary>
        private List<Image> cardIconImages = new List<Image>();

        /// <summary>当前选择回调（由 LevelUpManager 传入）</summary>
        private Action<UpgradeConfig.UpgradeDefinition> onSelect;

        private void Start()
        {
            CreateUI();
            panel.SetActive(false); // 初始隐藏
        }

        /// <summary>
        /// 显示升级面板。
        /// </summary>
        /// <param name="choices">三张升级卡片</param>
        /// <param name="onSelect">玩家选择后的回调</param>
        public void Show(List<UpgradeConfig.UpgradeDefinition> choices, Action<UpgradeConfig.UpgradeDefinition> onSelect)
        {
            this.onSelect = onSelect;

            // 逐张更新卡片内容
            for (int i = 0; i < cardButtons.Count && i < choices.Count; i++)
            {
                SetCard(cardButtons[i], cardTitleTexts[i], cardDescTexts[i], cardIconImages[i], choices[i]);
            }

            panel.SetActive(true);
        }

        /// <summary>
        /// 设置单张卡片：稀有度色边框 + 顶部色条 + 图标 + 标题/描述分行。
        /// </summary>
        private void SetCard(Button button, Text titleText, Text descText, Image iconImage, UpgradeConfig.UpgradeDefinition def)
        {
            Color rarityColor = UpgradeConfig.GetRarityColor(def.rarity);
            Color cardBgTop = new Color(0.14f, 0.12f, 0.18f, 0.95f);
            Color cardBgBottom = new Color(0.08f, 0.08f, 0.12f, 0.95f);

            // 卡片背景：带稀有度色边框 + 渐变填充
            button.image.sprite = UIDungeonTheme.CreateGradientBorderSprite(rarityColor, cardBgTop, cardBgBottom, 64, 3);
            button.image.type = Image.Type.Sliced;
            button.image.color = Color.white;

            // 更新顶部色条（稀有度色）
            Transform strip = button.transform.Find("RarityStrip");
            if (strip != null)
            {
                strip.GetComponent<Image>().color = rarityColor;
            }

            // 图标：从 UIArtCache 加载，未找到时隐藏
            if (iconImage != null)
            {
                Sprite icon = UIArtCache.GetUpgradeIcon(def.type);
                if (icon != null)
                {
                    iconImage.sprite = icon;
                    iconImage.color = Color.white;
                    iconImage.enabled = true;
                }
                else
                {
                    iconImage.enabled = false;
                }
            }

            // 标题（类型名 + 稀有度）
            string title = $"{UpgradeConfig.GetTypeName(def.type)}";
            string rarity = UpgradeConfig.GetRarityName(def.rarity);
            titleText.text = $"{title}\n<color=#{ColorUtility.ToHtmlStringRGBA(rarityColor)}>{rarity}</color>";
            titleText.supportRichText = true;

            // 描述
            descText.text = UpgradeConfig.GetDescription(def.type, def.rarity);

            // 重新绑定点击（先清空，防止多次 Show 时回调叠加）
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnCardClicked(def));
        }

        /// <summary>点击卡片：隐藏面板并触发选择回调</summary>
        private void OnCardClicked(UpgradeConfig.UpgradeDefinition def)
        {
            panel.SetActive(false);

            if (onSelect != null)
                onSelect(def);
        }

        /// <summary>
        /// 隐藏升级面板（GameOverUI 在结算时调用）。
        /// 防止玩家在升级暂停期间死亡后，升级卡仍可点击并恢复游戏。
        /// </summary>
        public void Hide()
        {
            panel.SetActive(false);
        }

        // ======== 程序化生成 UI ========

        /// <summary>创建 Canvas、EventSystem、面板和三张卡片</summary>
        private void CreateUI()
        {
            // 1. Canvas（Screen Space Overlay，1920×1080 参考分辨率）
            GameObject canvasObj = new GameObject("UpgradeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. EventSystem（场景里没有才创建）
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 3. 全屏半透明遮罩
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasObj.transform, false);

            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;

            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.02f, 0.02f, 0.04f, 0.8f);

            // 4. AI 背景图面板（含金色边框 + 石质底纹 + 角饰）
            panel = new GameObject("LevelUpPanel");
            panel.transform.SetParent(canvasObj.transform, false);

            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(1200f, 700f);
            pRect.anchoredPosition = Vector2.zero;

            Image pImage = panel.AddComponent<Image>();
            Sprite panelBg = UIArtCache.GetPanelBg(UIArtCache.PanelType.Upgrade);
            if (panelBg != null)
            {
                pImage.sprite = panelBg;
                pImage.type = Image.Type.Sliced;
                pImage.color = Color.white;
            }
            else
            {
                pImage.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.12f, 0.10f, 0.16f, 0.92f), new Color(0.06f, 0.06f, 0.10f, 0.92f), 64, 4);
                pImage.type = Image.Type.Sliced;
                pImage.color = Color.white;
            }

            // 5. 标题（金色 + 描边）
            CreateTitle(panel.transform);

            // 6. 三张卡片
            Font font = UIFont.Get();
            for (int i = 0; i < 3; i++)
            {
                CreateCard(panel.transform, font, i);
            }
        }

        /// <summary>创建标题文本</summary>
        private void CreateTitle(Transform parent)
        {
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(parent, false);

            RectTransform tRect = titleObj.AddComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.5f, 1f);
            tRect.anchorMax = new Vector2(0.5f, 1f);
            tRect.pivot = new Vector2(0.5f, 1f);
            tRect.anchoredPosition = new Vector2(0f, -30f);
            tRect.sizeDelta = new Vector2(1000f, 70f);

            Text title = titleObj.AddComponent<Text>();
            title.text = "升级！选择一项强化";
            title.font = UIFont.Get();
            title.fontSize = 36;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UIDungeonTheme.GoldText;
            UIDungeonTheme.AddTextEffect(title);

            // 装饰分隔线（标题下方）
            GameObject divider = new GameObject("TitleDivider");
            divider.transform.SetParent(parent, false);
            RectTransform divRect = divider.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 1f);
            divRect.anchorMax = new Vector2(0.5f, 1f);
            divRect.pivot = new Vector2(0.5f, 1f);
            divRect.anchoredPosition = new Vector2(0f, -115f);
            divRect.sizeDelta = new Vector2(800f, 2f);
            Image divImage = divider.AddComponent<Image>();
            divImage.sprite = UIDungeonTheme.CreateDividerSprite(UIDungeonTheme.Divider);
            divImage.color = Color.white;
            divImage.raycastTarget = false;
        }

        /// <summary>创建一张卡片：深色底+稀有度边框 + 顶部色条 + 图标 + 标题 + 分隔线 + 描述</summary>
        private void CreateCard(Transform parent, Font font, int index)
        {
            // 卡片本体（Button + Image，带边框 Sprite）
            GameObject cardObj = new GameObject($"Card{index}");
            cardObj.transform.SetParent(parent, false);

            RectTransform cRect = cardObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(300f, 440f);
            cRect.anchoredPosition = new Vector2((index - 1) * 340f, -30f);

            Image img = cardObj.AddComponent<Image>();
            img.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.14f, 0.12f, 0.18f, 0.95f), new Color(0.08f, 0.08f, 0.12f, 0.95f), 64, 3);
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            Button button = cardObj.AddComponent<Button>();
            button.targetGraphic = img;

            // 悬停放大效果
            UIDungeonTheme.AddHoverScale(cardObj, 1.05f);

            // 顶部稀有度色条（高 14px）
            GameObject strip = new GameObject("RarityStrip");
            strip.transform.SetParent(cardObj.transform, false);

            RectTransform stripRect = strip.AddComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0f, 1f);
            stripRect.anchorMax = new Vector2(1f, 1f);
            stripRect.pivot = new Vector2(0.5f, 1f);
            stripRect.anchoredPosition = new Vector2(0f, -4f);
            stripRect.sizeDelta = new Vector2(-8f, 14f);

            Image stripImage = strip.AddComponent<Image>();
            stripImage.color = Color.white;
            stripImage.raycastTarget = false;

            // 图标（80x80，卡片上半部分中央，色条下方）
            GameObject iconObj = new GameObject("CardIcon");
            iconObj.transform.SetParent(cardObj.transform, false);

            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 1f);
            iconRect.anchorMax = new Vector2(0.5f, 1f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(0f, -100f);
            iconRect.sizeDelta = new Vector2(80f, 80f);

            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;
            iconImage.enabled = false; // SetCard 时根据资源是否存在启用

            // 标题文字（类型名 + 稀有度，图标下方）
            GameObject titleObj = new GameObject("CardTitle");
            titleObj.transform.SetParent(cardObj.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -160f);
            titleRect.sizeDelta = new Vector2(-20f, 100f);

            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = font;
            titleText.fontSize = 28;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.horizontalOverflow = HorizontalWrapMode.Wrap;
            titleText.verticalOverflow = VerticalWrapMode.Overflow;
            titleText.raycastTarget = false;
            titleText.supportRichText = true;

            // 分隔线（标题与描述之间）
            GameObject divider = new GameObject("CardDivider");
            divider.transform.SetParent(cardObj.transform, false);
            RectTransform divRect = divider.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 1f);
            divRect.anchorMax = new Vector2(0.5f, 1f);
            divRect.pivot = new Vector2(0.5f, 1f);
            divRect.anchoredPosition = new Vector2(0f, -280f);
            divRect.sizeDelta = new Vector2(200f, 2f);
            Image divImage = divider.AddComponent<Image>();
            divImage.sprite = UIDungeonTheme.CreateDividerSprite(UIDungeonTheme.Divider);
            divImage.color = Color.white;
            divImage.raycastTarget = false;

            // 描述文字（效果说明，下方偏下）
            GameObject descObj = new GameObject("CardDesc");
            descObj.transform.SetParent(cardObj.transform, false);

            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0f);
            descRect.pivot = new Vector2(0.5f, 0f);
            descRect.anchoredPosition = new Vector2(0f, 30f);
            descRect.sizeDelta = new Vector2(-20f, 100f);

            Text descText = descObj.AddComponent<Text>();
            descText.font = font;
            descText.fontSize = 22;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.color = UIDungeonTheme.StoneText;
            descText.horizontalOverflow = HorizontalWrapMode.Wrap;
            descText.verticalOverflow = VerticalWrapMode.Overflow;
            descText.raycastTarget = false;

            // 存引用
            cardButtons.Add(button);
            cardTitleTexts.Add(titleText);
            cardDescTexts.Add(descText);
            cardIconImages.Add(iconImage);
        }
    }
}
