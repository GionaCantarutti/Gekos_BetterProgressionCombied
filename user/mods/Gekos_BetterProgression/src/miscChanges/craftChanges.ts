import { ItemTpl } from "@spt/models/enums/ItemTpl";
import { Context } from "../contex";

export function changeCrafts(context: Context): void
{

    //Do not affect crafts that output these
    const forbiddenCrafts = [
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
    ] as string[];
    
    const config = context.config.misc;
    const crafts = context.tables.hideout.production.recipes;
    const nonBtc = crafts.filter((production) => !forbiddenCrafts.includes(production.endProduct));
    
    for (const craft of nonBtc)
    {
        craft.count *= config.craftProductMultiplier;
        craft.productionTime *= config.craftTimeMultiplier;
    }
}