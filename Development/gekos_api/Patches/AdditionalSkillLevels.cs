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

            public Dictionary<ESkillId, float> GetDict()
            {
                return dict;
            }

        }

        private static SaveData _additionalLevels;

        private static float pointsPerLevel = 1;

        private static Dictionary<ESkillId, AbstractSkillClass> skills;

        private static PointsConfig config;

        //If true the additional levels will not get added to the Level getter
        public static bool ExposeNativeLevel = false;

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
            config = ConfigHandler.GetPointsConfig();
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(SkillClass), nameof(SkillClass.Level));
        }

        [PatchPostfix]
        static void Postfix(ref AbstractSkillClass __instance, ref int __result)
        {
            if (!config.enable) return;
            if (ExposeNativeLevel) return;
            ESkillId skillId = __instance.Id;
            skills[skillId] = __instance;

            if (AdditionalLevels.TryGetValue(skillId, out float delta))
            {
                __result += Mathf.FloorToInt(delta);
            }
        }

        public static void DeltaLevels(ESkillId skill, float delta)
        {
            _additionalLevels[skill] += delta;
            AbstractSkillClass actualSkill;
            if (skills.TryGetValue(skill, out actualSkill) && actualSkill is SkillClass)
            {
                UpdateBuffs((SkillClass)actualSkill);
                AvailableSkillPointsUI.UpdatePoints();
            }
        }

        /// <summary>
        /// Update all skill buffs that got changed
        /// </summary>
        public static void UpdateAllBuffs()
        {
            Profile player = Utils.GetPlayerProfile();

            foreach (KeyValuePair<ESkillId, float> s in AdditionalLevels.GetDict())
            {
                if (player.Skills.TryGetSkill(s.Key, out SkillClass skill))
                {
                    UpdateBuffs(skill);
                }
            }
        }

        public static int GetAvailableSkillPoints()
        {
            Profile player = Utils.GetPlayerProfile();
            int level = player.Info.Level;
            float spent = 0;
            foreach (KeyValuePair<ESkillId, float> s in AdditionalLevels.GetDict())
            {
                spent += s.Value;
                if (config.automaticallyRefundOverflows)
                {
                    if (player.Skills.TryGetSkill(s.Key, out SkillClass skill))
                    {
                        int skillLevel = skill.GetLevelForValue(skill.Current); //Get skill level before clamping
                        spent -= Mathf.Max(0, skillLevel - 51);
                    }
                }
            }
            return Mathf.FloorToInt(Mathf.Max(0, (level * config.skillPointsPerLevel) - spent));
        }

        /// <summary>
        /// Recalculate the buffs for the given skill
        /// </summary>
        /// <param name="skill"></param>
        public static void UpdateBuffs(SkillClass skill)
        {
            skill.method_3();
        }

        /// <summary>
        /// Returns the actual behind-the-scenes level of the skill before the increases given by skill points
        /// </summary>
        /// <param name="skill"></param>
        /// <returns></returns>
        public static int NativeLevel(SkillClass skill)
        {
            ESkillId skillId = skill.Id;

            if (AdditionalLevels.TryGetValue(skillId, out float delta))
            {
                return Mathf.CeilToInt(skill.Level - delta);
            }

            return skill.Level;
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
            if (GetAvailableSkillPoints() - delta >= 0)
            {
                DeltaLevels(skill, delta);
                return true;
            }
            return false;
        }

    }
}
