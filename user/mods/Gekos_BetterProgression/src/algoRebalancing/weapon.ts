import { IItem } from "@spt/models/eft/common/tables/IItem";
import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem";
import { Context } from "../contex";
import { bestFiremode, canAllAttachmentsBePurchased, findTrades, getDefaultAttachments, indexById, loyaltyFromScore, shareSameNiche } from "../utils";
import { ChangedItem } from "./types";

export function calculateWeaponLoyalty(item: IItem, assort: IItem[], context: Context): number
{
    const config = context.config.algorithmicalRebalancing.weaponRules;
    const tables = context.tables;

    const itemTemplate: ITemplateItem = tables.templates.items[item._tpl];
    const fireMode = bestFiremode(itemTemplate);
    const fireRate = itemTemplate._props.bFirerate;
    let loyalty: number = config.defaultBaseLoyalty;

    //Base loyalty level based on caliber
    for (const byCaliber of config.weaponBaseLoyaltyByCaliber)
    {
        if (byCaliber.caliber == itemTemplate._props.ammoCaliber) loyalty = byCaliber.baseLoyalty;
    }

    //Account for best available fire mode
    for (const byMode of config.fireModeRules)
    {
        if (byMode.mode == fireMode) loyalty += byMode.delta;
    }

    //Account for fire rate if applicable
    if (fireMode == "fullauto" || fireMode == "burst") for (const byRate of config.fireRateRules)
    {
        if (fireRate >= byRate.rateInterval[0] && fireRate < byRate.rateInterval[1]) loyalty += byRate.delta;
    }

    return loyalty + config.globalDelta;
}

export function followDefaultBuild(changedItems: Record<number, ChangedItem[]>, context: Context): void
{

    const changesById = indexById(changedItems);

    for (const changes of Object.values(changedItems)) for (const change of Object.values(changes))
    {
        if (!change.isWeapon) continue;

        const parts = getDefaultAttachments(change.trade._tpl, context);

        for (const part of parts)
        {
            const level = loyaltyFromScore(change.score, context.config.algorithmicalRebalancing.clampToMaxLevel);
            const partTrades = findTrades(part, context);

            for (const trade of partTrades)
            {
                const oldChange = changesById[trade.trade._id];
                if (oldChange != null)
                {
                    const oldLevel = loyaltyFromScore(oldChange.score, context.config.algorithmicalRebalancing.clampToMaxLevel);
                    if (oldLevel <= level)
                    {
                        continue;
                    }
                    else
                    {
                        changedItems[oldLevel].filter((item) => item.trade._id != trade.trade._id)
                    }
                }
                changedItems[level].push({
                    trade: trade.trade,
                    score: change.score,
                    trader: trade.trader,
                    logChange: false,
                    isWeapon: false
                })
            }
        }
    }
}

export function penalizeAdvancedAttachments(changedItems: Record<number, ChangedItem[]>, context: Context): void
{
    const config = context.config.algorithmicalRebalancing;
    const toPenalize: [ChangedItem, number][] = [];

    for (const [tier, changes] of Object.entries(changedItems)) for (const change of changes)
    {
        if (!change.isWeapon) continue;

        if (!canAllAttachmentsBePurchased(
            change.trade, change.trader.assort.items, true, true, Number(tier),
            getDefaultAttachments(change.trade._tpl, context), indexById(changedItems),
            context))
        {
            toPenalize.push([change, Number(tier)]);
            //context.logger.info(`Penalizing ${context.tables.templates.items[change.trade._tpl]._name}`);
        }
    }

    for (const [change, tier] of toPenalize)
    {
        changedItems[tier] = changedItems[tier].filter((item) => item.trade._id != change.trade._id);
        change.score += config.weaponRules.advancedAttachmentsDelta;
        const newLevel = loyaltyFromScore(change.score, config.clampToMaxLevel);
        if (changedItems[newLevel] == null)
        {
            changedItems[newLevel] = [change];
        }
        else
        {
            changedItems[newLevel].push(change);
        }
    }
}

export function weaponShifting(changedItems: { [level: number] : ChangedItem[] }, context: Context): void
{

    const config = context.config.algorithmicalRebalancing;
    const reverse = config.weaponRules.upshiftRules.shiftDownInstead;

    //Shifting system
    //WARNING!!! HORRIBLE CODE AHEAD!!!
    const keys = Object.keys(changedItems).sort();
    for (let i = 0; i < keys.length; i++)
    {
        const index = reverse ? keys.length - i - 1 : i; //Reverse order
        const changesInLevel = changedItems[keys[index]];

        if (changesInLevel == null || changesInLevel.length == 0) continue;

        const toShift: Set<number> = new Set<number>();

        for (let i = 0; i < changesInLevel.length; i++)
        {
            if (toShift.has(i)) continue;
            if (!changesInLevel[i].isWeapon) continue;
            for (let j = i + 1; j < changesInLevel.length; j++)
            {
                if (toShift.has(j)) continue;
                if (!changesInLevel[j].isWeapon) continue;

                const a = changesInLevel[i]; const b = changesInLevel[j];

                if (shareSameNiche(a.trade, b.trade, a.trader, b.trader, context))
                {
                    const aPowerLevel = config.weaponRules.upshiftRules.powerLevels[a.trade._tpl];
                    const bPowerLevel = config.weaponRules.upshiftRules.powerLevels[b.trade._tpl];

                    if (aPowerLevel == null || bPowerLevel == null || aPowerLevel == bPowerLevel) continue;

                    if (aPowerLevel < bPowerLevel)
                    {
                        toShift.add(reverse ? i : j);
                    }
                    else
                    {
                        toShift.add(reverse ? j : i);
                    }
                }

            }
        }
        
        for (const shiftIndex of toShift)
        {
            const change = changesInLevel[shiftIndex];
            change.score += config.weaponRules.upshiftRules.shiftAmount * (reverse ? -1 : 1);
            const newLevel = loyaltyFromScore(change.score, config.clampToMaxLevel);
            changedItems[keys[index]] = changedItems[keys[index]].filter(item => item.trade._id != change.trade._id);
            if (changedItems[newLevel] == null)
            {
                changedItems[newLevel] = [change];
            }
            else
            {
                changedItems[newLevel].push(change);
            }
        }
        
    }
}