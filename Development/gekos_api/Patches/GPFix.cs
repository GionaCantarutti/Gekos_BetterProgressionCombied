using EFT;
using EFT.InventoryLogic;
using EFT.UI.DragAndDrop;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace gekos_api.Patches
{
    internal class GPFix : ModulePatch
    {

        private static TMP_SpriteAsset gpAsset;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(TradingItemView), nameof(TradingItemView.SetPrice));
        }

        [PatchPostfix]
        static void Postfix(ref TradingItemView __instance)
        {
            TextMeshProUGUI currency = (TextMeshProUGUI)typeof(TradingItemView).GetField("_currency", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
            if (currency == null) return;

            bool missingAsset = gpAsset == null;

            if (currency.spriteAsset == null)
            {
                if (missingAsset)
                {
                    //Hyjack currency MonoBehavior to make it do our bidding
                    currency.StartCoroutine(TrySettingAsset(currency));
                    return;
                }
                currency.spriteAsset = gpAsset;
            } else
            {
                if (missingAsset && currency.spriteAsset.name == "GP_for_trading_view")
                {
                    Plugin.LogSource.LogMessage("Found the asset");
                    gpAsset = currency.spriteAsset;
                }
            }
        }

        private static IEnumerator TrySettingAsset(TextMeshProUGUI currency)
        {
            while (gpAsset == null) yield return null;
            Plugin.LogSource.LogMessage("Setting the asset");
            currency.spriteAsset = gpAsset;
            yield return null;
        }
    }
}
