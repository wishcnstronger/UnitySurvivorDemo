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
    /// 卡片背景色 = 稀有度颜色，文字两行 = 类型·稀有度 / 描述。
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        /// <summary>升级面板根节点（初始隐藏）</summary>
        private GameObject panel;

        /// <summary>3 张卡片按钮</summary>
        private List<Button> cardButtons = new List<Button>();

        /// <summary>3 张卡片的文字</summary>
        private List<Text> cardTexts = new List<Text>();

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
                SetCard(cardButtons[i], cardTexts[i], choices[i]);
            }

            panel.SetActive(true);
        }

        /// <summary>
        /// 设置单张卡片：背景色 = 稀有度颜色，文字 = 标题 + 描述，绑定点击回调。
        /// </summary>
        private void SetCard(Button button, Text text, UpgradeConfig.UpgradeDefinition def)
        {
            // 背景色 = 稀有度颜色
            button.image.color = UpgradeConfig.GetRarityColor(def.rarity);

            // 文字两行：标题（类型·稀有度）+ 描述
            string title = $"{UpgradeConfig.GetTypeName(def.type)}·{UpgradeConfig.GetRarityName(def.rarity)}";
            string desc = UpgradeConfig.GetDescription(def.type, def.rarity);
            text.text = title + "\n" + desc;

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

            // 3. 居中半透明面板
            panel = new GameObject("LevelUpPanel");
            panel.transform.SetParent(canvasObj.transform, false);

            RectTransform pRect = panel.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0.5f, 0.5f);
            pRect.anchorMax = new Vector2(0.5f, 0.5f);
            pRect.pivot = new Vector2(0.5f, 0.5f);
            pRect.sizeDelta = new Vector2(1200f, 700f);
            pRect.anchoredPosition = Vector2.zero;

            Image pImage = panel.AddComponent<Image>();
            pImage.color = new Color(0f, 0f, 0f, 0.75f); // 半透明深色

            // 4. 标题
            CreateTitle(panel.transform);

            // 5. 三张卡片
            Font font = GetDefaultFont();
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
            tRect.anchoredPosition = new Vector2(0f, -40f);
            tRect.sizeDelta = new Vector2(1000f, 80f);

            Text title = titleObj.AddComponent<Text>();
            title.text = "升级！选择一项强化";
            title.font = GetDefaultFont();
            title.fontSize = 60;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white; // 深色面板上用白色标题，保证可读
        }

        /// <summary>创建一张卡片：按钮 + 两行文字</summary>
        private void CreateCard(Transform parent, Font font, int index)
        {
            // 卡片本体（Button + Image）
            GameObject cardObj = new GameObject($"Card{index}");
            cardObj.transform.SetParent(parent, false);

            RectTransform cRect = cardObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(320f, 460f);
            cRect.anchoredPosition = new Vector2((index - 1) * 360f, 0f);

            Image img = cardObj.AddComponent<Image>();
            img.color = Color.white; // 初始白色，Show 时替换为稀有度色

            Button button = cardObj.AddComponent<Button>();
            button.targetGraphic = img;

            // 卡片文字（两行）
            GameObject textObj = new GameObject("CardText");
            textObj.transform.SetParent(cardObj.transform, false);

            RectTransform txtRect = textObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;

            Text text = textObj.AddComponent<Text>();
            text.font = font;
            text.fontSize = 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;

            // 存引用
            cardButtons.Add(button);
            cardTexts.Add(text);
        }

        /// <summary>缓存的动态字体（整个界面共用一份）</summary>
        private static Font cachedFont;

        /// <summary>
        /// 获取动态字体，确保文字渲染清晰。
        /// 优先用微软雅黑（中文系统自带、含中文字形），Arial 作为回退。
        /// 结果静态缓存，整个界面只创建一份字体，避免重复分配图集。
        /// </summary>
        private static Font GetDefaultFont()
        {
            if (cachedFont == null)
            {
                cachedFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Arial" }, 40);
            }
            return cachedFont;
        }
    }
}
