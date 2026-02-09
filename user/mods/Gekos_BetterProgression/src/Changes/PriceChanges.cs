using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.Changes;

public static class PriceChanges
{
    public static bool Apply(Context context)
    {
        foreach (var priceChange in context.config.misc.priceChanges)
        {
            var handbookItem = context.tables.Templates.Handbook.Items.Find((i) => i.Id == priceChange.Key);
            if (handbookItem == null)
            {
                continue;
            }
            handbookItem.Price = priceChange.Value;

            foreach (var trader in context.tables.Traders)
            {
                if (trader.Value.Assort == null)
                {
                    continue;
                }

                List<Item> assorts = trader.Value.Assort.Items.FindAll((i) => i.Template == priceChange.Key);
                foreach (Item assort in assorts)
                {
                    BarterScheme scheme = trader.Value.Assort.BarterScheme[assort.Id][0][0];
                    if (scheme == null)
                    {
                        continue;
                    }
                    // could probably store a static in util called ROUBLES or something that just contains this ID
                    if (scheme.Template == "5449016a4bdc2d6f028b456f")
                    {
                        scheme.Count = priceChange.Value;
                    }
                }
            }
        }

        return true;
    }
}