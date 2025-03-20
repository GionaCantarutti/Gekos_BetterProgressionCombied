import { ItemHelper } from "@spt/helpers/ItemHelper";
import { Context } from "../contex";
import { calculateAmmoLoyalty, rebalanceAmmoCrafts, scoreAmmo } from "./ammo";
import { calculateWeaponLoyalty, followDefaultBuild, penalizeAdvancedAttachments, weaponShifting as shiftWeapons } from "./weapon";
import { isBarterTrade, isQuestLocked, loyaltyFromScore, setLoyalty } from "../utils";
import { BaseClasses } from "@spt/models/enums/BaseClasses";
import { ChangedItem } from "./types";

export function algorithmicallyRebalance(context: Context): void
{
    //Alias
    const config = context.config.algorithmicalRebalancing;

    const itemHelper: ItemHelper = context.itemHelper;
    const traders = Object.values(context.tables.traders);

    //Organize changed items by loyalty level so that they can be easily accessed by the shifting system
    //[trade, loyaltyScore, trader, logChanges][index][loyaltyLevel]
    const changedItems: { [level: number] : ChangedItem[] } = {};

    //Loop over each trader
    for (const trader of traders)
    {
        const loyaltyLevels = trader?.assort?.loyal_level_items;
        if (loyaltyLevels == null) continue;

        const itemsForSale = trader?.assort?.items;
        if (itemsForSale == null) continue;

        if (config.excludeTraders.includes(trader.base._id)) continue;

        for (const item of itemsForSale)
        {
            if (Object.keys(config.explicitLoyaltyOverride.trades).includes(item._id)) continue;
            if (Object.keys(config.explicitLoyaltyOverride.items).includes(item._tpl)) continue;
            let thisItem: ChangedItem;

            //// AMMO ////
            if (config.ammoRules.enable)
            {
                let ammoOrBox = false;
                let ammo;
                let loyaltyScore;
                if (itemHelper.isOfBaseclass(item._tpl, BaseClasses.AMMO)) 
                {
                    loyaltyScore = calculateAmmoLoyalty(item, context);
                    ammo = item._tpl;
                    ammoOrBox = true;
                }
                else if (itemHelper.isOfBaseclass(item._tpl, BaseClasses.AMMO_BOX))
                {
                    ammo = context.tables.templates.items[item._tpl]._props.StackSlots[0]._props.filters[0].Filter[0];
                    loyaltyScore = scoreAmmo(context.tables.templates.items[ammo], context)
                    ammoOrBox = true
                }
                if (ammoOrBox && !config.ammoRules.ignoreCalibers.includes(context.tables.templates.items[ammo]._props.Caliber))
                {
                    thisItem = new ChangedItem(
                        item,
                        loyaltyScore,
                        trader,
                        config.ammoRules.logChanges,
                        false
                    );
                }
            }

            //// WEAPONS ////
            if (config.weaponRules.enable
                && itemHelper.isOfBaseclass(item._tpl, BaseClasses.WEAPON)
                && !itemHelper.isOfBaseclass(item._tpl, BaseClasses.SPECIAL_WEAPON)) //ToDo: double check melee and grenades (and possibly other exceptions)
            {
                const loyaltyScore = calculateWeaponLoyalty(item, itemsForSale, context);

                thisItem = new ChangedItem(
                    item,
                    loyaltyScore,
                    trader,
                    config.weaponRules.logChanges,
                    true
                );
            }

            if (thisItem != null) //If item is being affected by the mod
            {
                //Final modifications
                if (isQuestLocked(item, trader)) 
                {
                    thisItem.score += config.questLockDelta;
                    if (config.logBartersAndLocks) context.logger.info(context.tables.templates.items[item._tpl]._name + " is a quest-locked item\t(Trade ID: " + item._id + ")");
                }
                if (isBarterTrade(item, trader))
                {
                    thisItem.score += config.barterDelta;
                    if (config.logBartersAndLocks) context.logger.info(context.tables.templates.items[item._tpl]._name + " is a bartered item\t(Trade ID: " + item._id + ")");
                }

                //By-trader delta
                if (trader.base._id in config.deltaByTrader) thisItem.score += config.deltaByTrader[trader.base._id]

                //Explicit deltas
                const tradeD = config.explicitLoyaltyDelta.trades[thisItem.trade._id];
                const itemD = config.explicitLoyaltyDelta.items[thisItem.trade._tpl];
                if (!isNaN(tradeD)) thisItem.score += tradeD;
                if (!isNaN(itemD)) thisItem.score += itemD;

                const level = loyaltyFromScore(thisItem.score, config.clampToMaxLevel);
                if (changedItems[level] == null)
                {
                    changedItems[level] = [thisItem];
                }
                else
                {
                    changedItems[level].push(thisItem);
                }

            }
            
        }

    }

    if (config.weaponRules.upshiftRules.enable) shiftWeapons(changedItems, context);
    
    if (config.weaponRules.attachmentsFollowDefaultBuild) followDefaultBuild(changedItems, context);
    
    if (config.weaponRules.advancedAttachmentsDelta != 0) penalizeAdvancedAttachments(changedItems, context);

    //Apply changes
    for (const changesInLevel of Object.values(changedItems))
    {
        if (changesInLevel == null || changesInLevel.length == 0) continue;
        for (const changedItem of changesInLevel)
        {
            let doClamp: boolean = config.clampToMaxLevel;
            if (config.forceClampingOfQuestlockedItems && isQuestLocked(changedItem.trade, changedItem.trader)) doClamp = true;
            if (changedItem.logChange) context.logger.info(`Setting ${context.tables.templates.items[changedItem.trade._tpl]._name} at loyalty level ${loyaltyFromScore(changedItem.score, doClamp)} (${changedItem.score})`);
            setLoyalty(changedItem.trade._id, changedItem.score, changedItem.trader, doClamp);
        }
    }

    //Overrides
    for (const trader of traders)
    {
        const loyaltyLevels = trader?.assort?.loyal_level_items;
        if (loyaltyLevels == null) continue;

        const itemsForSale = trader?.assort?.items;
        if (itemsForSale == null) continue;

        if (config.excludeTraders.includes(trader.base._id)) continue;

        for (const item of trader.assort.items)
        {
            let override: number | null = null;
            override = config.explicitLoyaltyOverride.trades[item._id];
            if (override == null) override = config.explicitLoyaltyOverride.items[item._tpl];
            if (override == null) continue;
            
            setLoyalty(item._id, override, trader, config.clampToMaxLevel);
        }
    }

    if (config.ammoRules.craftSettings.enable) rebalanceAmmoCrafts(context);
    
}