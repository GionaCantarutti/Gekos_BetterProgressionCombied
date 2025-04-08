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
using System.Collections;

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

            if (ConfigHandler.GetPointsConfig().enable) //Do not enable patches if module is disabled
            {
                //Patch that creates the + and - buttons in the skills tab
                new SkillButtons().Enable();
                //Apply patch to add the additional skill levels
                new AdditionalSkillLevels().Enable();
                StartCoroutine(LoadSkillLevels());
                //Patch that creates the skills points UI element
                new AvailableSkillPointsUI().Enable();
                //Make it so that certain method use the native level of the skill instaed of the modified one
                new LevelExpFix().Enable();
                new CalculateExpOnFirstLevelsFix().Enable();
                new BaseProgressFix().Enable();
                new ProgressValueFix().Enable();
                new OnTriggerFix().Enable();
                new Method4Fix().Enable();
                new LevelProgressFix().Enable();
            }

            LogSource.LogInfo("Geko's API fully loaded!");
        }

        private IEnumerator LoadSkillLevels()
        {
            bool loaded = false;
            while (!loaded)
            {
                try
                {
                    Dictionary<ESkillId, float> loadedData;
                    if (SaveDataHandler.LoadProfileData<Dictionary<ESkillId, float>>("skill_levels_savedata", out loadedData))
                    {
                        Logger.LogMessage("Skills save data successfully loaded!");
                        AdditionalSkillLevels.AdditionalLevels.SetWithoutSaving(loadedData);
                        loaded = true;
                    }
                } catch { }
                yield return new WaitForSeconds(.5f);
            }
        }

    }
}
