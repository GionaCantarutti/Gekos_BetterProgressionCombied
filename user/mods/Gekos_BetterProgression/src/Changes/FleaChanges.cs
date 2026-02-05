using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Helpers;

namespace GekosBetterProgression.Changes;

public class FleaChanges()
{
    public static void Apply(Context context)
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

    }
}
