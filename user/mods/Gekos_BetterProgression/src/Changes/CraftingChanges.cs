using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GekosBetterProgression.Changes;

internal class CraftingChanges()
{

    private static readonly List<MongoId> craftsToNotModify =
    [
        ItemTpl.BARTER_PHYSICAL_BITCOIN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_GEARCRATE_BLUE_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_GEARCRATE_GREEN_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_GEARCRATE_VIOLET_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_JEWELRYCRATE_BLUE_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_JEWELRYCRATE_GREEN_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_JEWELRYCRATE_VIOLET_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_JUNKCRATE_BLUE_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_JUNKCRATE_GREEN_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_JUNKCRATE_VIOLET_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_WEAPONCRATE_BLUE_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_WEAPONCRATE_GREEN_OPEN,
        ItemTpl.RANDOMLOOTCONTAINER_ARENA_WEAPONCRATE_VIOLET_OPEN,
        ItemTpl.DRINK_CANISTER_WITH_PURIFIED_WATER,
        ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE
    ];

    public static void Apply(Context context)
    {
        List<HideoutProduction>? crafts = context.tables.Hideout.Production.Recipes?.FindAll((production) => { return !craftsToNotModify.Contains(production.EndProduct); });

        if (crafts is null)
        {
            context.logger.Error("Failed to fetch hideout crafts");
            return;
        }

        float craftProductMultiplier = (float)context.config.misc.craftProductMultiplier;
        float craftTimeMultiplier = (float)context.config.misc.craftTimeMultiplier;

        foreach (var craft in crafts)
        {
            craft.Count *= Convert.ToInt32(craftProductMultiplier);
            craft.ProductionTime *= craftTimeMultiplier;
        }
        
    }
}
