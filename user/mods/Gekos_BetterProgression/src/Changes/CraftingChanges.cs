using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;

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
        ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE,
        ItemTpl.BARTER_LOCKED_EQUIPMENT_CRATE_BATTLEPASS_0,
        "674098588466ebb03408b210", //Opened Box
        "674078c4a9c9adf0450d59f9", //Opened Case


        //ToDo: perhaps add this part as configurable
        ItemTpl.KEYCARD_OBJECT_11SR,
        ItemTpl.KEYCARD_TERRAGROUP_LABS_KEYCARD_VIOLET,
        ItemTpl.KEYCARD_TERRAGROUP_LABS_KEYCARD_BLUE,
        ItemTpl.KEYCARD_TERRAGROUP_LABS_KEYCARD_GREEN,
        ItemTpl.KEYCARD_TERRAGROUP_LABS_KEYCARD_RED,
        ItemTpl.CONTAINER_MAGAZINE_CASE,
        ItemTpl.CONTAINER_LUCKY_SCAV_JUNK_BOX,
        ItemTpl.CONTAINER_GRENADE_CASE,
        ItemTpl.DRINK_BOTTLE_OF_WATER_06L
    ];

    public static bool Apply(Context context)
    {
        List<HideoutProduction>? crafts = context.tables.Hideout.Production.Recipes?.FindAll((production) => { return !craftsToNotModify.Contains(production.EndProduct); });

        if (crafts is null)
        {
            context.logger.Error("Failed to fetch hideout crafts");
            return false;
        }

        float craftProductMultiplier = (float)context.config.misc.craftProductMultiplier;
        float craftTimeMultiplier = (float)context.config.misc.craftTimeMultiplier;

        foreach (var craft in crafts)
        {
            craft.Count *= Convert.ToInt32(craftProductMultiplier);
            craft.ProductionTime *= craftTimeMultiplier;
        }

        return true;
        
    }
}
