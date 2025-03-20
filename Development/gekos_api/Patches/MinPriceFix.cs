using EFT;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gekos_api.Patches
{
    internal class MinPriceFix : ModulePatch
    {

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TraderClass), nameof(TraderClass.GetUserItemPrice));
        }

        [PatchPostfix]
        static void Postfix(ref TraderClass.GStruct264? __result, ref TraderClass __instance, Item item)
        {
			//Only bother doing the Postfix if necessary
            if (__result != null) return;

            if (__instance.supplyData_0 == null) return;
            if (!__instance.Info.CanBuyItem(item)) return;

            //If we've safely gotten to this point then the original logic must have retunred NULL because value was close to 0
            //Let us return a value of 1 instead of NULL
			MongoID currencyId = GClass2934.GetCurrencyId(__instance.Settings.Currency);
			__result = new TraderClass.GStruct264?(new TraderClass.GStruct264(new MongoID?(currencyId), Convert.ToInt32(1)));
		}

    }
}
