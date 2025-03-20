import { IItem } from "@spt/models/eft/common/tables/IItem";
import { ITemplateItem } from "@spt/models/eft/common/tables/ITemplateItem";
import { Context } from "../contex";
import { BaseClasses } from "@spt/models/enums/BaseClasses";
import { isQuestLockedCraft, setAreaLevelRequirement } from "../utils";

export function calculateAmmoLoyalty(item: IItem, context: Context): number
{
    const itemTemplate: ITemplateItem = context.tables.templates.items[item._tpl];

    return scoreAmmo(itemTemplate, context);
}

export function scoreAmmo(itemTemplate: ITemplateItem, context: Context): number
{

    const config = context.config.algorithmicalRebalancing.ammoRules;

    let loyalty: number = config.defaultBaseLoyaltyByPen;

    //Base level from penetration
    for (const rule of config.ammoBaseLoyaltyByPen)
    {
        if (itemTemplate._props.PenetrationPower >= rule.penInterval[0] && itemTemplate._props.PenetrationPower < rule.penInterval[1])
        {
            loyalty = rule.baseLoyalty;
        }
    }
    
    //Modify by caliber
    for (const rule of config.caliberRules)
    {
        if (itemTemplate._props.Caliber == rule.caliber)
        {
            loyalty += rule.loyaltyDelta;
        }
    }

    //Modify by damage (accounting for projectile count)
    for (const rule of config.damageRules)
    {
        const totalDamage = itemTemplate._props.Damage * itemTemplate._props.ProjectileCount;
        if (totalDamage >= rule.damageInterval[0] && totalDamage < rule.damageInterval[1])
        {
            loyalty += rule.loyaltyDelta;
        }
    }

    loyalty += config.globalDelta;

    return loyalty
}

export function rebalanceAmmoCrafts(context: Context): void
{
    const config = context.config.algorithmicalRebalancing.ammoRules;
    const crafts = context.tables.hideout.production.recipes;

    for (const craft of crafts)
    {
        const ammoId: string = craft.endProduct;
        if (!context.itemHelper.isOfBaseclass(ammoId, BaseClasses.AMMO)) continue; //Exclude non-ammo

        let score: number = scoreAmmo(context.tables.templates.items[ammoId], context);

        if (isQuestLockedCraft(craft)) score += context.config.algorithmicalRebalancing.questLockDelta;

        for (const map of config.craftSettings.loyaltyToLevelRanges)
        {
            if (score >= map.range[0] && score < map.range[1]) setAreaLevelRequirement(craft, map.level);
        }
    }

}