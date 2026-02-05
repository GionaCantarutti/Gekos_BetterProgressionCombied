using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.Changes;

public static class AdditionalItemsChanges 
{
    public static void Apply(Context context)
    {
        Dictionary<string, IEnumerable<Buff>> buffDatabase = context.tables.Globals.Configuration.Health.Effects.Stimulator.Buffs;
        Dictionary<MongoId, TemplateItem> itemDatabase = context.tables.Templates.Items;

        foreach (var buff in context.advancedConfig.customBuffs)
        {
            buffDatabase[buff.Key] = buff.Value;
        }

        foreach (var item in context.advancedConfig.customItems)
        {
            itemDatabase[item.Key] = item.Value;
        }

        foreach (var item in context.advancedConfig.customLocales)
        {
            Utils.AddToLocale(context, item.Key, item.Value);
        }

        // rest of the legacy code then adds the items, using code identical to customTrades.ts
        // assuming that's a mistake and only one of the code paths run
    }
}
