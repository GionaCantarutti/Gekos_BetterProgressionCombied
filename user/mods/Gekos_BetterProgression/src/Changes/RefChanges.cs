using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using System.Reflection;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Eft.Game;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Models.Eft.Match;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Constants;

namespace GekosBetterProgression.Changes;

public static class RefChanges
{
    public static readonly string REF_TRADER_ID = "6617beeaa9cfa777ca915b7c";

    public static bool Apply(Context context)
    {
        ChangeRefPurchasingOptions(context);

        GainRefRepOnKillPatch.context = context;
        new GainRefRepOnKillPatch().Enable();
        new AddSupportForGPTradersPatch().Enable();
        
        return true;
    }

    private static void ChangeRefPurchasingOptions(Context context)
    {
        Trader refTrader = context.tables.Traders[REF_TRADER_ID];
        GekoConfig.RefChanges config = context.config.refChanges;

        if (config.refBuysInGPCoins)
        {
            refTrader.Base.Currency = CurrencyType.GP;
        }

        if (config.refOnlyBuysDogtags)
        {
            refTrader.Base.ItemsBuy.Category = new HashSet<MongoId>();
            refTrader.Base.ItemsBuy.IdList = Utils.GetDogtagsList(context);
        }

        if (config.refAlsoBuysLegaMedals)
        {
            refTrader.Base.ItemsBuy.IdList.Add(ItemTpl.BARTER_LEGA_MEDAL);
        }
    }
}

public class AddSupportForGPTradersPatch : AbstractPatch 
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TraderController).GetMethod(nameof(TraderController.GetItemPrices));
    }

    [PatchPostfix]
    static void Postfix(ref GetItemPricesResponse __result)
    {
        __result.CurrencyCourses["5d235b4d86f7742e017bc88a"] = 7500; // ToDo from Legacy: Configurable?
    }
}

public class GainRefRepOnKillPatch() : AbstractPatch
{
    public static Context context;

    protected override MethodBase GetTargetMethod()
    {
        return typeof(LocationLifecycleService).GetMethod(nameof(LocationLifecycleService.EndLocalRaid));
    }

    [PatchPostfix]
    static void Postfix(MongoId sessionId, EndLocalRaidRequestData request)
    {
        string[] validKilledSides = new string[] { Sides.PmcUsec, Sides.PmcBear };
        IEnumerable<Victim> pmcKills = request.Results.Profile.Stats.Eft.Victims.Where((victim) => validKilledSides.Contains(victim.Role));

        SptProfile fullProfile = context.profileHelper.GetFullProfile(sessionId);
        fullProfile.CharacterData.PmcData.TradersInfo["6617beeaa9cfa777ca915b7c"].Standing += RepByKills(context, pmcKills);
    }

    private static double RepByKills(Context context, IEnumerable<Victim> victims)
    {
        double totalRep = 0;

        foreach (var victim in victims)
        {
            foreach (var repRange in context.config.refChanges.refStandingOnKill.repByKillLevel)
            {
                if (victim.Level >= repRange.levelRange[0] && victim.Level <= repRange.levelRange[1])
                {
                    totalRep += repRange.rep;
                }
            }
        }

        return totalRep;
    }
}