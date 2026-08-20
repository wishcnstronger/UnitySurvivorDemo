using UnityEngine;
using UnityEngine.UI;

namespace SurvivorDemo
{
    /// <summary>
    /// 诅咒效果控制器（挂在 Player 上，由 GameSetup 自动添加）。
    /// 根据诅咒值阶段显示屏幕红色叠加层；终焉状态（≥100）每秒扣 2 HP。
    /// 只读 curseValue，不修改任何属性。
    /// </summary>
    public class CurseEffectController : MonoBehaviour
    {
        /// <summary>终焉状态每秒掉血量</summary>
        private const float FinalDmgPerSecond = 2f;

        /// <summary>终焉红边脉动速度</summary>
        private const float PulseSpeed = 3f;

        /// <summary>基础红边颜色（深红）</summary>
        private static readonly Color OverlayColor = new Color(0.6f, 0.05f, 0.1f, 0f);

        private PlayerStats stats;
        private PlayerHealth health;
        private Image overlay;
        private float chipTimer;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            health = GetComponent<PlayerHealth>();
            CreateOverlay();
        }

        /// <summary>程序化创建全屏红色叠加 Image</summary>
        private void CreateOverlay()
        {
            Canvas canvas = new GameObject("CurseOverlayCanvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.gameObject.AddComponent<CanvasScaler>();
            canvas.gameObject.AddComponent<GraphicRaycaster>();

            GameObject imgObj = new GameObject("CurseOverlay");
            imgObj.transform.SetParent(canvas.transform, false);

            overlay = imgObj.AddComponent<Image>();
            overlay.color = Color.clear;
            overlay.raycastTarget = false;

            // 全屏拉伸
            RectTransform rt = overlay.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private void Update()
        {
            if (stats == null) return;

            int curse = stats.curseValue;

            // 阶段式 alpha
            float baseAlpha;
            if (curse >= 100)      baseAlpha = 0.25f;
            else if (curse >= 80)  baseAlpha = 0.20f;
            else if (curse >= 60)  baseAlpha = 0.15f;
            else if (curse >= 40)  baseAlpha = 0.10f;
            else if (curse >= 20)  baseAlpha = 0.05f;
            else                   baseAlpha = 0f;

            // 终焉脉动
            if (curse >= 100)
            {
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * PulseSpeed);
                baseAlpha *= 0.7f + 0.3f * pulse;
            }

            overlay.color = new Color(OverlayColor.r, OverlayColor.g, OverlayColor.b, baseAlpha);

            // 终焉 chip damage（逆命免疫）
            if (curse >= 100 && stats.CurrentHP > 0f && !stats.curseImmune)
            {
                chipTimer += Time.deltaTime;
                if (chipTimer >= 1f)
                {
                    chipTimer = 0f;
                    if (health != null)
                        health.TakeDamage(FinalDmgPerSecond);
                    else
                        stats.TakeDamage(FinalDmgPerSecond);
                }
            }
        }
    }
}
