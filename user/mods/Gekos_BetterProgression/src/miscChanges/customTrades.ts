import fs from "fs";
// eslint-disable-next-line @typescript-eslint/naming-convention
import JSON5 from "json5";
import { Context } from "../contex";
import { getModFolder, lockBehindQuest } from "../utils";
import { IItem } from "@spt/models/eft/common/tables/IItem";
import { IBarterScheme } from "@spt/models/eft/common/tables/ITrader";

export function addCustomTrades(context: Context): void
{
    //Read custom trades config
    const fileContent = fs.readFileSync(getModFolder() + "/config/advanced/customTrades.json5", "utf-8");
    const tradesConfig = JSON5.parse(fileContent);

    for (const [traderID, additions] of
        Object.entries(tradesConfig as 
            Record< 
            string, {
                items: Record<string, IItem>,
                // eslint-disable-next-line @typescript-eslint/naming-convention
                barter_scheme: Record<string, IBarterScheme[][]>,
                // eslint-disable-next-line @typescript-eslint/naming-convention
                loyal_level_items: Record<string, number>,
                questlocks: Record<string, {quest: string, rewardId: string, targetId: string}>
            }
            >
        )
    )
    {
        const trader = context.tables.traders[traderID];
        for (const trade of Object.values(additions.items))
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

        for (const [trade, lockProps] of Object.entries(additions.questlocks))
        {
            lockBehindQuest(context, traderID, trade, lockProps.quest, additions.items[trade]._tpl, lockProps.rewardId, lockProps.targetId);
        }
    }
}