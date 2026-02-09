using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.AlgoRebalance;

public static class Core
{
    //Main entrypoint
    public static bool AlgorithmicallyRebalance(Context context)
    {
        var traders = context.tables.Traders.Values;

        var changedItems = new Dictionary<int, List<ChangedItem>>();

        foreach (var trader in traders)
        {
            ProcessTrader(trader, context, changedItems);
        }

        PostProcessChangedItems(changedItems, context);

        ApplyChanges(changedItems, context);

        ApplyOverrides(context);

        if (context.config.algorithmicalRebalancing.ammoRules.craftSettings.enable) Ammo.RebalanceAmmoCrafts(context);

        return true;
    }

    private static void ProcessTrader(Trader trader, Context context, Dictionary<int, List<ChangedItem>> changedItems)
    {
        var cfg = context.config.algorithmicalRebalancing;

        if (trader is null || trader.Base is null) return;
        var loyaltyLevels = trader.Assort?.LoyalLevelItems;
        if (loyaltyLevels is null) return;

        var itemsForSale = trader.Assort?.Items;
        if (itemsForSale is null) return;

        if (cfg.excludeTraders.Contains(trader.Base.Id)) return;

        foreach (var item in itemsForSale)
        {
            if (cfg.explicitLoyaltyOverride.trades.ContainsKey(item.Id)) continue;
            if (cfg.explicitLoyaltyOverride.items.ContainsKey(item.Template)) continue;

            var changed = CreateChangedItemForItem(item, trader, itemsForSale, context);
            if (changed is null) continue;

            // Final modifications
            if (Utils.IsQuestLocked(changed.trade, changed.trader, context))
            {
                changed.score += (float)cfg.questLockDelta;
                if (cfg.logBartersAndLocks) context.logger.Info(context.tables.Templates.Items[changed.trade.Template].Name + " is a quest-locked item\t(Trade ID: " + changed.trade.Id + ")");
            }

            if (Utils.IsBarterTrade(changed.trade, changed.trader))
            {
                changed.score += (float)cfg.barterDelta;
                if (cfg.logBartersAndLocks) context.logger.Info(context.tables.Templates.Items[changed.trade.Template].Name + " is a bartered item\t(Trade ID: " + changed.trade.Id + ")");
            }

            if (cfg.deltaByTrader.ContainsKey(trader.Base.Id)) changed.score += (float)cfg.deltaByTrader[trader.Base.Id];

            if (cfg.explicitLoyaltyDelta.trades.TryGetValue(changed.trade.Id, out var tradeD)) changed.score += (float)tradeD;
            if (cfg.explicitLoyaltyDelta.items.TryGetValue(changed.trade.Template, out var itemD)) changed.score += (float)itemD;

            var level = Utils.LoyaltyFromScore(changed.score, cfg.clampToMaxLevel);
            if (!changedItems.ContainsKey(level)) changedItems[level] = new List<ChangedItem>();
            changedItems[level].Add(changed);
        }
    }

    private static ChangedItem? CreateChangedItemForItem(Item item, Trader trader, IEnumerable<Item> itemsForSale, Context context)
    {
        var cfg = context.config.algorithmicalRebalancing;
        var itemHelper = context.itemHelper;

        ChangedItem? thisItem = null;

        // AMMO
        if (cfg.ammoRules.enable)
        {
            bool ammoOrBox = false;
            string? ammo = null;
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
                    ammo = (string)tpl.Properties.StackSlots[0].Props.filters[0].Filter[0];
                    loyaltyScore = Ammo.ScoreAmmo(context.tables.Templates.Items[ammo], context);
                    ammoOrBox = true;
                }
                catch
                {
                    ammoOrBox = false;
                }
            }

            if (ammoOrBox && ammo != null)
            {
                if (context.tables.Templates.Items.TryGetValue(ammo, out var tpl) && tpl.Properties != null)
                {
                    if (tpl.Properties.Caliber != null && !cfg.ammoRules.ignoreCalibers.Contains(tpl.Properties.Caliber))
                    {
                        thisItem = new ChangedItem(item, loyaltyScore, trader, cfg.ammoRules.logChanges, false);
                    }
                }
            }
        }

        // WEAPONS
        if (cfg.weaponRules.enable
            && itemHelper.IsOfBaseclass(item.Template, BaseClasses.WEAPON)
            && !itemHelper.IsOfBaseclass(item.Template, BaseClasses.SPECIAL_WEAPON))
        {
            var loyaltyScore = Weapon.CalculateWeaponLoyalty(item, itemsForSale.ToList(), context);
            thisItem = new ChangedItem(item, loyaltyScore, trader, cfg.weaponRules.logChanges, true);
        }

        return thisItem;
    }

    private static void PostProcessChangedItems(Dictionary<int, List<ChangedItem>> changedItems, Context context)
    {
        var cfg = context.config.algorithmicalRebalancing;

        if (cfg.weaponRules.upshiftRules.enable) Weapon.WeaponShifting(changedItems, context);
        if (cfg.weaponRules.attachmentsFollowDefaultBuild) Weapon.FollowDefaultBuild(changedItems, context);
        if (cfg.weaponRules.advancedAttachmentsDelta != 0) Weapon.PenalizeAdvancedAttachments(changedItems, context);
    }

    private static void ApplyChanges(Dictionary<int, List<ChangedItem>> changedItems, Context context)
    {
        var cfg = context.config.algorithmicalRebalancing;

        foreach (var changesInLevel in changedItems.Values)
        {
            if (changesInLevel == null || changesInLevel.Count == 0) continue;
            foreach (var changedItem in changesInLevel)
            {
                bool doClamp = cfg.clampToMaxLevel;
                if (cfg.forceClampingOfQuestlockedItems && Utils.IsQuestLocked(changedItem.trade, changedItem.trader, context)) doClamp = true;
                if (changedItem.logChange) context.logger.Info($"Setting {context.tables.Templates.Items[changedItem.trade.Template].Name} at loyalty level {Utils.LoyaltyFromScore(changedItem.score, doClamp)} ({changedItem.score})");
                Utils.SetLoyalty(changedItem.trade.Id, changedItem.score, changedItem.trader, doClamp);
            }
        }
    }

    private static void ApplyOverrides(Context context)
    {
        var cfg = context.config.algorithmicalRebalancing;
        var traders = context.tables.Traders.Values;

        foreach (var trader in traders)
        {
            if (trader == null || trader.Base == null) continue;
            var loyaltyLevels = trader.Assort?.LoyalLevelItems;
            if (loyaltyLevels == null) continue;

            var itemsForSale = trader.Assort?.Items;
            if (itemsForSale == null) continue;

            if (cfg.excludeTraders.Contains(trader.Base.Id)) continue;

            foreach (var item in itemsForSale)
            {
                int? overrideVal = null;
                if (cfg.explicitLoyaltyOverride.trades.TryGetValue(item.Id, out var t)) overrideVal = t;
                if (overrideVal == null && cfg.explicitLoyaltyOverride.items.TryGetValue(item.Template, out var it)) overrideVal = it;
                if (overrideVal == null) continue;

                Utils.SetLoyalty(item.Id, overrideVal.Value, trader, cfg.clampToMaxLevel);
            }
        }
    }
}
