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

        /// <summary>面板背景：深石紫</summary>
        public static readonly Color PanelBg = new Color(0.06f, 0.05f, 0.10f, 0.92f);

        /// <summary>全屏遮罩：半透明深色</summary>
        public static readonly Color OverlayBg = new Color(0.04f, 0.04f, 0.06f, 0.7f);

        /// <summary>地牢金边框（更亮更饱和）</summary>
        public static readonly Color GoldBorder = new Color(0.95f, 0.72f, 0.15f, 1f);

        /// <summary>亮金文字（更亮）</summary>
        public static readonly Color GoldText = new Color(1f, 0.88f, 0.35f, 1f);

        /// <summary>石灰文字（提高可读性）</summary>
        public static readonly Color StoneText = new Color(0.78f, 0.76f, 0.82f, 1f);

        /// <summary>深石边框（HUD 用）</summary>
        public static readonly Color StoneBorder = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        /// <summary>地牢蓝（经验条用，更亮更饱和）</summary>
        public static readonly Color DungeonBlue = new Color(0.25f, 0.55f, 1.0f, 1f);

        /// <summary>暖白文字（时间用）</summary>
        public static readonly Color WarmWhite = new Color(1f, 0.95f, 0.85f, 1f);

        /// <summary>按钮深棕底（更暗）</summary>
        public static readonly Color BtnNormal = new Color(0.18f, 0.14f, 0.08f, 1f);

        /// <summary>按钮悬停暖棕（更亮更暖）</summary>
        public static readonly Color BtnHover = new Color(0.38f, 0.30f, 0.12f, 1f);

        /// <summary>按钮按下深暗</summary>
        public static readonly Color BtnPressed = new Color(0.14f, 0.11f, 0.07f, 1f);

        /// <summary>HUD 背景半透明深色（更暗更不透明）</summary>
        public static readonly Color HudBg = new Color(0.04f, 0.03f, 0.08f, 0.85f);

        /// <summary>分隔线颜色</summary>
        public static readonly Color Divider = new Color(0.85f, 0.65f, 0.2f, 0.25f);

        // ======== Sprite 生成 ========

        /// <summary>生成圆角矩形 Sprite（用于面板背景）。设置了 border，配合 Sliced 使用。</summary>
        public static Sprite CreateRoundedSprite(Color color, int size = 64, float cornerRadius = 8f)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inCornerX = x < cornerRadius || x >= size - cornerRadius;
                    bool inCornerY = y < cornerRadius || y >= size - cornerRadius;

                    if (inCornerX && inCornerY)
                    {
                        // 角区域：检查到角圆心的距离
                        float cx = x < cornerRadius ? cornerRadius : size - cornerRadius - 1;
                        float cy = y < cornerRadius ? cornerRadius : size - cornerRadius - 1;
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        tex.SetPixel(x, y, dist <= cornerRadius ? color : Color.clear);
                    }
                    else
                    {
                        // 非角区域：直接填充
                        tex.SetPixel(x, y, color);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect, new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
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
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect, new Vector4(thickness, thickness, thickness, thickness));
        }

        /// <summary>
        /// 生成带边框的垂直渐变 Sprite（边框色 + 内部从 topColor 到 bottomColor 渐变）。
        /// 设置了 Sprite.border，配合 Image.Type.Sliced 使用时边框不会被拉伸。
        /// </summary>
        public static Sprite CreateGradientBorderSprite(Color borderColor, Color topColor, Color bottomColor, int size = 64, int thickness = 4)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x < thickness || x >= size - thickness || y < thickness || y >= size - thickness)
                        tex.SetPixel(x, y, borderColor);
                    else
                    {
                        float t = (float)(y - thickness) / (size - 2 * thickness);
                        tex.SetPixel(x, y, Color.Lerp(bottomColor, topColor, t));
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size, 0, SpriteMeshType.FullRect, new Vector4(thickness, thickness, thickness, thickness));
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

        /// <summary>给文字添加柔和投影（比 Outline 更自然，不臃肿）</summary>
        public static void AddShadow(Text text, Color? color = null, Vector2? distance = null)
        {
            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = color ?? new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = distance ?? new Vector2(1.5f, -1.5f);
        }

        /// <summary>
        /// 给文字添加组合效果：微 Outline（清晰度）+ 柔 Shadow（深度感）。
        /// 比单独用 Outline(effectDistance=2,-2) 更精致，不臃肿。
        /// </summary>
        public static void AddTextEffect(Text text, Color? outlineColor = null, Color? shadowColor = null)
        {
            Outline outline = text.gameObject.AddComponent<Outline>();
            outline.effectColor = outlineColor ?? new Color(0f, 0f, 0f, 0.5f);
            outline.effectDistance = new Vector2(1f, -1f);

            Shadow shadow = text.gameObject.AddComponent<Shadow>();
            shadow.effectColor = shadowColor ?? new Color(0f, 0f, 0f, 0.4f);
            shadow.effectDistance = new Vector2(2f, -2f);
        }

        /// <summary>生成水平分隔线 Sprite（两端渐隐，中间实色）</summary>
        public static Sprite CreateDividerSprite(Color color, int width = 128, int height = 4)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            float center = width / 2f;
            float fadeRange = width * 0.35f;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Mathf.Abs(x - center);
                    float alpha = dist < fadeRange ? 1f : Mathf.Max(0f, 1f - (dist - fadeRange) / (width / 2f - fadeRange));
                    Color c = color;
                    c.a *= alpha;
                    tex.SetPixel(x, y, c);
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), width);
        }
    }
}
