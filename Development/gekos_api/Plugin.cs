using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gekos_api.Patches;
using EFT.InventoryLogic;
using gekos_api.Utils;

namespace gekos_api
{
    [BepInPlugin("gekos_api_uniqueGUID", "gekos_api", "0.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        //public static SkillsConfig SkillsConfig;

        private void Awake() {
            LogSource = Logger;

            //Fix GP icon
            //new GPFix().Enable();
            //GClass2934.CurrencyIndex[ECurrencyType.GP].GetType().GetField("Char").SetValue(GClass2934.CurrencyIndex[ECurrencyType.GP], "GP");

            //Apply skill changes
            new SkillsMultipliers().Enable();

            LogSource.LogInfo("Geko's API loaded!");
        }

    }
}
