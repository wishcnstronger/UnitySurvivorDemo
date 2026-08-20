using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 升级界面（三选一卡片 + 刷新按钮）。
    /// 无稀有度系统，卡片统一金色边框。
    /// 显示：图标 + 名称 + 当前等级/最大等级 + 诅咒标签 + 描述。
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        private GameObject panel;
        private List<Button> cardButtons = new List<Button>();
        private List<Text> cardTitleTexts = new List<Text>();
        private List<Text> cardDescTexts = new List<Text>();
        private List<Image> cardIconImages = new List<Image>();
        private List<Text> cardLevelTexts = new List<Text>();
        private List<GameObject> cardCurseTags = new List<GameObject>();

        /// <summary>刷新按钮</summary>
        private Button refreshButton;
        private Text refreshText;

        /// <summary>当前选择回调</summary>
        private Action<UpgradeConfig.UpgradeDefinition> onSelect;

        /// <summary>当前显示的三张卡（刷新时需要知道当前选项）</summary>
        private List<UpgradeConfig.UpgradeDefinition> currentChoices = new List<UpgradeConfig.UpgradeDefinition>();

        private void Start()
        {
            CreateUI();
            panel.SetActive(false);
        }

        public void Show(List<UpgradeConfig.UpgradeDefinition> choices, Action<UpgradeConfig.UpgradeDefinition> onSelect)
        {
            this.onSelect = onSelect;
            currentChoices = choices;

            for (int i = 0; i < cardButtons.Count && i < choices.Count; i++)
            {
                SetCard(i, choices[i]);
            }

            panel.SetActive(true);
            if (refreshButton != null)
                refreshButton.gameObject.SetActive(true);
            if (statsPanelObj != null)
                statsPanelObj.SetActive(true);
            UpdateStatsDisplay();
        }

        /// <summary>更新左侧属性面板每行的数值</summary>
        private void UpdateStatsDisplay()
        {
            if (statRows.Count == 0) return;
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            PlayerWeapon weapon = stats != null ? stats.GetComponent<PlayerWeapon>() : null;
            if (stats == null || weapon == null) return;

            float fireRate = weapon.fireInterval > 0f ? 1f / weapon.fireInterval : 0f;

            string[] values = {
                $"{weapon.damage:0}",
                $"×{fireRate:0.0}",
                $"{weapon.critChance * 100f:0}%",
                $"{stats.MoveSpeed:0.0}",
                $"{Mathf.RoundToInt(stats.CurrentHP)}/{Mathf.RoundToInt(stats.MaxHP)}",
                $"{stats.Armor:0}",
                $"{stats.MagnetRange:0.#}",
                $"×{stats.XPRate:0.0}",
                $"{stats.curseValue}",
                $"{stats.lifestealRate * 100f:0}%",
            };

            for (int i = 0; i < statRows.Count && i < values.Length; i++)
            {
                var row = statRows[i];
                string label = row.label;
                string val = values[i];

                // 诅咒/吸血用紫色，其他用白色
                if (label == "诅咒")
                    row.text.text = $"<color=#B04CFF>{label}</color>  {val}";
                else if (label == "吸血")
                    row.text.text = $"<color=#4CFF7F>{label}</color>  {val}";
                else
                    row.text.text = $"{label}  {val}";

                // 设置图标（从 UIArtCache 获取）
                UpgradeConfig.UpgradeType iconType = UpgradeConfig.UpgradeType.Damage;
                switch (label)
                {
                    case "伤害": iconType = UpgradeConfig.UpgradeType.Damage; break;
                    case "攻速": iconType = UpgradeConfig.UpgradeType.FireRate; break;
                    case "暴击": iconType = UpgradeConfig.UpgradeType.Crit; break;
                    case "移速": iconType = UpgradeConfig.UpgradeType.MoveSpeed; break;
                    case "生命": iconType = UpgradeConfig.UpgradeType.MaxHP; break;
                    case "护甲": iconType = UpgradeConfig.UpgradeType.Armor; break;
                    case "磁铁": iconType = UpgradeConfig.UpgradeType.MagnetRange; break;
                    case "经验": iconType = UpgradeConfig.UpgradeType.XPBoost; break;
                    case "诅咒": iconType = UpgradeConfig.UpgradeType.SoulCurse; break;
                    case "吸血": iconType = UpgradeConfig.UpgradeType.Lifesteal; break;
                }
                Sprite icon = UIArtCache.GetUpgradeIcon(iconType);
                if (icon != null)
                {
                    row.icon.sprite = icon;
                    row.icon.color = label == "诅咒" ? new Color(0.69f, 0.3f, 1f) : Color.white;
                    row.icon.enabled = true;
                }
            }
        }

        /// <summary>设置刷新次数和回调</summary>
        public void SetRefreshCharges(int remaining, Action onRefresh)
        {
            if (refreshButton == null) return;

            refreshButton.onClick.RemoveAllListeners();
            refreshButton.onClick.AddListener(() => onRefresh());

            bool canRefresh = remaining > 0;
            refreshButton.interactable = canRefresh;

            if (refreshText != null)
                refreshText.text = $"刷新 ({remaining})";

            // 灰色不可点时降低透明度
            var btnImage = refreshButton.targetGraphic as Image;
            if (btnImage != null)
                btnImage.color = canRefresh ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.6f);
        }

        /// <summary>设置单张卡片内容</summary>
        private void SetCard(int index, UpgradeConfig.UpgradeDefinition def)
        {
            var button = cardButtons[index];
            var titleText = cardTitleTexts[index];
            var descText = cardDescTexts[index];
            var iconImage = cardIconImages[index];
            var levelText = cardLevelTexts[index];
            var curseTag = cardCurseTags[index];

            // 卡片背景：统一金色边框
            button.image.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.14f, 0.12f, 0.18f, 0.95f), new Color(0.08f, 0.08f, 0.12f, 0.95f), 64, 3);
            button.image.type = Image.Type.Sliced;
            button.image.color = Color.white;

            // 顶部色条：按分类取色（Stat=金/Mechanic=紫/Core=蓝/Curse=红）
            Transform strip = button.transform.Find("CategoryStrip");
            if (strip != null)
            {
                var cat = UpgradeConfig.GetCategory(def.type);
                strip.GetComponent<Image>().color = UpgradeConfig.GetCategoryColor(cat);
            }

            // 图标
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
                    iconImage.enabled = false;
            }

            // 标题
            titleText.text = UpgradeConfig.GetTypeName(def.type);

            // 等级显示
            PlayerStats stats = FindObjectOfType<PlayerStats>();
            int currentLevel = stats != null ? stats.GetPickCount(def.type) : 0;
            var data = UpgradeConfig.GetLevelData(def.type);
            if (currentLevel >= data.maxLevel)
            {
                levelText.text = "Lv.MAX";
                levelText.color = UIDungeonTheme.GoldText;
            }
            else if (currentLevel == 0)
            {
                levelText.text = "新";
                levelText.color = UIDungeonTheme.GoldText;
            }
            else
            {
                levelText.text = $"Lv.{currentLevel} → Lv.{currentLevel + 1}";
                levelText.color = UIDungeonTheme.StoneText;
            }

            // 诅咒标签
            if (curseTag != null)
                curseTag.SetActive(data.curseCost > 0);

            // 描述
            int nextLevel = Mathf.Min(currentLevel + 1, data.maxLevel);
            descText.text = UpgradeConfig.GetDescription(def.type, nextLevel);

            // 点击
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnCardClicked(def));
        }

        private void OnCardClicked(UpgradeConfig.UpgradeDefinition def)
        {
            panel.SetActive(false);
            if (refreshButton != null)
                refreshButton.gameObject.SetActive(false);
            if (statsPanelObj != null)
                statsPanelObj.SetActive(false);
            if (onSelect != null)
                onSelect(def);
        }

        public void Hide()
        {
            panel.SetActive(false);
            if (refreshButton != null)
                refreshButton.gameObject.SetActive(false);
            if (statsPanelObj != null)
                statsPanelObj.SetActive(false);
        }

        // ======== 程序化生成 UI ========

        private void CreateUI()
        {
            // 1. Canvas
            GameObject canvasObj = new GameObject("UpgradeCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            canvasObj.AddComponent<GraphicRaycaster>();

            // 2. EventSystem
            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
            }

            // 3. 全屏遮罩
            GameObject overlay = new GameObject("Overlay");
            overlay.transform.SetParent(canvasObj.transform, false);
            RectTransform overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0.02f, 0.02f, 0.04f, 0.8f);

            // 4. 面板
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
                pImage.type = Image.Type.Simple;
                pImage.color = Color.white;
            }
            else
            {
                pImage.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.12f, 0.10f, 0.16f, 0.92f), new Color(0.06f, 0.06f, 0.10f, 0.92f), 64, 4);
                pImage.type = Image.Type.Sliced;
                pImage.color = Color.white;
            }

            // 5. 标题
            CreateTitle(panel.transform);

            // 6. 三张卡片
            Font font = UIFont.Get();
            for (int i = 0; i < 3; i++)
                CreateCard(panel.transform, font, i);

            // 7. 刷新按钮（面板外右侧独立位置，挂在 Canvas 上而非面板内）
            CreateRefreshButton(canvasObj.transform, font);
            refreshButton.gameObject.SetActive(false);

            // 8. 左侧属性面板（面板外左侧，挂在 Canvas 上）
            CreateStatsPanel(canvasObj.transform, font);
        }

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

            // 分隔线
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

        private void CreateCard(Transform parent, Font font, int index)
        {
            // 卡片本体
            GameObject cardObj = new GameObject($"Card{index}");
            cardObj.transform.SetParent(parent, false);
            RectTransform cRect = cardObj.AddComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0.5f);
            cRect.anchorMax = new Vector2(0.5f, 0.5f);
            cRect.pivot = new Vector2(0.5f, 0.5f);
            cRect.sizeDelta = new Vector2(300f, 440f);
            // 整体左移给右侧刷新按钮腾空间
            cRect.anchoredPosition = new Vector2((index - 1) * 320f, -30f);

            Image img = cardObj.AddComponent<Image>();
            img.sprite = UIDungeonTheme.CreateGradientBorderSprite(UIDungeonTheme.GoldBorder, new Color(0.14f, 0.12f, 0.18f, 0.95f), new Color(0.08f, 0.08f, 0.12f, 0.95f), 64, 3);
            img.type = Image.Type.Sliced;
            img.color = Color.white;

            Button button = cardObj.AddComponent<Button>();
            button.targetGraphic = img;
            UIDungeonTheme.AddHoverScale(cardObj, 1.05f);

            // 顶部分类色条
            GameObject strip = new GameObject("CategoryStrip");
            strip.transform.SetParent(cardObj.transform, false);
            RectTransform stripRect = strip.AddComponent<RectTransform>();
            stripRect.anchorMin = new Vector2(0f, 1f);
            stripRect.anchorMax = new Vector2(1f, 1f);
            stripRect.pivot = new Vector2(0.5f, 1f);
            stripRect.anchoredPosition = new Vector2(0f, -4f);
            stripRect.sizeDelta = new Vector2(-8f, 14f);
            Image stripImage = strip.AddComponent<Image>();
            stripImage.color = UIDungeonTheme.GoldBorder;
            stripImage.raycastTarget = false;

            // 诅咒标签（右上角）
            GameObject curseObj = new GameObject("CurseTag");
            curseObj.transform.SetParent(cardObj.transform, false);
            RectTransform curseRect = curseObj.AddComponent<RectTransform>();
            curseRect.anchorMin = new Vector2(1f, 1f);
            curseRect.anchorMax = new Vector2(1f, 1f);
            curseRect.pivot = new Vector2(1f, 1f);
            curseRect.anchoredPosition = new Vector2(-6f, -22f);
            curseRect.sizeDelta = new Vector2(80f, 22f);
            Image curseBg = curseObj.AddComponent<Image>();
            curseBg.color = new Color(0.5f, 0.2f, 0.8f, 0.9f);
            curseBg.raycastTarget = false;
            Text curseText = new GameObject("CurseText").AddComponent<Text>();
            curseText.transform.SetParent(curseObj.transform, false);
            RectTransform ctRect = curseText.GetComponent<RectTransform>();
            ctRect.anchorMin = Vector2.zero;
            ctRect.anchorMax = Vector2.one;
            ctRect.offsetMin = Vector2.zero;
            ctRect.offsetMax = Vector2.zero;
            curseText.text = "诅咒";
            curseText.font = font;
            curseText.fontSize = 14;
            curseText.alignment = TextAnchor.MiddleCenter;
            curseText.color = Color.white;
            curseText.raycastTarget = false;
            curseObj.SetActive(false);

            // 图标
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
            iconImage.enabled = false;

            // 标题
            GameObject titleObj = new GameObject("CardTitle");
            titleObj.transform.SetParent(cardObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -160f);
            titleRect.sizeDelta = new Vector2(-20f, 40f);
            Text titleText = titleObj.AddComponent<Text>();
            titleText.font = font;
            titleText.fontSize = 28;
            titleText.alignment = TextAnchor.MiddleCenter;
            titleText.color = Color.white;
            titleText.raycastTarget = false;

            // 等级文字（标题下方）
            GameObject levelObj = new GameObject("CardLevel");
            levelObj.transform.SetParent(cardObj.transform, false);
            RectTransform levelRect = levelObj.AddComponent<RectTransform>();
            levelRect.anchorMin = new Vector2(0f, 1f);
            levelRect.anchorMax = new Vector2(1f, 1f);
            levelRect.pivot = new Vector2(0.5f, 1f);
            levelRect.anchoredPosition = new Vector2(0f, -205f);
            levelRect.sizeDelta = new Vector2(-20f, 24f);
            Text levelText = levelObj.AddComponent<Text>();
            levelText.font = font;
            levelText.fontSize = 18;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.color = UIDungeonTheme.StoneText;
            levelText.raycastTarget = false;

            // 分隔线
            GameObject divider = new GameObject("CardDivider");
            divider.transform.SetParent(cardObj.transform, false);
            RectTransform divRect = divider.AddComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 1f);
            divRect.anchorMax = new Vector2(0.5f, 1f);
            divRect.pivot = new Vector2(0.5f, 1f);
            divRect.anchoredPosition = new Vector2(0f, -240f);
            divRect.sizeDelta = new Vector2(200f, 2f);
            Image divImage = divider.AddComponent<Image>();
            divImage.sprite = UIDungeonTheme.CreateDividerSprite(UIDungeonTheme.Divider);
            divImage.color = Color.white;
            divImage.raycastTarget = false;

            // 描述
            GameObject descObj = new GameObject("CardDesc");
            descObj.transform.SetParent(cardObj.transform, false);
            RectTransform descRect = descObj.AddComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0f, 0f);
            descRect.anchorMax = new Vector2(1f, 0f);
            descRect.pivot = new Vector2(0.5f, 0f);
            descRect.anchoredPosition = new Vector2(0f, 30f);
            descRect.sizeDelta = new Vector2(-20f, 160f);
            Text descText = descObj.AddComponent<Text>();
            descText.font = font;
            descText.fontSize = 20;
            descText.alignment = TextAnchor.MiddleCenter;
            descText.color = UIDungeonTheme.StoneText;
            descText.horizontalOverflow = HorizontalWrapMode.Wrap;
            descText.verticalOverflow = VerticalWrapMode.Overflow;
            descText.raycastTarget = false;

            cardButtons.Add(button);
            cardTitleTexts.Add(titleText);
            cardDescTexts.Add(descText);
            cardIconImages.Add(iconImage);
            cardLevelTexts.Add(levelText);
            cardCurseTags.Add(curseObj);
        }

        /// <summary>创建刷新按钮（面板外右侧独立位置）</summary>
        private void CreateRefreshButton(Transform parent, Font font)
        {
            GameObject btnObj = new GameObject("RefreshButton");
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            // 面板右边缘 +30px 间距，垂直居中
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            // 面板宽1200/2=600 + 按钮宽160/2=80 + 间距30 = 710
            btnRect.anchoredPosition = new Vector2(710f, 0f);
            btnRect.sizeDelta = new Vector2(160f, 80f);

            Image btnImage = btnObj.AddComponent<Image>();
            Sprite btnBg = UIArtCache.ButtonBg;
            if (btnBg != null)
            {
                btnImage.sprite = btnBg;
                btnImage.type = Image.Type.Sliced;
            }
            else
            {
                btnImage.sprite = UIDungeonTheme.CreateRoundedSprite(UIDungeonTheme.BtnNormal, 64, 8f);
                btnImage.type = Image.Type.Sliced;
            }
            btnImage.color = Color.white;

            refreshButton = btnObj.AddComponent<Button>();
            refreshButton.targetGraphic = btnImage;
            UIDungeonTheme.StyleButton(refreshButton);
            UIDungeonTheme.AddHoverScale(btnObj, 1.05f);

            // 按钮文字
            GameObject txtObj = new GameObject("RefreshText");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            refreshText = txtObj.AddComponent<Text>();
            refreshText.text = "刷新 (2)";
            refreshText.font = font;
            refreshText.fontSize = 24;
            refreshText.alignment = TextAnchor.MiddleCenter;
            refreshText.color = UIDungeonTheme.GoldText;
            refreshText.raycastTarget = false;
        }

        /// <summary>创建左侧属性面板（面板外左侧独立位置，每行图标+文字）</summary>
        private void CreateStatsPanel(Transform parent, Font font)
        {
            GameObject statsObj = new GameObject("StatsPanel");
            statsObj.transform.SetParent(parent, false);
            RectTransform statsRect = statsObj.AddComponent<RectTransform>();
            statsRect.anchorMin = new Vector2(0.5f, 0.5f);
            statsRect.anchorMax = new Vector2(0.5f, 0.5f);
            statsRect.pivot = new Vector2(0.5f, 0.5f);
            statsRect.anchoredPosition = new Vector2(-710f, 0f);
            statsRect.sizeDelta = new Vector2(220f, 540f);

            // 半透明深色背景
            Image statsBg = statsObj.AddComponent<Image>();
            statsBg.sprite = UIDungeonTheme.CreateRoundedSprite(new Color(0.06f, 0.05f, 0.10f, 0.92f), 64, 8f);
            statsBg.type = Image.Type.Sliced;
            statsBg.color = Color.white;
            statsBg.raycastTarget = false;

            // 标题
            GameObject titleObj = new GameObject("StatsTitle");
            titleObj.transform.SetParent(statsObj.transform, false);
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -15f);
            titleRect.sizeDelta = new Vector2(180f, 30f);
            Text title = titleObj.AddComponent<Text>();
            title.font = font;
            title.fontSize = 22;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = UIDungeonTheme.GoldText;
            title.text = "当前属性";
            title.raycastTarget = false;
            UIDungeonTheme.AddTextEffect(title);

            // 属性行容器
            GameObject rowsObj = new GameObject("StatsRows");
            rowsObj.transform.SetParent(statsObj.transform, false);
            RectTransform rowsRect = rowsObj.AddComponent<RectTransform>();
            rowsRect.anchorMin = new Vector2(0f, 1f);
            rowsRect.anchorMax = new Vector2(1f, 1f);
            rowsRect.pivot = new Vector2(0.5f, 1f);
            rowsRect.anchoredPosition = new Vector2(0f, -55f);
            rowsRect.sizeDelta = new Vector2(0f, 470f);

            // 统一创建 11 行：图标(左) + 文字(右)
            string[] statNames = { "伤害", "攻速", "暴击", "移速", "生命", "护甲", "磁铁", "经验", "诅咒", "吸血" };
            float rowHeight = 38f;
            float startY = 0f;
            for (int i = 0; i < statNames.Length; i++)
            {
                CreateStatRow(rowsObj.transform, font, i, statNames[i], rowHeight, startY - i * rowHeight);
            }

            statsObj.SetActive(false);
            statsPanelObj = statsObj;
        }

        /// <summary>创建一行属性：图标(左) + 数值文字(右)</summary>
        private void CreateStatRow(Transform parent, Font font, int index, string label, float rowHeight, float yOffset)
        {
            GameObject row = new GameObject($"Stat_{label}");
            row.transform.SetParent(parent, false);
            RectTransform rowRect = row.AddComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.5f, 1f);
            rowRect.anchorMax = new Vector2(0.5f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, yOffset);
            rowRect.sizeDelta = new Vector2(200f, rowHeight);

            // 图标 (左侧 28x28)
            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(row.transform, false);
            RectTransform iconRect = iconObj.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(20f, 0f);
            iconRect.sizeDelta = new Vector2(28f, 28f);
            Image iconImage = iconObj.AddComponent<Image>();
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = false;
            iconImage.enabled = false;

            // 标签 + 数值文字 (右侧)
            GameObject txtObj = new GameObject("Value");
            txtObj.transform.SetParent(row.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = new Vector2(0f, 0.5f);
            txtRect.anchorMax = new Vector2(1f, 0.5f);
            txtRect.pivot = new Vector2(0f, 0.5f);
            txtRect.anchoredPosition = new Vector2(42f, 0f);
            txtRect.sizeDelta = new Vector2(-50f, rowHeight);
            Text txt = txtObj.AddComponent<Text>();
            txt.font = font;
            txt.fontSize = 18;
            txt.alignment = TextAnchor.MiddleLeft;
            txt.color = Color.white;
            txt.raycastTarget = false;
            txt.supportRichText = true;

            statRows.Add(new StatRowData { icon = iconImage, text = txt, label = label });
        }

        private struct StatRowData
        {
            public Image icon;
            public Text text;
            public string label;
        }
        private List<StatRowData> statRows = new List<StatRowData>();
        private GameObject statsPanelObj;
    }
}
