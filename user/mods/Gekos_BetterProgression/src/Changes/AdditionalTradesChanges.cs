using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression;

public static class AdditionalTradesChanges 
{
    public static void Apply(Context context)
    {
        foreach (var customTrade in context.advancedConfig.customTrades)
        {
            string traderId = customTrade.Key;
            string tradeId = customTrade.Value.barterScheme.First().Key; 
            Trader trader = context.tables.Traders[traderId];
            AdvancedConfig.CustomTrade trade = customTrade.Value;

            trader.Assort.Items.AddRange(trade.items);

            foreach (var barter in trade.barterScheme)
            {
                trader.Assort.BarterScheme.Add(barter.Key, barter.Value);
            }

            foreach (var loyalty in trade.loyalLevelItems)
            {
                trader.Assort.LoyalLevelItems.Add(loyalty.Key, loyalty.Value);
            }

            foreach (var questLock in trade.questLocks)
            {
                Utils.LockBehindQuest(context, traderId, tradeId, trade.items.First().Template, questLock.Value);
            }
        }
    }
}