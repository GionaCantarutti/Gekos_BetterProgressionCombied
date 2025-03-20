import { BaseClasses } from "@spt/models/enums/BaseClasses";
import { Context } from "../contex";

export function disableFleaMarket(context: Context): void
{
    const allItems = context.tables.templates.items;
    const config = context.config.fleaMarketChanges;

    for (const [id, item] of Object.entries(allItems))
    {
        let allowed: boolean = config.fleaWhitelist.includes(id);
        if (config.stillAllowKeys && context.itemHelper.isOfBaseclass(id, BaseClasses.KEY) && item._props.CanSellOnRagfair) allowed = true;
        item._props.CanRequireOnRagfair = allowed && item._props.CanRequireOnRagfair;
        item._props.CanSellOnRagfair = allowed;
    }
}