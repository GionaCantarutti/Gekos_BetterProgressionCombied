using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.Changes;

public class FleaChanges()
{
    public static bool Apply(Context context)
    {
        Dictionary<MongoId, TemplateItem> allItems = context.tables.Templates.Items;

        bool allowKeys = context.config.fleaMarketChanges.stillAllowKeys;

        foreach (KeyValuePair<MongoId, TemplateItem> item in allItems)
        {
            bool allowed = context.config.fleaMarketChanges.fleaWhitelist.Contains(item.Key);
            
            bool isAKey = context.itemHelper.IsOfBaseclass(item.Key, BaseClasses.KEY);
            bool isSoldOnFleaByDefault = item.Value.Properties.CanSellOnRagfair.Equals(true); //ToDo: config option to ignore this?

            if (allowKeys && isAKey && isSoldOnFleaByDefault)
            {
                allowed = true;
            }

            item.Value.Properties.CanRequireOnRagfair = allowed && item.Value.Properties.CanRequireOnRagfair.Equals(true);
            item.Value.Properties.CanSellOnRagfair = allowed;
        }

        return true;

    }
}
