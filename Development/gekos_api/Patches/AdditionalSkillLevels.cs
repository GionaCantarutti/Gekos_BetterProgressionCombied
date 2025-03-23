using EFT;
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

        private static Dictionary<ESkillId, float> additionalLevels;

        static AdditionalSkillLevels()
        {
            additionalLevels = new Dictionary<ESkillId, float>();
            additionalLevels.Add(ESkillId.Endurance, 5);
            additionalLevels.Add(ESkillId.Strength, 1000);
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(AbstractSkillClass), nameof(AbstractSkillClass.Current));
        }

        [PatchPostfix]
        static void Postfix(ref AbstractSkillClass __instance, ref float __result)
        {
            ESkillId skillId = __instance.Id;

            if (additionalLevels.TryGetValue(skillId, out float delta))
            {
                __result += delta * 100f;
            }
        }
    }
}
