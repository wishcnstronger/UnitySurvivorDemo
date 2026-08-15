using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 地牢主题 UI 辅助工具。
    /// 提供统一色板、圆角/边框 Sprite 生成、按钮悬停效果，供各 UI 脚本复用。
    /// </summary>
    public static class UIDungeonTheme
    {
        // ======== 色板常量 ========

        /// <summary>面板背景：深石色</summary>
        public static readonly Color PanelBg = new Color(0.08f, 0.08f, 0.12f, 0.9f);

        /// <summary>全屏遮罩：半透明深色</summary>
        public static readonly Color OverlayBg = new Color(0.04f, 0.04f, 0.06f, 0.7f);

        /// <summary>地牢金边框</summary>
        public static readonly Color GoldBorder = new Color(0.85f, 0.65f, 0.2f, 1f);

        /// <summary>亮金文字</summary>
        public static readonly Color GoldText = new Color(1f, 0.85f, 0.3f, 1f);

        /// <summary>石灰文字</summary>
        public static readonly Color StoneText = new Color(0.7f, 0.7f, 0.75f, 1f);

        /// <summary>深石边框（HUD 用）</summary>
        public static readonly Color StoneBorder = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        /// <summary>地牢蓝（经验条用）</summary>
        public static readonly Color DungeonBlue = new Color(0.3f, 0.5f, 0.9f, 1f);

        /// <summary>暖白文字（时间用）</summary>
        public static readonly Color WarmWhite = new Color(1f, 0.95f, 0.85f, 1f);

        /// <summary>按钮深棕底</summary>
        public static readonly Color BtnNormal = new Color(0.15f, 0.12f, 0.08f, 1f);

        /// <summary>按钮悬停暖棕</summary>
        public static readonly Color BtnHover = new Color(0.25f, 0.2f, 0.1f, 1f);

        /// <summary>按钮按下深暗</summary>
        public static readonly Color BtnPressed = new Color(0.1f, 0.08f, 0.05f, 1f);

        // ======== Sprite 生成 ========

        /// <summary>生成圆角矩形 Sprite（用于面板背景）</summary>
        public static Sprite CreateRoundedSprite(Color color, int size = 64, float cornerRadius = 8f)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float center = size / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // 四角圆角检测
                    float dist = Mathf.Infinity;
                    float cx = center, cy = center;

                    if (x < cornerRadius) cx = cornerRadius;
                    else if (x > size - cornerRadius) cx = size - cornerRadius;

                    if (y < cornerRadius) cy = cornerRadius;
                    else if (y > size - cornerRadius) cy = size - cornerRadius;

                    if (x != cx || y != cy)
                        dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    if (dist <= cornerRadius)
                        tex.SetPixel(x, y, color);
                    else
                        tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>生成带边框的矩形 Sprite（边框色 + 内部填充色）</summary>
        public static Sprite CreateBorderSprite(Color borderColor, Color fillColor, int size = 64, int thickness = 3)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < thickness || x >= size - thickness || y < thickness || y >= size - thickness)
                        tex.SetPixel(x, y, borderColor);
                    else
                        tex.SetPixel(x, y, fillColor);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        /// <summary>生成纯色矩形 Sprite</summary>
        public static Sprite CreateSolidSprite(Color color, int size = 32)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, color);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        // ======== 按钮效果 ========

        /// <summary>给按钮添加地牢主题配色 + 悬停变色</summary>
        public static void StyleButton(Button button, Color? normalColor = null, Color? hoverColor = null, Color? pressedColor = null)
        {
            ColorBlock cb = button.colors;
            cb.normalColor = normalColor ?? BtnNormal;
            cb.highlightedColor = hoverColor ?? BtnHover;
            cb.pressedColor = pressedColor ?? BtnPressed;
            cb.selectedColor = normalColor ?? BtnNormal;
            cb.fadeDuration = 0.1f;
            button.colors = cb;
        }

        /// <summary>给 Transform 添加悬停缩放效果（需要 EventTrigger）</summary>
        public static void AddHoverScale(GameObject go, float hoverScale = 1.05f)
        {
            EventTrigger trigger = go.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = go.AddComponent<EventTrigger>();

            Vector3 originalScale = go.transform.localScale;

            EventTrigger.Entry enter = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerEnter
            };
            enter.callback.AddListener(_ => go.transform.localScale = originalScale * hoverScale);
            trigger.triggers.Add(enter);

            EventTrigger.Entry exit = new EventTrigger.Entry
            {
                eventID = EventTriggerType.PointerExit
            };
            exit.callback.AddListener(_ => go.transform.localScale = originalScale);
            trigger.triggers.Add(exit);
        }

        /// <summary>给文字添加黑色描边</summary>
        public static void AddOutline(Text text, Color? color = null, Vector2? distance = null)
        {
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = color ?? new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = distance ?? new Vector2(2f, -2f);
        }
    }
}
