using System;
using System.Collections.Generic;
using System.Linq;
using GekosBetterProgression;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace GekosBetterProgression.AlgoRebalance;

public static class Core
{
    public static void AlgorithmicallyRebalance(Context context)
    {
        var config = context.config.algorithmicalRebalancing;

        var itemHelper = context.itemHelper;
        var traders = context.tables.Traders.Values;

        var changedItems = new Dictionary<int, List<ChangedItem>>();

        foreach (var trader in traders)
        {
            var loyaltyLevels = trader?.Assort?.LoyalLevelItems;
            if (loyaltyLevels == null) continue;

            var itemsForSale = trader?.Assort?.Items;
            if (itemsForSale == null) continue;

            if (config.excludeTraders.Contains(trader!.Base.Id)) continue;

            foreach (var item in itemsForSale)
            {
                if (config.explicitLoyaltyOverride.trades.ContainsKey(item.Id)) continue;
                if (config.explicitLoyaltyOverride.items.ContainsKey(item.Template)) continue;

                ChangedItem thisItem = null;

                // AMMO
                if (config.ammoRules.enable)
                {
                    bool ammoOrBox = false;
                    string ammo = null;
                    float loyaltyScore = 0f;

                    if (itemHelper.IsOfBaseclass(item.Template, BaseClasses.AMMO))
                    {
                        loyaltyScore = Ammo.CalculateAmmoLoyalty(item, context);
                        ammo = item.Template;
                        ammoOrBox = true;
                    }
                    else if (itemHelper.IsOfBaseclass(item.Template, BaseClasses.AMMO_BOX))
                    {
                        try
                        {
                            dynamic tpl = context.tables.Templates.Items[item.Template];
                            // Attempt to follow legacy structure: StackSlots[0]._props.filters[0].Filter[0]
                            ammo = (string)tpl.Properties.StackSlots[0].Props.filters[0].Filter[0];
                            loyaltyScore = Ammo.ScoreAmmo(context.tables.Templates.Items[ammo], context);
                            ammoOrBox = true;
                        }
                        catch
                        {
                            ammoOrBox = false;
                        }
                    }

                    if (ammoOrBox && !config.ammoRules.ignoreCalibers.Contains(context.tables.Templates.Items[ammo].Properties.Caliber))
                    {
                        thisItem = new ChangedItem(item, loyaltyScore, trader, config.ammoRules.logChanges, false);
                    }
                }

                // WEAPONS
                if (config.weaponRules.enable
                    && itemHelper.IsOfBaseclass(item.Template, BaseClasses.WEAPON)
                    && !itemHelper.IsOfBaseclass(item.Template, BaseClasses.SPECIAL_WEAPON))
                {
                    var loyaltyScore = Weapon.CalculateWeaponLoyalty(item, itemsForSale, context);
                    thisItem = new ChangedItem(item, loyaltyScore, trader, config.weaponRules.logChanges, true);
                }

                if (thisItem != null)
                {
                    if (Utils.IsQuestLocked(thisItem.trade, thisItem.trader, context))
                    {
                        thisItem.score += (float)config.questLockDelta;
                        if (config.logBartersAndLocks) context.logger.Info(context.tables.Templates.Items[thisItem.trade.Template].Name + " is a quest-locked item\t(Trade ID: " + thisItem.trade.Id + ")");
                    }
                    if (Utils.IsBarterTrade(thisItem.trade, thisItem.trader))
                    {
                        thisItem.score += (float)config.barterDelta;
                        if (config.logBartersAndLocks) context.logger.Info(context.tables.Templates.Items[thisItem.trade.Template].Name + " is a bartered item\t(Trade ID: " + thisItem.trade.Id + ")");
                    }

                    if (config.deltaByTrader.ContainsKey(trader.Base.Id)) thisItem.score += (float)config.deltaByTrader[trader.Base.Id];

                    if (config.explicitLoyaltyDelta.trades.TryGetValue(thisItem.trade.Id, out var tradeD)) thisItem.score += (float)tradeD;
                    if (config.explicitLoyaltyDelta.items.TryGetValue(thisItem.trade.Template, out var itemD)) thisItem.score += (float)itemD;

                    var level = Utils.LoyaltyFromScore(thisItem.score, config.clampToMaxLevel);
                    if (!changedItems.ContainsKey(level)) changedItems[level] = new List<ChangedItem>();
                    changedItems[level].Add(thisItem);
                }
            }
        }

        if (config.weaponRules.upshiftRules.enable) Weapon.WeaponShifting(changedItems, context);
        if (config.weaponRules.attachmentsFollowDefaultBuild) Weapon.FollowDefaultBuild(changedItems, context);
        if (config.weaponRules.advancedAttachmentsDelta != 0) Weapon.PenalizeAdvancedAttachments(changedItems, context);

        // Apply changes
        foreach (var changesInLevel in changedItems.Values)
        {
            if (changesInLevel == null || changesInLevel.Count == 0) continue;
            foreach (var changedItem in changesInLevel)
            {
                bool doClamp = config.clampToMaxLevel;
                if (config.forceClampingOfQuestlockedItems && Utils.IsQuestLocked(changedItem.trade, changedItem.trader, context)) doClamp = true;
                if (changedItem.logChange) context.logger.Info($"Setting {context.tables.Templates.Items[changedItem.trade.Template].Name} at loyalty level {Utils.LoyaltyFromScore(changedItem.score, doClamp)} ({changedItem.score})");
                Utils.SetLoyalty(changedItem.trade.Id, changedItem.score, changedItem.trader, doClamp);
            }
        }

        // Overrides
        foreach (var trader in traders)
        {
            var loyaltyLevels = trader?.Assort?.LoyalLevelItems;
            if (loyaltyLevels == null) continue;

            var itemsForSale = trader?.Assort?.Items;
            if (itemsForSale == null) continue;

            if (config.excludeTraders.Contains(trader.Base.Id)) continue;

            foreach (var item in itemsForSale)
            {
                int? overrideVal = null;
                if (config.explicitLoyaltyOverride.trades.TryGetValue(item.Id, out var t)) overrideVal = t;
                if (overrideVal == null && config.explicitLoyaltyOverride.items.TryGetValue(item.Template, out var it)) overrideVal = it;
                if (overrideVal == null) continue;

                Utils.SetLoyalty(item.Id, overrideVal.Value, trader, config.clampToMaxLevel);
            }
        }

        if (config.ammoRules.craftSettings.enable) Ammo.RebalanceAmmoCrafts(context);
    }
}
