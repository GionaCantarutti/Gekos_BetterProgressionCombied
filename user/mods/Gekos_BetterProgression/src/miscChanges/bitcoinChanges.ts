import { ItemTpl } from "@spt/models/enums/ItemTpl";
import { Context } from "../contex";
import { isBarterTrade } from "../utils";

export function changeBitcoinFarming(context: Context): void
{
    const config = context.config.bitcoinChanges;

    //Set bitcoin value
    if (config.overrideValue)
    {
        context.tables.templates.handbook.Items.find((item) => item.Id === ItemTpl.BARTER_PHYSICAL_BITCOIN).Price = config.value;
    }

    //Multiply btc production rate
    const btcProds = context.tables.hideout.production.recipes.filter((production) => production.endProduct === ItemTpl.BARTER_PHYSICAL_BITCOIN);
    for (const p of btcProds)
    {
        p.productionTime = Math.round(p.productionTime / config.btcFarmSpeedMult);
        p.productionLimitCount = config.btcCapacity;
    }

    //Apply boost rate
    context.tables.hideout.settings.gpuBoostRate = config.gpuBoostRate;

    //Remove all non-barters for GPUs
    if (config.cannotBuyGPU) for (const trader of Object.values(context.tables.traders))
    {
        if (trader.assort == null) continue;
        trader.assort.items = trader.assort.items.filter(assort => assort._tpl != "57347ca924597744596b4e71" || isBarterTrade(assort, trader) );
    }
}