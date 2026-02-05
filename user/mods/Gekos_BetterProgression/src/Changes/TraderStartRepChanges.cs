using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.Changes;

public static class TraderStartRepChanges
{ 
    public static void Apply(Context context)
    {
        double initialStanding = context.config.overrideInitialStanding.defaultOverride;

        foreach (KeyValuePair<string, ProfileSides> item in context.tables.Templates.Profiles)
        {
            foreach (var template in new TemplateSide[] { item.Value.Bear, item.Value.Usec })
            {
                template.Trader.InitialStanding["default"] = initialStanding;

                foreach (var traderId in context.tables.Traders.Keys)
                {
                    template.Trader.InitialStanding[traderId] = initialStanding;
                }

                foreach (var traderStanding in context.config.overrideInitialStanding.individualOverrides)
                {
                    template.Trader.InitialStanding[traderStanding.Key] = traderStanding.Value;
                }
            }
        }
    }
}