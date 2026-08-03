using System.Collections.Generic;
using UnityEngine;

namespace SurvivorDemo
{
    /// <summary>
    /// 升级流程管理器（挂在 Player 上）。
    /// 检测待处理升级次数，暂停游戏并弹出三选一界面，应用选择结果。
    /// 用 pendingLevelUps 计数 + 逐帧检查，不用事件总线。
    /// </summary>
    public class LevelUpManager : MonoBehaviour
    {
        /// <summary>玩家属性（升级次数、属性来源）</summary>
        public PlayerStats stats;

        /// <summary>玩家武器（强化攻击）</summary>
        public PlayerWeapon weapon;

        /// <summary>升级界面</summary>
        public UpgradeUI upgradeUI;

        /// <summary>当前是否正在选择中（防止同一帧重复弹出）</summary>
        private bool isChoosing;

        /// <summary>界面缺失错误只报一次，避免每帧刷屏</summary>
        private bool uiMissingLogged;

        private void Awake()
        {
            // 引用同挂载物体上的组件（GameSetup 已保证创建顺序）
            if (stats == null) stats = GetComponent<PlayerStats>();
            if (weapon == null) weapon = GetComponent<PlayerWeapon>();
            if (upgradeUI == null) upgradeUI = GetComponent<UpgradeUI>();
        }

        private void Update()
        {
            // 有待处理升级且当前没在选择中 → 触发升级
            if (!isChoosing && stats != null && stats.pendingLevelUps > 0)
            {
                // 玩家已死亡时不弹升级：GameOverUI 已接管结算，
                // 否则死亡当帧残留的待处理升级会在结算后再弹出一层升级面板
                if (stats.CurrentHP <= 0f)
                    return;

                StartLevelUp();
            }
        }

        /// <summary>
        /// 开始一次升级：暂停游戏，随机生成三张卡，交给界面展示。
        /// </summary>
        private void StartLevelUp()
        {
            // 界面缺失时无法展示，报错（只报一次）避免卡死
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

            // 暂停游戏：timeScale = 0 时其他脚本 deltaTime ≈ 0，天然静止
            Time.timeScale = 0f;

            // 随机生成 3 张卡，保证类型不重复
            List<UpgradeConfig.UpgradeDefinition> choices = new List<UpgradeConfig.UpgradeDefinition>();
            for (int i = 0; i < 3; i++)
            {
                UpgradeConfig.UpgradeDefinition choice;
                int attempts = 0;
                do
                {
                    choice = RollUsableChoice();
                    attempts++;
                }
                while (TypeAlreadyInList(choices, choice.type) && attempts < 10);

                choices.Add(choice);
            }

            // 交给界面展示，等待玩家选择
            upgradeUI.Show(choices, OnChoiceSelected);
        }

        /// <summary>
        /// 玩家选择一张卡后调用：应用强化 → 消耗次数 → 恢复游戏。
        /// 若还有待处理升级，下一帧 Update 会自动再弹。
        /// </summary>
        private void OnChoiceSelected(UpgradeConfig.UpgradeDefinition def)
        {
            // 按类型应用强化
            ApplyUpgrade(def);

            // 消耗一次待处理升级
            if (stats != null)
            {
                stats.ConsumePendingLevelUp();
            }

            // 玩家已在选择期间死亡（timeScale=0 暂停时物理回调仍会触发）
            // → 不恢复游戏，结算界面保持显示
            if (stats != null && stats.CurrentHP <= 0f)
                return;

            // 恢复游戏
            isChoosing = false;
            Time.timeScale = 1f;
        }

        /// <summary>
        /// 随机一张升级卡；若抽到攻速卡但攻速已到上限（零收益），重摇换成其他类型。
        /// 最多重摇 5 次，避免极端情况下无限循环。
        /// </summary>
        private UpgradeConfig.UpgradeDefinition RollUsableChoice()
        {
            for (int attempt = 0; attempt < 5; attempt++)
            {
                UpgradeConfig.UpgradeDefinition def = UpgradeConfig.RollChoice();
                bool fireRateAtCap = def.type == UpgradeConfig.UpgradeType.FireRate
                                     && weapon != null
                                     && weapon.IsFireRateAtCap();
                bool armorAtCap = def.type == UpgradeConfig.UpgradeType.Armor
                                  && stats != null
                                  && stats.IsArmorAtCap();
                if (!fireRateAtCap && !armorAtCap)
                    return def;
            }
            // 多次重摇仍未避开（理论上不会出现），接受最后一次结果
            




























































            return UpgradeConfig.RollChoice();
        }

        /// <summary>检查类型是否已在列表中（用于保证三张卡不重复）</summary>
        private static bool TypeAlreadyInList(List<UpgradeConfig.UpgradeDefinition> list, UpgradeConfig.UpgradeType type)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].type == type)
                    return true;
            }
            return false;
        }

        /// <summary>根据升级类型调用对应的强化方法</summary>
        private void ApplyUpgrade(UpgradeConfig.UpgradeDefinition def)
        {
            // 防御：属性缺失时直接返回（生命/护甲升级只需 stats，不需 weapon）
            if (stats == null)
                return;

            float value = UpgradeConfig.GetValue(def);

            switch (def.type)
            {
                case UpgradeConfig.UpgradeType.FireRate:
                    if (weapon != null) weapon.AddFireRateMultiplier(value);
                    break;

                case UpgradeConfig.UpgradeType.BulletCount:
                    if (weapon != null) weapon.AddBulletCount((int)value);
                    break;

                case UpgradeConfig.UpgradeType.Penetration:
                    if (weapon != null) weapon.AddPenetration((int)value);
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
            }
        }
    }
}
