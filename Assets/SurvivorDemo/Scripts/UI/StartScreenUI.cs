using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 开始界面（挂在独立 GameObject 上，不挂 Player）。
    /// 程序化生成：大标题 + 副标题 + Start 按钮。
    /// GameSetup.Awake 把 timeScale 设为 0 停在开始界面，点 Start 后才开始游戏。
    /// Canvas sortingOrder = 120（最高层）；点 Start 后必须隐藏自身 Canvas，
    /// 否则会永久盖住左上角 HUD（sortingOrder 90）。
    /// </summary>
    public class StartScreenUI : MonoBehaviour
    {
        /// <summary>开始界面 Canvas（点 Start 后隐藏）</summary>
        private GameObject canvasObject;

        private void Start()
        {
            CreateUI();
        }

        /// <summary>点击开始按钮：恢复时间流速并隐藏开始界面</summary>
        private void OnStartClicked()
        {
            // 恢复游戏（GameSetup.Awake 里 timeScale 被设为 0 暂停在开始界面）
            Time.timeScale = 1f;

            // 隐藏自身 Canvas：sortingOrder=120 最高层，不隐藏会永久盖住左上角 HUD
            if (canvasObject != null)
                canvasObject.SetActive(false);
        }

        /// <summary>程序化创建 Canvas、标题、副标题和 Start 按钮</summary>
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

            // 3. 大标题
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(canvasObject.transform, false);

            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.5f);
            titleRect.anchorMax = new Vector2(0.5f, 0.5f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = new Vector2(0f, 150f);
            titleRect.sizeDelta = new Vector2(900f, 140f);

            Text title = titleObj.AddComponent<Text>();
            title.text = "幸存者 Demo";
            title.font = font;
            title.fontSize = 72;
            title.alignment = TextAnchor.MiddleCenter;
            title.color = Color.white;

            // 4. 副标题（操作提示）
            GameObject subObj = new GameObject("Subtitle");
            subObj.transform.SetParent(canvasObject.transform, false);

            RectTransform subRect = subObj.AddComponent<RectTransform>();
            subRect.anchorMin = new Vector2(0.5f, 0.5f);
            subRect.anchorMax = new Vector2(0.5f, 0.5f);
            subRect.pivot = new Vector2(0.5f, 0.5f);
            subRect.anchoredPosition = new Vector2(0f, 70f);
            subRect.sizeDelta = new Vector2(900f, 60f);

            Text sub = subObj.AddComponent<Text>();
            sub.text = "WASD 移动 · 自动攻击";
            sub.font = font;
            sub.fontSize = 32;
            sub.alignment = TextAnchor.MiddleCenter;
            sub.color = new Color(0.8f, 0.8f, 0.8f); // 灰白

            // 5. Start 按钮（白底黑字）
            GameObject btnObj = new GameObject("StartButton");
            btnObj.transform.SetParent(canvasObject.transform, false);

            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.5f, 0.5f);
            btnRect.anchorMax = new Vector2(0.5f, 0.5f);
            btnRect.pivot = new Vector2(0.5f, 0.5f);
            btnRect.anchoredPosition = new Vector2(0f, -60f);
            btnRect.sizeDelta = new Vector2(200f, 80f);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = Color.white;

            Button button = btnObj.AddComponent<Button>();
            button.targetGraphic = btnImage;
            button.onClick.AddListener(OnStartClicked);

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
            btnText.fontSize = 40;
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.black;
        }
    }
}
