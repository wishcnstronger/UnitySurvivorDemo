using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SurvivorDemo
{
    /// <summary>
    /// 升级数据表（静态配置类）。
    /// 所有升级相关的数值集中在这里，策划调数值只需改这个文件。
    /// </summary>
    public static class UpgradeConfig
    {
        /// <summary>升级类型</summary>
        public enum UpgradeType { FireRate, BulletCount, Penetration, Damage, MoveSpeed }

        /// <summary>稀有度（白→金，越来越稀有）</summary>
        public enum Rarity { White, Green, Blue, Purple, Gold }

        /// <summary>一张升级卡片的内容：类型 + 稀有度</summary>
        public struct UpgradeDefinition
        {
            public UpgradeType type;
            public Rarity rarity;

            public UpgradeDefinition(UpgradeType t, Rarity r)
            {
                type = t;
                rarity = r;
            }
        }

        // ======== 稀有度配置 ========

        /// <summary>稀有度出现权重（白→金递减，总和 100）</summary>
        private static readonly int[] rarityWeights = { 50, 25, 15, 8, 2 };

        /// <summary>稀有度颜色（白 / 绿 / 蓝 / 紫 / 金）</summary>
        private static readonly Color[] rarityColors =
        {
            new Color(1f, 1f, 1f),          // 白 #FFFFFF
            new Color(0f, 1f, 0.5f),        // 绿 #00FF7F
            new Color(0.3f, 0.65f, 1f),     // 蓝 #4CA6FF
            new Color(0.69f, 0.3f, 1f),     // 紫 #B04CFF
            new Color(1f, 0.84f, 0f)        // 金 #FFD700
        };

        /// <summary>稀有度中文名</summary>
        private static readonly string[] rarityNames = { "白", "绿", "蓝", "紫", "金" };

        // ======== 数值表（每种类型 × 5 种稀有度） ========

        /// <summary>攻速提升：攻速倍率（>1 表示更快），换算到攻击间隔时取倒数</summary>
        private static readonly float[] fireRateValues = { 1.09f, 1.16f, 1.25f, 1.39f, 1.61f };

        /// <summary>子弹数量：加法，作用于每轮发射数</summary>
        private static readonly int[] bulletCountValues = { 1, 1, 2, 2, 3 };

        /// <summary>穿透：加法，作用于子弹可穿透敌人数</summary>
        private static readonly int[] penetrationValues = { 1, 1, 2, 2, 3 };

        /// <summary>攻击力：加法，作用于每发子弹伤害</summary>
        private static readonly int[] damageValues = { 1, 2, 4, 7, 12 };

        /// <summary>移动速度：系数，作用于 moveSpeed（越大越快）</summary>
        private static readonly float[] moveSpeedValues = { 1.05f, 1.08f, 1.12f, 1.16f, 1.22f };

        // ======== 查询方法 ========

        /// <summary>获取某张卡的加成数值（系数返回原值，加法返回整数）</summary>
        public static float GetValue(UpgradeDefinition def)
        {
            switch (def.type)
            {
                case UpgradeType.FireRate: return fireRateValues[(int)def.rarity];
                case UpgradeType.BulletCount: return bulletCountValues[(int)def.rarity];
                case UpgradeType.Penetration: return penetrationValues[(int)def.rarity];
                case UpgradeType.Damage: return damageValues[(int)def.rarity];
                case UpgradeType.MoveSpeed: return moveSpeedValues[(int)def.rarity];
                default: return 0f;
            }
        }

        /// <summary>获取升级类型的中文名</summary>
        public static string GetTypeName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.FireRate: return "攻速提升";
                case UpgradeType.BulletCount: return "子弹数量";
                case UpgradeType.Penetration: return "穿透";
                case UpgradeType.Damage: return "攻击力";
                case UpgradeType.MoveSpeed: return "移动速度";
                default: return "";
            }
        }

        /// <summary>获取升级卡片的一句话描述</summary>
        public static string GetDescription(UpgradeType type, Rarity rarity)
        {
            float value = GetValue(new UpgradeDefinition(type, rarity));

            switch (type)
            {
                case UpgradeType.FireRate: return $"攻速 ×{value:0.##}";
                case UpgradeType.BulletCount: return $"每轮子弹 +{(int)value}";
                case UpgradeType.Penetration: return $"穿透敌人 +{(int)value}";
                case UpgradeType.Damage: return $"伤害 +{(int)value}";
                case UpgradeType.MoveSpeed: return $"移速 ×{value:0.##}";
                default: return "";
            }
        }

        /// <summary>获取稀有度颜色</summary>
        public static Color GetRarityColor(Rarity rarity)
        {
            return rarityColors[(int)rarity];
        }

        /// <summary>获取稀有度中文名</summary>
        public static string GetRarityName(Rarity rarity)
        {
            return rarityNames[(int)rarity];
        }

        // ======== 随机方法 ========

        /// <summary>
        /// 按权重随机出一个稀有度。
        /// 累加权值法：把权重排成一列，随机一个总数内的数，落在哪段就是哪个稀有度。
        /// </summary>
        public static Rarity RollRarity()
        {
            // 总权重
            int total = 0;
            for (int i = 0; i < rarityWeights.Length; i++)
                total += rarityWeights[i];

            // 随机 0 ~ total-1
            int roll = Random.Range(0, total);

            // 累加权重，判断落在哪一段
            int cumulative = 0;
            for (int i = 0; i < rarityWeights.Length; i++)
            {
                cumulative += rarityWeights[i];
                if (roll < cumulative)
                    return (Rarity)i;
            }

            return Rarity.White; // 兜底，正常情况下走不到
        }

        /// <summary>
        /// 随机生成一张升级卡：随机稀有度 + 随机类型。
        /// </summary>
        public static UpgradeDefinition RollChoice()
        {
            Rarity rarity = RollRarity();
            // 用枚举成员数做随机上限，避免新增类型时忘了改这里导致抽不到新类型
            UpgradeType type = (UpgradeType)Random.Range(0, Enum.GetValues(typeof(UpgradeType)).Length);
            return new UpgradeDefinition(type, rarity);
        }
    }
}
