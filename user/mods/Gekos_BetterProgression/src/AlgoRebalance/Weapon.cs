using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.AlgoRebalance;

public static class Weapon
{
    public static float CalculateWeaponLoyalty(Item item, List<Item> assort, Context context)
    {
        var config = context.config.algorithmicalRebalancing.weaponRules;

        var itemTemplate = context.tables.Templates.Items[item.Template];
        var fireMode = Utils.BestFiremode(itemTemplate);
        var fireRate = itemTemplate.Properties.BFirerate;
        float loyalty = (float)config.defaultBaseLoyalty;

        // Base loyalty level based on caliber
        foreach (var byCaliber in config.weaponBaseLoyaltyByCaliber)
        {
            if (byCaliber.caliber == itemTemplate.Properties.AmmoCaliber) loyalty = (float)byCaliber.baseLoyalty;
        }

        // Account for best available fire mode
        foreach (var byMode in config.fireModeRules)
        {
            if (byMode.mode == fireMode) loyalty += (float)byMode.delta;
        }

        // Account for fire rate if applicable
        if (fireMode == "fullauto" || fireMode == "burst")
        {
            foreach (var byRate in config.fireRateRules)
            {
                if (fireRate >= byRate.rateInterval[0] && fireRate < byRate.rateInterval[1]) loyalty += (float)byRate.delta;
            }
        }

        return loyalty + (float)config.globalDelta;
    }

    // Move attachments that are part of some default build to the same tier of the weapon they are part of the build for
    public static void FollowDefaultBuild(Dictionary<int, List<ChangedItem>> changedItems, Context context)
    {
        var changesById = Utils.IndexById(changedItems);

        foreach (var weaponChange in changesById.Values)
        {
            if (!weaponChange.isWeapon) continue;

            var parts = Utils.GetDefaultAttachments(weaponChange.trade.Template, context);

            foreach (var part in parts)
            {
                var level = Utils.LoyaltyFromScore(weaponChange.score, context.config.algorithmicalRebalancing.clampToMaxLevel);
                var partTrades = Utils.FindTrades(part, context);

                foreach (var partTrade in partTrades)
                {
                    changesById.TryGetValue(partTrade.trade.Id.ToString(), out var oldPartChange);
                    if (oldPartChange != null)
                    {
                        var oldLevel = Utils.LoyaltyFromScore(oldPartChange.score, context.config.algorithmicalRebalancing.clampToMaxLevel);
                        if (oldLevel <= level)
                        {
                            continue;
                        }
                        else
                        {
                            if (changedItems.ContainsKey(oldLevel))
                            {
                                changedItems[oldLevel] = changedItems[oldLevel].Where(item => item.trade.Id != partTrade.trade.Id).ToList();
                            }
                        }
                    }

                    if (!changedItems.ContainsKey(level)) changedItems[level] = new List<ChangedItem>();

                    changedItems[level].Add(new ChangedItem(partTrade.trade, weaponChange.score, partTrade.trader, false, false));
                }
            }
        }
    }

    public static void PenalizeAdvancedAttachments(Dictionary<int, List<ChangedItem>> changedItems, Context context)
    {
        var config = context.config.algorithmicalRebalancing;
        var toPenalize = new List<(ChangedItem change, int tier)>();

        foreach (var kvp in changedItems)
        {
            var tier = kvp.Key;
            var changes = kvp.Value;
            if (changes == null) continue;

            for (int i = 0; i < changes.Count; i++)
            {
                var change = changes[i];
                if (!change.isWeapon) continue;

                if (!Utils.CanAllAttachmentsBePurchased(
                    change.trade,
                    change.trader.Assort.Items,
                    true,
                    true,
                    tier,
                    Utils.GetDefaultAttachments(change.trade.Template, context),
                    Utils.IndexById(changedItems),
                    context))
                {
                    toPenalize.Add((change, tier));
                }
            }
        }

        foreach (var entry in toPenalize)
        {
            var change = entry.change;
            var tier = entry.tier;

            if (changedItems.ContainsKey(tier))
            {
                changedItems[tier] = changedItems[tier].Where(item => item.trade.Id != change.trade.Id).ToList();
            }

            change.score += (float)config.weaponRules.advancedAttachmentsDelta;
            var newLevel = Utils.LoyaltyFromScore(change.score, config.clampToMaxLevel);
            if (!changedItems.ContainsKey(newLevel)) changedItems[newLevel] = new List<ChangedItem>();
            changedItems[newLevel].Add(change);
        }
    }

    public static void WeaponShifting(Dictionary<int, List<ChangedItem>> changedItems, Context context)
    {
        var config = context.config.algorithmicalRebalancing;
        var reverse = config.weaponRules.upshiftRules.shiftDownInstead;

        var keys = changedItems.Keys.OrderBy(k => k).ToList();

        for (int idx = 0; idx < keys.Count; idx++)
        {
            int index = reverse ? keys.Count - idx - 1 : idx;
            var levelKey = keys[index];
            var changesInLevel = changedItems.ContainsKey(levelKey) ? changedItems[levelKey] : null;

            if (changesInLevel == null || changesInLevel.Count == 0) continue;

            var toShift = new HashSet<int>();

            for (int i = 0; i < changesInLevel.Count; i++)
            {
                if (toShift.Contains(i)) continue;
                if (!changesInLevel[i].isWeapon) continue;
                for (int j = i + 1; j < changesInLevel.Count; j++)
                {
                    if (toShift.Contains(j)) continue;
                    if (!changesInLevel[j].isWeapon) continue;

                    var a = changesInLevel[i];
                    var b = changesInLevel[j];

                    if (Utils.ShareSameNiche(a.trade, b.trade, a.trader, b.trader, context))
                    {
                        if (!config.weaponRules.upshiftRules.powerLevels.TryGetValue(a.trade.Template, out var aPowerLevel)) continue;
                        if (!config.weaponRules.upshiftRules.powerLevels.TryGetValue(b.trade.Template, out var bPowerLevel)) continue;
                        if (aPowerLevel == bPowerLevel) continue;

                        if (aPowerLevel < bPowerLevel)
                        {
                            toShift.Add(reverse ? i : j);
                        }
                        else
                        {
                            toShift.Add(reverse ? j : i);
                        }
                    }
                }
            }

            foreach (var shiftIndex in toShift.OrderBy(x => x))
            {
                if (shiftIndex < 0 || shiftIndex >= changesInLevel.Count) continue;
                var change = changesInLevel[shiftIndex];
                change.score += config.weaponRules.upshiftRules.shiftAmount * (reverse ? -1 : 1);
                var newLevel = Utils.LoyaltyFromScore(change.score, config.clampToMaxLevel);
                changedItems[levelKey] = changedItems[levelKey].Where(item => item.trade.Id != change.trade.Id).ToList();
                if (!changedItems.ContainsKey(newLevel)) changedItems[newLevel] = new List<ChangedItem>();
                changedItems[newLevel].Add(change);
            }
        }
    }
}
