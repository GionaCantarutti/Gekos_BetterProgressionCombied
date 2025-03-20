using EFT;
using EFT.InventoryLogic;
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
    internal class GPFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass2934), nameof(GClass2934.GetCurrencyId));
        }

        [PatchPrefix]
        static bool Prefix(ECurrencyType currencyType, ref MongoID __result)
        {
            if (currencyType == ECurrencyType.GP)
            {
                __result = new GClass2934.GClass2935(ECurrencyType.GP, "GP", "GP", GClass2934.GP_ID, null).Id;
                return false;
            }
            else return true;
        }
    }
}
