using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SurvivorDemo
{
    /// <summary>
    /// 升级数据表（静态配置类）。
    /// 移除稀有度系统，改为等级递增。
    /// 分为属性池（8种）和机制池（10种），按时间动态调整权重。
    /// 机制分灵魂流（死神收割灵魂）和收割流（镰刀近战+吸血）。
    /// </summary>
    public static class UpgradeConfig
    {
        // ======== 枚举 ========

        /// <summary>升级分类</summary>
        public enum UpgradeCategory { Stat, Mechanic, Core, Curse }

        /// <summary>升级类型（8 属性 + 10 机制）</summary>
        public enum UpgradeType
        {
            // === 属性升级（可重复选，效果随等级递增） ===
            FireRate = 0,
            Damage = 1,
            MoveSpeed = 2,
            MaxHP = 3,
            Armor = 4,
            MagnetRange = 5,
            XPBoost = 6,
            Crit = 7,
            // === 灵魂流机制 ===
            SoulHarvest = 8,
            SoulPower = 9,
            SoulChain = 10,
            SoulSwarm = 11,
            SoulCurse = 12,
            // === 收割流机制 ===
            ScytheUnlock = 13,
            ScytheRange = 14,
            ScytheDamage = 15,
            ScytheSpeed = 16,
            Lifesteal = 17,
            // === 核心机制 ===
            DeathLight = 18,
            // === 光束强化 ===
            BeamCount = 19,
            BeamRadius = 20,
            BeamRefraction = 21,
            // === 灵魂强化 ===
            SoulMultiply = 22,
            SoulExplosion = 23,
            // === 核心机制 ===
            DeathDescend = 24,
            // === 诅咒强力升级 ===
            CurseDamage = 25,
            CurseBeam = 26,
            CurseSoul = 27,
            CurseSurvival = 28,
            // === 诅咒阈值解锁升级 ===
            ForbiddenKnowledge = 29,
            GraspOfDeath = 30,
            Calamity = 31,
            DefyFate = 32
        }

        /// <summary>升级类型总数（pickCounts 数组长度用）</summary>
        public static int TypeCount => (int)UpgradeType.DefyFate + 1;

        /// <summary>一张升级卡片的内容：类型（无稀有度）</summary>
        public struct UpgradeDefinition
        {
            public UpgradeType type;
            public UpgradeDefinition(UpgradeType t) { type = t; }
        }

        // ======== 升级元数据 ========

        /// <summary>升级等级数据</summary>
        public struct UpgradeLevelData
        {
            public int maxLevel;
            public int curseCost;
            public UpgradeCategory category;
            public UpgradeType prerequisite;
            public int prerequisiteLevel;
            public int curseThreshold;
        }

        /// <summary>获取某升级类型的元数据</summary>
        public static UpgradeLevelData GetLevelData(UpgradeType type)
        {
            switch (type)
            {
                // --- 属性升级（无前置，无诅咒） ---
                case UpgradeType.FireRate:    return new UpgradeLevelData { maxLevel = 99, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.Damage:      return new UpgradeLevelData { maxLevel = 99, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.MoveSpeed:   return new UpgradeLevelData { maxLevel = 20, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.MaxHP:       return new UpgradeLevelData { maxLevel = 99, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.Armor:       return new UpgradeLevelData { maxLevel = 30, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.MagnetRange:  return new UpgradeLevelData { maxLevel = 20, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.XPBoost:     return new UpgradeLevelData { maxLevel = 20, curseCost = 0, category = UpgradeCategory.Stat };
                case UpgradeType.Crit:        return new UpgradeLevelData { maxLevel = 99, curseCost = 0, category = UpgradeCategory.Stat };

                // --- 灵魂流机制 ---
                case UpgradeType.SoulHarvest:  return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic };
                case UpgradeType.SoulPower:    return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 1 };
                case UpgradeType.SoulChain:    return new UpgradeLevelData { maxLevel = 5, curseCost = 2, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 3 };
                case UpgradeType.SoulSwarm:    return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 1 };
                case UpgradeType.SoulCurse:    return new UpgradeLevelData { maxLevel = 1, curseCost = 3, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 2 };

                // --- 收割流机制 ---
                case UpgradeType.ScytheUnlock:  return new UpgradeLevelData { maxLevel = 1, curseCost = 0, category = UpgradeCategory.Mechanic };
                case UpgradeType.ScytheRange:   return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.ScytheUnlock, prerequisiteLevel = 1 };
                case UpgradeType.ScytheDamage:   return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.ScytheUnlock, prerequisiteLevel = 1 };
                case UpgradeType.ScytheSpeed:   return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.ScytheUnlock, prerequisiteLevel = 1 };
                case UpgradeType.Lifesteal:     return new UpgradeLevelData { maxLevel = 5, curseCost = 1, category = UpgradeCategory.Mechanic };

                // --- 核心机制 ---
                case UpgradeType.DeathLight:   return new UpgradeLevelData { maxLevel = 1, curseCost = 0, category = UpgradeCategory.Core };

                // --- 光束强化 ---
                case UpgradeType.BeamCount:    return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.DeathLight, prerequisiteLevel = 1 };
                case UpgradeType.BeamRadius:   return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.DeathLight, prerequisiteLevel = 1 };
                case UpgradeType.BeamRefraction: return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.DeathLight, prerequisiteLevel = 1 };

                // --- 灵魂强化 ---
                case UpgradeType.SoulMultiply:   return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 1 };
                case UpgradeType.SoulExplosion:  return new UpgradeLevelData { maxLevel = 5, curseCost = 0, category = UpgradeCategory.Mechanic, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 1 };

                // --- 核心机制（续） ---
                case UpgradeType.DeathDescend:  return new UpgradeLevelData { maxLevel = 1, curseCost = 0, category = UpgradeCategory.Core };

                // --- 诅咒强力升级（跨流派，高收益高代价） ---
                case UpgradeType.CurseDamage:   return new UpgradeLevelData { maxLevel = 3, curseCost = 15, category = UpgradeCategory.Curse };
                case UpgradeType.CurseBeam:    return new UpgradeLevelData { maxLevel = 3, curseCost = 20, category = UpgradeCategory.Curse, prerequisite = UpgradeType.DeathLight, prerequisiteLevel = 1 };
                case UpgradeType.CurseSoul:    return new UpgradeLevelData { maxLevel = 3, curseCost = 20, category = UpgradeCategory.Curse, prerequisite = UpgradeType.SoulHarvest, prerequisiteLevel = 1 };
                case UpgradeType.CurseSurvival: return new UpgradeLevelData { maxLevel = 3, curseCost = 10, category = UpgradeCategory.Curse };

                // --- 诅咒阈值解锁升级（需达到对应诅咒值才出现） ---
                case UpgradeType.ForbiddenKnowledge: return new UpgradeLevelData { maxLevel = 3, curseCost = 10, category = UpgradeCategory.Curse, curseThreshold = 20 };
                case UpgradeType.GraspOfDeath:    return new UpgradeLevelData { maxLevel = 3, curseCost = 10, category = UpgradeCategory.Curse, curseThreshold = 40 };
                case UpgradeType.Calamity:       return new UpgradeLevelData { maxLevel = 3, curseCost = 15, category = UpgradeCategory.Curse, curseThreshold = 60 };
                case UpgradeType.DefyFate:       return new UpgradeLevelData { maxLevel = 1, curseCost = 20, category = UpgradeCategory.Curse, curseThreshold = 80 };

                default: return new UpgradeLevelData { maxLevel = 1, curseCost = 0, category = UpgradeCategory.Stat };
            }
        }

        /// <summary>获取某类型的分类（优先看 GetLevelData，再按池归属判定）</summary>
        public static UpgradeCategory GetCategory(UpgradeType type)
        {
            var data = GetLevelData(type);
            // Core 和 Curse 需要在 GetLevelData 中显式设置 category
            // 当前所有已有升级的 category 是 Stat 或 Mechanic，不受影响
            return data.category;
        }

        /// <summary>获取分类对应的色条颜色</summary>
        public static Color GetCategoryColor(UpgradeCategory category)
        {
            switch (category)
            {
                case UpgradeCategory.Stat:      return new Color(0.95f, 0.72f, 0.15f);  // 金色
                case UpgradeCategory.Mechanic:  return new Color(0.69f, 0.3f, 1f);     // 紫色
                case UpgradeCategory.Core:      return new Color(0.3f, 0.65f, 1f);     // 蓝色
                case UpgradeCategory.Curse:     return new Color(1f, 0.2f, 0.2f);     // 红色
                default:                        return new Color(0.95f, 0.72f, 0.15f);
            }
        }

        // ======== 等级数值公式 ========

        /// <summary>按等级计算升级数值（level 从 1 开始，1 = 首次选择的效果）</summary>
        public static float GetValue(UpgradeType type, int level)
        {
            switch (type)
            {
                // --- 属性升级 ---
                case UpgradeType.FireRate:    return 1f + 0.08f * level;           // 攻速倍率
                case UpgradeType.Damage:      return 1f + Mathf.Floor(level * 0.3f); // 伤害加法
                case UpgradeType.MoveSpeed:   return 1f + 0.05f * level;           // 移速倍率
                case UpgradeType.MaxHP:       return 5f + level * 2f;               // 生命加法
                case UpgradeType.Armor:       return 1f + Mathf.Floor(level * 0.2f); // 护甲加法
                case UpgradeType.MagnetRange:  return 0.5f + level * 0.1f;          // 磁铁加法
                case UpgradeType.XPBoost:     return 0.1f + level * 0.02f;          // 经验加法
                case UpgradeType.Crit:        return 0.03f + level * 0.005f;        // 暴击加法

                // --- 灵魂流 ---
                case UpgradeType.SoulHarvest:  return 0.15f + 0.05f * level;        // 生成概率
                case UpgradeType.SoulPower:    return 0.5f + 0.3f * level;          // 伤害倍率
                case UpgradeType.SoulChain:    return 3 + 2 * level;                 // 连锁次数
                case UpgradeType.SoulSwarm:    return 3 + 3 * level;                // 上限
                case UpgradeType.SoulCurse:    return 1f;                           // 标记

                // --- 收割流 ---
                case UpgradeType.ScytheUnlock:  return 1f;                          // 标记
                case UpgradeType.ScytheRange:   return 3.0f + 0.5f * level;         // 半径
                case UpgradeType.ScytheDamage:   return 1.2f + 0.3f * level;        // 伤害倍率
                case UpgradeType.ScytheSpeed:   return Mathf.Max(0.3f, 1.5f * (1f - 0.1f * level)); // 间隔
                case UpgradeType.Lifesteal:     return 0.03f + 0.02f * level;       // 吸血率

                // --- 核心机制 ---
                case UpgradeType.DeathLight:   return 1f;                           // 标记

                // --- 光束强化 ---
                case UpgradeType.BeamCount:    return 1 + level;                     // 光束总数（base 1 + level）
                case UpgradeType.BeamRadius:   return 0.5f + 0.3f * level;            // 光束命中半径
                case UpgradeType.BeamRefraction: return 1 + level;                     // 最大折射次数

                // --- 灵魂强化 ---
                case UpgradeType.SoulMultiply:   return 1 + level;                     // 生成数量倍率
                case UpgradeType.SoulExplosion:  return 1.5f + 0.5f * level;            // 爆炸半径

                // --- 核心机制（续） ---
                case UpgradeType.DeathDescend:  return 1f;                             // 标记

                // --- 诅咒强力升级 ---
                case UpgradeType.CurseDamage:   return 0.5f * level;                   // 伤害百分比加成（Lv1=+50%, Lv2=+100%, Lv3=+150%）
                case UpgradeType.CurseBeam:    return level;                          // 光束数量额外加成（Lv1=+1, Lv2=+2, Lv3=+3）
                case UpgradeType.CurseSoul:    return level;                          // 灵魂生成概率倍率等级
                case UpgradeType.CurseSurvival: return 50f * level;                   // 生命加成（Lv1=+50, Lv2=+100, Lv3=+150）

                // --- 诅咒阈值解锁升级 ---
                case UpgradeType.ForbiddenKnowledge: return 1f * level;                 // 经验倍率加成（Lv1=+100%, Lv2=+200%, Lv3=+300%）
                case UpgradeType.GraspOfDeath:    return 0.15f * level;                 // 吸血率加成（Lv1=+15%, Lv2=+30%, Lv3=+45%）
                case UpgradeType.Calamity:       return 0.8f * level;                  // 伤害百分比加成（Lv1=+80%, Lv2=+160%, Lv3=+240%）
                case UpgradeType.DefyFate:       return 1f;                            // 标记

                default: return 0f;
            }
        }

        // ======== 名称与描述 ========

        /// <summary>获取升级类型的中文名</summary>
        public static string GetTypeName(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.FireRate:    return "攻速提升";
                case UpgradeType.Damage:      return "攻击力";
                case UpgradeType.MoveSpeed:   return "移动速度";
                case UpgradeType.MaxHP:       return "生命提升";
                case UpgradeType.Armor:       return "护甲强化";
                case UpgradeType.MagnetRange:  return "拾取范围";
                case UpgradeType.XPBoost:     return "经验加成";
                case UpgradeType.Crit:        return "暴击";
                case UpgradeType.SoulHarvest:  return "灵魂收割";
                case UpgradeType.SoulPower:    return "灵魂强化";
                case UpgradeType.SoulChain:    return "灵魂连锁";
                case UpgradeType.SoulSwarm:    return "灵魂蜂拥";
                case UpgradeType.SoulCurse:    return "灵魂诅咒";
                case UpgradeType.ScytheUnlock:  return "镰刀解锁";
                case UpgradeType.ScytheRange:   return "镰刀范围";
                case UpgradeType.ScytheDamage:  return "镰刀伤害";
                case UpgradeType.ScytheSpeed:   return "镰刀攻速";
                case UpgradeType.Lifesteal:     return "生命汲取";
                case UpgradeType.DeathLight:   return "死神之光";
                case UpgradeType.BeamCount:    return "光束增殖";
                case UpgradeType.BeamRadius:   return "光束扩散";
                case UpgradeType.BeamRefraction: return "光束折射";
                case UpgradeType.SoulMultiply:   return "灵魂增殖";
                case UpgradeType.SoulExplosion:  return "灵魂爆裂";
                case UpgradeType.DeathDescend:  return "死神降临";
                case UpgradeType.CurseDamage:   return "死神契约";
                case UpgradeType.CurseBeam:    return "万光归一";
                case UpgradeType.CurseSoul:    return "亡魂盛宴";
                case UpgradeType.CurseSurvival: return "死者馈赠";
                case UpgradeType.ForbiddenKnowledge: return "禁忌知识";
                case UpgradeType.GraspOfDeath:    return "死亡之握";
                case UpgradeType.Calamity:       return "灾厄";
                case UpgradeType.DefyFate:       return "逆命";
                default: return "";
            }
        }

        /// <summary>获取升级卡片的描述（按下一级等级计算效果）</summary>
        public static string GetDescription(UpgradeType type, int nextLevel)
        {
            float value = GetValue(type, nextLevel);

            switch (type)
            {
                // 属性
                case UpgradeType.FireRate:    return $"攻速 +{value * 100f - 100f:0}%";
                case UpgradeType.Damage:      return $"伤害 +{(int)value}";
                case UpgradeType.MoveSpeed:   return $"移速 +{(value - 1f) * 100f:0}%";
                case UpgradeType.MaxHP:       return $"最大生命 +{(int)value}";
                case UpgradeType.Armor:       return $"护甲 +{(int)value}";
                case UpgradeType.MagnetRange:  return $"拾取范围 +{value:0.#}";
                case UpgradeType.XPBoost:     return $"经验获取 +{value * 100f:0}%";
                case UpgradeType.Crit:        return $"暴击率 +{value * 100f:0.#}%";

                // 灵魂流
                case UpgradeType.SoulHarvest:  return $"击杀 {value * 100f:0}% 概率生成灵魂\n同时存在上限 {(int)GetValue(UpgradeType.SoulSwarm, 0) + (int)value * 0 + 3 + (nextLevel - 1)}";
                case UpgradeType.SoulPower:    return $"灵魂伤害倍率 ×{value:0.#}";
                case UpgradeType.SoulChain:    return $"灵魂命中后连锁 {(int)value} 个敌人\n每跳伤害递减 20%";
                case UpgradeType.SoulSwarm:    return $"灵魂上限 +{(int)value - 3}";
                case UpgradeType.SoulCurse:    return $"灵魂大幅强化：穿透+不掉血\n代价：每次生成灵魂 -1 HP";

                // 收割流
                case UpgradeType.ScytheUnlock:  return "解锁近战镰刀：每 1.5s 挥砍\n前方扇形范围造成 120% 伤害";
                case UpgradeType.ScytheRange:   return $"镰刀范围 → {value:0.#}";
                case UpgradeType.ScytheDamage:  return $"镰刀伤害倍率 → {value:0.#}";
                case UpgradeType.ScytheSpeed:   return $"镰刀间隔 → {value:0.##}s";
                case UpgradeType.Lifesteal:     return $"所有伤害吸血 {value * 100f:0}%";
                case UpgradeType.DeathLight:   return "将普通攻击转化为死亡光束";
                case UpgradeType.BeamCount:    return $"光束数量 → {(int)value}";
                case UpgradeType.BeamRadius:   return $"光束半径 → {value:0.#}";
                case UpgradeType.BeamRefraction: return $"击杀后可折射 {(int)value} 次\n50% 概率向附近敌人折射";
                case UpgradeType.SoulMultiply:   return $"击杀生成灵魂数量 ×{(int)value}";
                case UpgradeType.SoulExplosion:  return $"灵魂消散时爆炸\n半径 {value:0.#} 造成范围伤害";
                case UpgradeType.DeathDescend:  return "每 30 秒释放死亡波\n清除普通敌人，Boss 受大量伤害";
                case UpgradeType.CurseDamage:   return $"全局伤害 +{value * 100f:0}%\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.CurseBeam:    return $"光束数量 +{(int)value}\n光束伤害 +{value * 20f:0}%\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.CurseSoul:    return $"灵魂生成概率 ×{(int)value + 1}\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.CurseSurvival: return $"最大生命 +{(int)value}\n护甲 +{nextLevel * 5}\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.ForbiddenKnowledge: return $"经验获取 +{value * 100f:0}%\n需诅咒 ≥{GetLevelData(type).curseThreshold}\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.GraspOfDeath:    return $"吸血率 +{value * 100f:0}%\n需诅咒 ≥{GetLevelData(type).curseThreshold}\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.Calamity:       return $"全局伤害 +{value * 100f:0}%\n需诅咒 ≥{GetLevelData(type).curseThreshold}\n诅咒 +{GetLevelData(type).curseCost}";
                case UpgradeType.DefyFate:       return $"免疫终焉状态\n全属性大幅提升\n需诅咒 ≥{GetLevelData(type).curseThreshold}\n诅咒 +{GetLevelData(type).curseCost}";

                default: return "";
            }
        }

        // ======== 随机抽取（双池系统） ========

        /// <summary>属性池中的类型</summary>
        private static readonly UpgradeType[] statPool =
        {
            UpgradeType.FireRate, UpgradeType.Damage, UpgradeType.MoveSpeed,
            UpgradeType.MaxHP, UpgradeType.Armor, UpgradeType.MagnetRange,
            UpgradeType.XPBoost, UpgradeType.Crit
        };

        /// <summary>机制池中的类型</summary>
        private static readonly UpgradeType[] mechanicPool =
        {
            UpgradeType.SoulHarvest, UpgradeType.SoulPower, UpgradeType.SoulChain,
            UpgradeType.SoulSwarm, UpgradeType.SoulCurse,
            UpgradeType.ScytheUnlock, UpgradeType.ScytheRange, UpgradeType.ScytheDamage,
            UpgradeType.ScytheSpeed, UpgradeType.Lifesteal,
            UpgradeType.BeamCount, UpgradeType.BeamRadius,
            UpgradeType.BeamRefraction,
            UpgradeType.SoulMultiply, UpgradeType.SoulExplosion
        };

        /// <summary>核心机制池（当前为空，后续添加死神之光/亡魂契约/死神降临）</summary>
        private static readonly UpgradeType[] corePool = { UpgradeType.DeathLight, UpgradeType.DeathDescend };

        /// <summary>诅咒池（当前为空，后续添加跨流派诅咒升级）</summary>
        private static readonly UpgradeType[] cursePool =
        {
            UpgradeType.CurseDamage, UpgradeType.CurseBeam,
            UpgradeType.CurseSoul, UpgradeType.CurseSurvival
        };

        /// <summary>
        /// 四池随机抽取。
        /// 前 5 分钟：属性 60% / 机制 40%；5 分钟后：各 50%。
        /// Core 和 Curse 池当前为空，抽取时自动跳过回退到属性池。
        /// 满级和前置不满足的升级从池中排除。
        /// </summary>
        public static UpgradeDefinition RollChoice(int[] pickCounts, float gameTime)
        {
            // 决定抽哪个池
            float mechanicWeight = gameTime < 300f ? 40f : 50f;
            bool rollMechanic = Random.Range(0f, 100f) < mechanicWeight;

            if (rollMechanic)
            {
                var available = GetAvailableFromPool(mechanicPool, pickCounts);
                if (available.Count > 0)
                    return new UpgradeDefinition(available[Random.Range(0, available.Count)]);
            }

            // 尝试 Core 池（当前为空，自动跳过）
            var availableCore = GetAvailableFromPool(corePool, pickCounts);
            if (availableCore.Count > 0 && Random.Range(0f, 100f) < 15f)
                return new UpgradeDefinition(availableCore[Random.Range(0, availableCore.Count)]);

            // 尝试 Curse 池（当前为空，自动跳过）
            var availableCurse = GetAvailableFromPool(cursePool, pickCounts);
            if (availableCurse.Count > 0 && Random.Range(0f, 100f) < 10f)
                return new UpgradeDefinition(availableCurse[Random.Range(0, availableCurse.Count)]);

            // 退回属性池
            var availableStats = GetAvailableFromPool(statPool, pickCounts);
            if (availableStats.Count > 0)
                return new UpgradeDefinition(availableStats[Random.Range(0, availableStats.Count)]);

            // 属性池也空了（理论上不会）→ 随便给一个
            return new UpgradeDefinition(UpgradeType.Damage);
        }

        /// <summary>从指定池中获取可用升级（排除满级 + 前置不满足的）</summary>
        private static List<UpgradeType> GetAvailableFromPool(UpgradeType[] pool, int[] pickCounts)
        {
            var result = new List<UpgradeType>();
            foreach (var type in pool)
            {
                int level = pickCounts[(int)type];
                var data = GetLevelData(type);
                if (level >= data.maxLevel)
                    continue;
                // 检查前置
                if (data.prerequisiteLevel > 0)
                {
                    int prereqLevel = pickCounts[(int)data.prerequisite];
                    if (prereqLevel < data.prerequisiteLevel)
                        continue;
                }
                result.Add(type);
            }
            return result;
        }

        /// <summary>检查前置是否满足</summary>
        public static bool IsPrerequisiteMet(UpgradeType type, int[] pickCounts)
        {
            var data = GetLevelData(type);
            if (data.prerequisiteLevel <= 0)
                return true;
            return pickCounts[(int)data.prerequisite] >= data.prerequisiteLevel;
        }
    }
}
