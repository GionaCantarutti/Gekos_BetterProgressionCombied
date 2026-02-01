import { DependencyContainer } from "tsyringe";
import { Context } from "../contex";
import { LocationLifecycleService } from "@spt/services/LocationLifecycleService";
import { IPmcData } from "@spt/models/eft/common/IPmcData";
import { IEndLocalRaidRequestData } from "@spt/models/eft/match/IEndLocalRaidRequestData";
import { IVictim } from "@spt/models/eft/common/tables/IBotBase";
import { ISptProfile } from "@spt/models/eft/profile/ISptProfile";
import { TraderController } from "@spt/controllers/TraderController";
import { getDogtagsList } from "../utils";
import { ItemTpl } from "@spt/models/enums/ItemTpl";

export function addSupportForGPTraders(context: Context, container: DependencyContainer): void
{
    container.afterResolution("TraderController", (_t, result: TraderController) =>
    {
        // We want to replace the original method logic with something different
        const old = result.getItemPrices.bind(result);
        result.getItemPrices = (sessionId: string, traderId: string) =>
        {
            const oldOut = old(sessionId, traderId); //Add old logic back in

            oldOut.currencyCourses["5d235b4d86f7742e017bc88a"] = 7500; //ToDo: configurable?
            
            return oldOut;
        };
    }, {frequency: "Always"});
}

export function changeRefPurchasingOptions(context: Context): void
{
    const ref = context.tables.traders["6617beeaa9cfa777ca915b7c"];
    const config = context.config.refChanges;

    if (config.refBuysInGPCoins) ref.base.currency = "GP";

    if (config.refOnlyBuysDogtags)
    {
        ref.base.items_buy.category = [];
        ref.base.items_buy.id_list = getDogtagsList(context);
    }

    if (config.refAlsoBuysLegaMedals) ref.base.items_buy.id_list.push(ItemTpl.BARTER_LEGA_MEDAL);
}

export function gainRefRepOnKill(context: Context, container: DependencyContainer): void
{
    
    container.afterResolution("LocationLifecycleService", (_t, result: LocationLifecycleService) =>
    {
        // We want to replace the original method logic with something different
        const old = (result as any).handlePostRaidPmc.bind(result);
        (result as any).handlePostRaidPmc = (sessionId: string, fullProfile: ISptProfile, scavProfile: IPmcData, isDead: boolean, isSurvived: boolean, isTransfer: boolean, request: IEndLocalRaidRequestData, locationName: string) =>
        {
            old(sessionId, fullProfile, scavProfile, isDead, isSurvived, isTransfer, request, locationName); //Add old logic back in

            let pmcKills: IVictim[] = [];

            try
            {
                //Filter PMC kills
                pmcKills = request.results.profile.Stats.Eft.Victims.filter(
                    (victim) => ["pmcbear", "pmcusec"].includes(victim.Role.toLowerCase()));
            } 
            catch (error)
            {
                this.context.logger.error("Error Details:" + "Something went wrong when trying to tally up PMC kills!");
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }

            try 
            {
                fullProfile.characters.pmc.TradersInfo["6617beeaa9cfa777ca915b7c"].standing += repByKills(pmcKills, context);
            }
            catch (error)
            {
                this.context.logger.error("Error Details:" + "Something went wrong when trying add Ref rep for PMC kills!");
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }
            
        };
        // The modifier Always makes sure this replacement method is ALWAYS replaced
    }, {frequency: "Always"});

}

function repByKills(pmcKills: IVictim[], context: Context): number
{
    let totalRep: number = 0;

    for (const kill of pmcKills) for (const repRange of context.config.refChanges.refStandingOnKill.repByKillLevel)
    {
        if (kill.Level >= repRange.levelRange[0] && kill.Level < repRange.levelRange[1]) totalRep += repRange.rep;
    }

    return totalRep;
}