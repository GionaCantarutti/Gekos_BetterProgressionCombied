using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.Changes;

public class BitcoinChanges
{
    public static void Apply(Context context)
    {
        if (context.config.bitcoinChanges.overrideValue)
        {
            HandbookItem? item = context.tables.Templates.Handbook.Items.Find((item) => item.Id == ItemTpl.BARTER_PHYSICAL_BITCOIN);
            if (item is null)
            {
                context.logger.Error("Could not find base bitcoin to edit");
                return;
            }
            item.Price = context.config.bitcoinChanges.value;
        }

        List<HideoutProduction>? btcProduction = context.tables.Hideout.Production.Recipes?.FindAll((production) => production.EndProduct == ItemTpl.BARTER_PHYSICAL_BITCOIN);
        if (btcProduction is null)
        {
            context.logger.Error("Could not find Bitcoin craft");
            return;
        }

        foreach (HideoutProduction prod in btcProduction)
        {
            prod.ProductionTime = Math.Round((double)prod.ProductionTime / context.config.bitcoinChanges.btcFarmSpeedMult);
            prod.ProductionLimitCount = context.config.bitcoinChanges.btcCapacity;
        }

        context.tables.Hideout.Settings.GpuBoostRate = context.config.bitcoinChanges.gpuBoostRate;

        if (context.config.bitcoinChanges.cannotBuyGPU)
        {
            foreach (Trader trader in context.tables.Traders.Values)
            {
                if (trader.Assort == null)
                {
                    continue;
                }
                trader.Assort.Items = trader.Assort.Items.FindAll((item) => item.Template != "57347ca924597744596b4e71" || Utils.IsBarterTrade(item, trader));
            }
        }

        return;
    }
}