using BepInEx;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gekos_api.Patches;
using EFT.InventoryLogic;
using gekos_api.Helpers;
using TMPro;
using UnityEngine;
using System.Reflection;
using System.IO;
using SPT.Reflection.Utils;
using EFT;

namespace gekos_api
{
    [BepInPlugin("gekos_api_uniqueGUID", "gekos_api", "0.4.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        public static string PluginFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string AssetsFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Assets");

        private void Awake() {
            LogSource = Logger;

            //Fix GP icon
            new GPFix().Enable();

            //Make it so that items whose sale price is close to 0 are sold for 1 instead of being unsellable
            new MinPriceFix().Enable();

            //Apply skill XP multipliers
            new SkillsMultipliers().Enable();

            //Apply skill buff multipliers
            new SkillBuffMulti1303().Enable();
            new SkillBuffMulti1304().Enable();
            new SkillBuffMulti1305().Enable();
            new SkillBuffMulti1306().Enable();

            //Apply patch to add the additional skill levels
            new AdditionalSkillLevels().Enable();

            LogSource.LogInfo("Geko's API loaded!");
        }

    }
}
