import fs from "fs";
// eslint-disable-next-line @typescript-eslint/naming-convention
import JSON5 from "json5";
import { Context } from "../contex";
import { addToLocale, getModFolder } from "../utils";
import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem";
import { IItem } from "@spt/models/eft/common/tables/IItem";
import { IBarterScheme } from "@spt/models/eft/common/tables/ITrader";

export function addNewItems(context: Context): void
{
    //Read custom items config
    const fileContent = fs.readFileSync(getModFolder() + "/config/advanced/customItems.json5", "utf-8");
    const itemsConfig = JSON5.parse(fileContent);

    const buffs = context.tables.globals.config.Health.Effects.Stimulator.Buffs;
    const items = context.tables.templates.items;
    const locales = context.tables.locales.global;
    
    //Add the buffs
    for (const [buff, effects] of Object.entries(itemsConfig.customBuffs))
    {
        buffs[buff] = effects;
    }

    //Add the items
    for (const [id, item] of Object.entries(itemsConfig.customItems as Record<string, ITemplateItem>))
    {
        items[id] = item;
    }

    //Add the locales
    for (const [item, locale] of Object.entries(itemsConfig.customLocales as Record<string, {Name: string, ShortName: string, Description: string}>))
    {
        addToLocale(locales, item, locale.Name, locale.ShortName, locale.Description)
    }

    //Add the trades
    for (const [traderID, additions] of
        Object.entries(itemsConfig.customTrades as 
            Record< 
            // eslint-disable-next-line @typescript-eslint/naming-convention
            string, {items: IItem[], barter_scheme: Record<string, IBarterScheme[][]>, loyal_level_items: Record<string, number>}
            >
        )
    )
    {
        const trader = context.tables.traders[traderID];
        for (const trade of additions.items)
        {
            trader.assort.items.push(trade);
        }

        for (const [trade, scheme] of Object.entries(additions.barter_scheme))
        {
            trader.assort.barter_scheme[trade] = scheme;
        }

        for (const [trade, loyalty] of Object.entries(additions.loyal_level_items))
        {
            trader.assort.loyal_level_items[trade] = loyalty;
        }
    }
}