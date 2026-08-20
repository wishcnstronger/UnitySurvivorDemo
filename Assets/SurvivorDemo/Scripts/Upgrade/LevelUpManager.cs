using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 升级流程管理器（挂在 Player 上）。
    /// 双池抽取（属性/机制）、刷新系统（每次升级 2 次刷新）、机制升级分发。
    /// </summary>
    public class LevelUpManager : MonoBehaviour
    {
        public PlayerStats stats;
        public PlayerWeapon weapon;
        public UpgradeUI upgradeUI;

        private bool isChoosing;
        private bool uiMissingLogged;

        /// <summary>每次升级提供的刷新次数</summary>
        private const int MaxRefreshPerLevel = 2;
        private int refreshCharges;

        /// <summary>当前三张卡（刷新时重新生成需要排除的类型列表）</summary>
        private List<UpgradeConfig.UpgradeType> currentTypes = new List<UpgradeConfig.UpgradeType>();

        private void Awake()
        {
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (weapon == null) weapon = GetComponent<PlayerWeapon>();
            if (upgradeUI == null) upgradeUI = GetComponent<UpgradeUI>();
        }

        private void Update()
        {
            if (!isChoosing && stats != null && stats.pendingLevelUps > 0)
            {
                if (stats.CurrentHP <= 0f)
                    return;
                StartLevelUp();
            }
        }

        private void StartLevelUp()
        {
            if (upgradeUI == null)
            {
                if (!uiMissingLogged)
                {
                    uiMissingLogged = true;
                    Debug.LogError("LevelUpManager 缺少 UpgradeUI 引用！");
                }
                return;
            }

            isChoosing = true;
            Time.timeScale = 0f;

            refreshCharges = MaxRefreshPerLevel;
            GenerateChoices();
            upgradeUI.SetRefreshCharges(refreshCharges, OnRefreshClicked);
        }

        /// <summary>生成三张不重复的卡</summary>
        private void GenerateChoices()
        {
            currentTypes.Clear();
            float gameTime = GameStats.playTime;

            for (int i = 0; i < 3; i++)
            {
                UpgradeConfig.UpgradeType choice;
                int attempts = 0;
                do
                {
                    var def = UpgradeConfig.RollChoice(BuildPickCounts(), gameTime);
                    choice = def.type;
                    attempts++;
                }
                while (currentTypes.Contains(choice) && attempts < 20);

                currentTypes.Add(choice);
            }

            var choices = new List<UpgradeConfig.UpgradeDefinition>();
            foreach (var t in currentTypes)
                choices.Add(new UpgradeConfig.UpgradeDefinition(t));

            upgradeUI.Show(choices, OnChoiceSelected);
        }

        /// <summary>刷新按钮回调：重新生成三张卡</summary>
        public void OnRefreshClicked()
        {
            if (refreshCharges <= 0) return;
            refreshCharges--;
            GenerateChoices();
            upgradeUI.SetRefreshCharges(refreshCharges, OnRefreshClicked);
        }

        private void OnChoiceSelected(UpgradeConfig.UpgradeDefinition def)
        {
            ApplyUpgrade(def.type);

            if (stats != null)
                stats.ConsumePendingLevelUp();

            if (stats != null && stats.CurrentHP <= 0f)
                return;

            isChoosing = false;
            Time.timeScale = 1f;
        }

        /// <summary>按类型应用升级</summary>
        private void ApplyUpgrade(UpgradeConfig.UpgradeType type)
        {
            if (stats == null) return;

            int level = stats.GetPickCount(type); // 当前等级（0=首次）
            int nextLevel = level + 1;
            var data = UpgradeConfig.GetLevelData(type);
            float value = UpgradeConfig.GetValue(type, nextLevel);

            if (data.category == UpgradeConfig.UpgradeCategory.Stat)
            {
                ApplyStatUpgrade(type, value);
            }
            else if (data.category == UpgradeConfig.UpgradeCategory.Core)
            {
                ApplyCoreUpgrade(type);
            }
            else if (data.category == UpgradeConfig.UpgradeCategory.Curse)
            {
                ApplyCurseUpgrade(type, nextLevel);
            }
            else
            {
                ApplyMechanicUpgrade(type, nextLevel);
            }

            // 诅咒
            if (data.curseCost > 0)
                stats.AddCurse(data.curseCost);

            stats.RecordPick(type);
        }

        /// <summary>属性升级分发</summary>
        private void ApplyStatUpgrade(UpgradeConfig.UpgradeType type, float value)
        {
            switch (type)
            {
                case UpgradeConfig.UpgradeType.FireRate:
                    if (weapon != null) weapon.AddFireRateMultiplier(value);
                    break;
                case UpgradeConfig.UpgradeType.Damage:
                    if (weapon != null) weapon.AddDamage(value);
                    break;
                case UpgradeConfig.UpgradeType.MoveSpeed:
                    stats.AddMoveSpeedMultiplier(value);
                    break;
                case UpgradeConfig.UpgradeType.MaxHP:
                    stats.AddMaxHP(value);
                    break;
                case UpgradeConfig.UpgradeType.Armor:
                    stats.AddArmor(value);
                    break;
                case UpgradeConfig.UpgradeType.MagnetRange:
                    stats.AddMagnetRange(value);
                    break;
                case UpgradeConfig.UpgradeType.XPBoost:
                    stats.AddXPRate(value);
                    break;
                case UpgradeConfig.UpgradeType.Crit:
                    if (weapon != null) weapon.AddCritChance(value);
                    break;
            }
        }

        /// <summary>核心机制升级分发</summary>
        private void ApplyCoreUpgrade(UpgradeConfig.UpgradeType type)
        {
            switch (type)
            {
                case UpgradeConfig.UpgradeType.DeathLight:
                    if (weapon != null) weapon.UnlockDeathLight();
                    break;
                case UpgradeConfig.UpgradeType.DeathDescend:
                    if (GetComponent<DeathDescendController>() == null)
                        gameObject.AddComponent<DeathDescendController>();
                    break;
            }
        }

        /// <summary>机制升级分发</summary>
        private void ApplyMechanicUpgrade(UpgradeConfig.UpgradeType type, int level)
        {
            switch (type)
            {
                // === 灵魂流 ===
                case UpgradeConfig.UpgradeType.SoulHarvest:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl == null) ctrl = gameObject.AddComponent<SoulController>();
                    ctrl.SetLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.SoulPower:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null) ctrl.SetPowerLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.SoulChain:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null) ctrl.SetChainLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.SoulSwarm:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null) ctrl.SetSwarmLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.SoulCurse:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null) ctrl.ActivateCurse();
                    break;
                }

                // === 收割流 ===
                case UpgradeConfig.UpgradeType.ScytheUnlock:
                {
                    var ctrl = GetComponent<ScytheController>();
                    if (ctrl == null) ctrl = gameObject.AddComponent<ScytheController>();
                    break;
                }
                case UpgradeConfig.UpgradeType.ScytheRange:
                {
                    var ctrl = GetComponent<ScytheController>();
                    if (ctrl != null) ctrl.SetRangeLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.ScytheDamage:
                {
                    var ctrl = GetComponent<ScytheController>();
                    if (ctrl != null) ctrl.SetDamageLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.ScytheSpeed:
                {
                    var ctrl = GetComponent<ScytheController>();
                    if (ctrl != null) ctrl.SetSpeedLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.Lifesteal:
                {
                    stats.AddLifesteal(UpgradeConfig.GetValue(type, level));
                    break;
                }

                // === 光束强化 ===
                case UpgradeConfig.UpgradeType.BeamCount:
                {
                    if (weapon != null)
                        weapon.SetBeamCount((int)UpgradeConfig.GetValue(type, level));
                    break;
                }
                case UpgradeConfig.UpgradeType.BeamRadius:
                {
                    if (weapon != null)
                        weapon.SetBeamRadius(UpgradeConfig.GetValue(type, level));
                    break;
                }
                case UpgradeConfig.UpgradeType.BeamRefraction:
                {
                    if (weapon != null)
                        weapon.SetBeamRefraction((int)UpgradeConfig.GetValue(type, level));
                    break;
                }

                // === 灵魂强化 ===
                case UpgradeConfig.UpgradeType.SoulMultiply:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null) ctrl.SetMultiplyLevel(level);
                    break;
                }
                case UpgradeConfig.UpgradeType.SoulExplosion:
                {
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null) ctrl.SetExplosionLevel(level);
                    break;
                }
            }
        }

        /// <summary>诅咒强力升级分发（跨流派，高收益高代价）</summary>
        private void ApplyCurseUpgrade(UpgradeConfig.UpgradeType type, int level)
        {
            switch (type)
            {
                case UpgradeConfig.UpgradeType.CurseDamage:
                {
                    // 全局伤害百分比加成
                    if (weapon != null)
                        weapon.AddDamagePercent(UpgradeConfig.GetValue(type, level));
                    break;
                }
                case UpgradeConfig.UpgradeType.CurseBeam:
                {
                    // 光束数量额外 +level，光束伤害 +level*20%
                    if (weapon != null)
                    {
                        weapon.SetBeamCount(weapon.beamCount + (int)UpgradeConfig.GetValue(type, level));
                        weapon.AddDamagePercent(UpgradeConfig.GetValue(type, level) * 0.2f);
                    }
                    break;
                }
                case UpgradeConfig.UpgradeType.CurseSoul:
                {
                    // 灵魂生成概率倍率：提升 harvestLevel 等效
                    var ctrl = GetComponent<SoulController>();
                    if (ctrl != null)
                        ctrl.SetMultiplyLevel(ctrl.GetMultiplyLevel() + (int)UpgradeConfig.GetValue(type, level));
                    break;
                }
                case UpgradeConfig.UpgradeType.CurseSurvival:
                {
                    // 最大生命 +50*level，护甲 +5*level
                    stats.AddMaxHP(UpgradeConfig.GetValue(type, level));
                    stats.AddArmor(level * 5);
                    break;
                }
            }
        }

        /// <summary>开局初始三选一</summary>
        public void ShowInitialChoice()
        {
            if (upgradeUI == null)
            {
                if (!uiMissingLogged)
                {
                    uiMissingLogged = true;
                    Debug.LogError("LevelUpManager 缺少 UpgradeUI 引用！");
                }
                return;
            }

            isChoosing = true;
            refreshCharges = MaxRefreshPerLevel;

            currentTypes.Clear();
            var choices = new List<UpgradeConfig.UpgradeDefinition>
            {
                new UpgradeConfig.UpgradeDefinition(UpgradeConfig.UpgradeType.Damage),
                new UpgradeConfig.UpgradeDefinition(UpgradeConfig.UpgradeType.SoulHarvest),
                new UpgradeConfig.UpgradeDefinition(UpgradeConfig.UpgradeType.MaxHP)
            };
            foreach (var c in choices) currentTypes.Add(c.type);

            upgradeUI.SetRefreshCharges(refreshCharges, OnRefreshClicked);
            upgradeUI.Show(choices, OnChoiceSelected);
        }

        private int[] BuildPickCounts()
        {
            int[] counts = new int[UpgradeConfig.TypeCount];
            for (int i = 0; i < counts.Length; i++)
                counts[i] = stats != null ? stats.GetPickCount((UpgradeConfig.UpgradeType)i) : 0;
            return counts;
        }
    }
}
