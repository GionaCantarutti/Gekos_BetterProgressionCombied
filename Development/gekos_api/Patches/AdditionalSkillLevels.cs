using EFT;
using gekos_api.Helpers;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace gekos_api.Patches
{
    class AdditionalSkillLevels : ModulePatch
    {

        public class SaveData
        {
            private Dictionary<ESkillId, float> dict = new Dictionary<ESkillId, float>();
            
            public float this[ESkillId key]
            {
                get {
                    if (dict.TryGetValue(key, out float res)) return res;
                    return 0;
                }
                set {
                    dict[key] = value;
                    Save();
                }
            }

            public void Save()
            {
                SaveDataHandler.SaveProfileData("skill_levels_savedata", dict);
            }

            public bool TryGetValue(ESkillId key, out float value)
            {
                return dict.TryGetValue(key, out value);
            }

            public void SetWithoutSaving(Dictionary<ESkillId, float> newData)
            {
                dict = newData;
            }

            public Dictionary<ESkillId, float>.ValueCollection GetSpentValues()
            {
                return dict.Values;
            }

        }

        private static SaveData _additionalLevels;

        private static float pointsPerLevel = 1;

        private static Dictionary<ESkillId, AbstractSkillClass> skills;

        public static SaveData AdditionalLevels
        {
            get { return _additionalLevels; }
            set
            {
                _additionalLevels = value;
                _additionalLevels.Save();
            }
        }

        static AdditionalSkillLevels()
        {
            _additionalLevels = new SaveData();
            skills = new Dictionary<ESkillId, AbstractSkillClass>();
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(AbstractSkillClass), nameof(AbstractSkillClass.Current));
        }

        [PatchPostfix]
        static void Postfix(ref AbstractSkillClass __instance, ref float __result)
        {
            ESkillId skillId = __instance.Id;
            skills[skillId] = __instance;

            if (AdditionalLevels.TryGetValue(skillId, out float delta))
            {
                __result += delta * 100f;
            }
        }

        public static void DeltaLevels(ESkillId skill, float delta)
        {
            _additionalLevels[skill] += delta;
            AbstractSkillClass actualSkill;
            if (skills.TryGetValue(skill, out actualSkill) && actualSkill is SkillClass)
            {
                ((SkillClass)actualSkill).method_3(); //Update buffs
            }
        }

        public static int GetAvailableSkillPoints()
        {
            int level = Utils.GetPlayerProfile().Info.Level;
            Plugin.LogSource.LogWarning($"Player is level {level}");
            float spent = 0;
            foreach (float s in AdditionalLevels.GetSpentValues())
            {
                spent += s;
            }
            return Mathf.FloorToInt((level * pointsPerLevel) - spent);
        }

        /// <summary>
        /// Attempts to apply the given delta if available and allocated skill points allow
        /// Returns false if the operation was denied
        /// </summary>
        /// <param name="skill"></param>
        /// <param name="delta"></param>
        /// <returns></returns>
        public static bool TryDeltaLevels(ESkillId skill, float delta)
        {
            if (GetAvailableSkillPoints() - delta > 0)
            {
                DeltaLevels(skill, delta);
                return true;
            }
            return false;
        }

    }
}
