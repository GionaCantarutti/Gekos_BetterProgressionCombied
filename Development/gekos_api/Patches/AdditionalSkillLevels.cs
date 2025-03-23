using EFT;
using gekos_api.Helpers;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
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
        }

        private static SaveData _additionalLevels;

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
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(AbstractSkillClass), nameof(AbstractSkillClass.Current));
        }

        [PatchPostfix]
        static void Postfix(ref AbstractSkillClass __instance, ref float __result)
        {
            ESkillId skillId = __instance.Id;

            if (AdditionalLevels.TryGetValue(skillId, out float delta))
            {
                __result += delta * 100f;
            }
        }

        public void DeltaLevels(ESkillId skill, float delta)
        {
            _additionalLevels[skill] += delta;
        }

    }
}
