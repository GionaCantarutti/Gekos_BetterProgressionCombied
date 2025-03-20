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
using TMPro;
using UnityEngine;

namespace gekos_api
{
    [BepInPlugin("gekos_api_uniqueGUID", "gekos_api", "0.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;

        //public static SkillsConfig SkillsConfig;

        private void Awake() {
            LogSource = Logger;

            //Fix GP icon
            new GPFix().Enable();

            //Make it so that items whose sale price is close to 0 are sold for 1 instead of being unsellable
            new MinPriceFix().Enable();

            //Apply skill multipliers
            new SkillsMultipliers().Enable();

            LogSource.LogInfo("Geko's API loaded!");
        }

    }
}
