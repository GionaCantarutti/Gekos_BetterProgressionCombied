import { IBarterScheme } from "@spt/models/eft/common/tables/ITrader";
import { Context } from "../contex";

export function changePrices(context: Context): void
{

    const config = context.config.misc.priceChanges;

    for (const [itemId, change] of Object.entries(config as Record<string, number>))
    {
        //Set handbook price
        const handbookItem = context.tables.templates.handbook.Items.find((item) => item.Id === itemId);
        if (handbookItem == null) continue;
        handbookItem.Price = change;

        //Change trader assorts
        for (const trader of Object.values(context.tables.traders))
        {
            if (trader.assort == null) continue;
            const assorts = trader.assort.items.filter((item) => item._tpl == itemId);

            for (const assort of assorts)
            {
                const scheme: IBarterScheme = trader.assort.barter_scheme[assort._id][0][0];
                if (scheme == null) continue;
                if (scheme._tpl == "5449016a4bdc2d6f028b456f")
                {
                    scheme.count = change;
                }
            }
        }
    }

}