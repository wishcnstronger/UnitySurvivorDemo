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
        public enum UpgradeType { FireRate, BulletCount, Penetration, Damage, MoveSpeed, MaxHP, Armor, MagnetRange }

        /// <summary>稀有度（白→金→红，红为特殊卡）</summary>
        public enum Rarity { White, Green, Blue, Purple, Gold, Red }

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

        /// <summary>稀有度出现权重（白→金，红卡在 RollChoice 中单独处理）</summary>
        private static readonly int[] rarityWeights = { 35, 25, 18, 12, 10 };

        /// <summary>稀有度颜色（白 / 绿 / 蓝 / 紫 / 金 / 红）</summary>
        private static readonly Color[] rarityColors =
        {
            new Color(1f, 1f, 1f),          // 白 #FFFFFF
            new Color(0f, 1f, 0.5f),        // 绿 #00FF7F
            new Color(0.3f, 0.65f, 1f),     // 蓝 #4CA6FF
            new Color(0.69f, 0.3f, 1f),     // 紫 #B04CFF
            new Color(1f, 0.84f, 0f),       // 金 #FFD700
            new Color(1f, 0.2f, 0.2f)       // 红 #FF3333
        };

        /// <summary>稀有度中文名</summary>
        private static readonly string[] rarityNames = { "白", "绿", "蓝", "紫", "金", "红" };

        // ======== 数值表（每种类型 × 5 种稀有度，金卡概率 10% 下期望略微提高） ========

        /// <summary>攻速提升：攻速倍率（>1 表示更快），换算到攻击间隔时取倒数</summary>
        private static readonly float[] fireRateValues = { 1.08f, 1.15f, 1.22f, 1.35f, 1.55f };

        /// <summary>攻击力：加法，作用于每发子弹伤害</summary>
        private static readonly int[] damageValues = { 1, 2, 3, 5, 8 };

        /// <summary>移动速度：系数，作用于 moveSpeed（越大越快）</summary>
        private static readonly float[] moveSpeedValues = { 1.05f, 1.08f, 1.11f, 1.15f, 1.20f };

        /// <summary>最大生命值：加法，作用于 maxHP</summary>
        private static readonly float[] maxHPValues = { 5f, 8f, 15f, 25f, 45f };

        /// <summary>护甲：加法，固定减伤</summary>
        private static readonly int[] armorValues = { 2, 3, 4, 6, 10 };

        /// <summary>经验拾取范围：加法，作用于 magnetRange</summary>
        private static readonly float[] magnetRangeValues = { 1f, 2f, 3f, 4f, 5f };

        /// <summary>红卡固定值：子弹数量和穿透各 +1</summary>
        private const int RedCardValue = 1;

        // ======== 查询方法 ========

        /// <summary>获取某张卡的加成数值</summary>
        public static float GetValue(UpgradeDefinition def)
        {
            // 红卡（子弹数量/穿透）固定值
            if (def.rarity == Rarity.Red)
                return RedCardValue;

            switch (def.type)
            {
                case UpgradeType.FireRate: return fireRateValues[(int)def.rarity];
                case UpgradeType.Damage: return damageValues[(int)def.rarity];
                case UpgradeType.MoveSpeed: return moveSpeedValues[(int)def.rarity];
                case UpgradeType.MaxHP: return maxHPValues[(int)def.rarity];
                case UpgradeType.Armor: return armorValues[(int)def.rarity];
                case UpgradeType.MagnetRange: return magnetRangeValues[(int)def.rarity];
                // 子弹数量和穿透只有红卡，走到这里说明数据异常，返回 0
                case UpgradeType.BulletCount: return 0;
                case UpgradeType.Penetration: return 0;
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
                case UpgradeType.MaxHP: return "生命提升";
                case UpgradeType.Armor: return "护甲强化";
                case UpgradeType.MagnetRange: return "拾取范围";
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
                case UpgradeType.MaxHP: return $"最大生命 +{(int)value}";
                case UpgradeType.Armor: return $"护甲 +{(int)value}";
                case UpgradeType.MagnetRange: return $"拾取范围 +{value:0.#}";
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
        /// 按权重随机出一个稀有度（白→金）。
        /// 累加权值法：把权重排成一列，随机一个总数内的数，落在哪段就是哪个稀有度。
        /// </summary>
        public static Rarity RollRarity()
        {
            int total = 0;
            for (int i = 0; i < rarityWeights.Length; i++)
                total += rarityWeights[i];

            int roll = Random.Range(0, total);

            int cumulative = 0;
            for (int i = 0; i < rarityWeights.Length; i++)
            {
                cumulative += rarityWeights[i];
                if (roll < cumulative)
                    return (Rarity)i;
            }

            return Rarity.White;
        }

        /// <summary>
        /// 随机生成一张升级卡。
        /// 5% 概率出红卡（子弹数量或穿透，固定 +1）；
        /// 95% 概率出普通卡（6 种类型 × 5 种稀有度）。
        /// 护甲 50% 概率替换为其他类型，降低整体出现率。
        /// </summary>
        public static UpgradeDefinition RollChoice()
        {
            // 5% 概率出红卡
            if (Random.Range(0f, 1f) < 0.20f)
            {
                UpgradeType redType = Random.Range(0, 2) == 0
                    ? UpgradeType.BulletCount
                    : UpgradeType.Penetration;
                return new UpgradeDefinition(redType, Rarity.Red);
            }

            // 正常卡：6 种类型（不含子弹数量和穿透）
            UpgradeType[] normalTypes =
            {
                UpgradeType.FireRate,
                UpgradeType.Damage,
                UpgradeType.MoveSpeed,
                UpgradeType.MaxHP,
                UpgradeType.Armor,
                UpgradeType.MagnetRange
            };

            Rarity rarity = RollRarity();
            UpgradeType type = normalTypes[Random.Range(0, normalTypes.Length)];

            // 护甲整体抽取概率降低：50% 概率换成其他类型
            if (type == UpgradeType.Armor && Random.Range(0f, 1f) < 0.5f)
            {
                type = normalTypes[Random.Range(0, normalTypes.Length)];
            }

            return new UpgradeDefinition(type, rarity);
        }
    }
}
