using EFT;
using gekos_api.Utils;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gekos_api.Patches
{
    internal class SkillsMultipliers : ModulePatch
    {

        private static readonly SkillsConfig skillsConfig;

        static SkillsMultipliers()
        {
            skillsConfig = ConfigHandler.GetStatsConfig();
        }

        protected override MethodBase GetTargetMethod()
        {
            return typeof(SkillClass).GetMethod(nameof(SkillClass.UseEffectiveness), BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
        }

        [PatchPostfix]
        private static void Postfix(ref SkillClass __instance, ref float __result, ref float ___float_2)
        {
            bool skillSpecific = skillsConfig.SkillMultipliers.TryGetValue(__instance.Id.ToString(), out float multiplier);

            if (!skillSpecific) multiplier = 1;

            multiplier *= skillsConfig.GlobalMultiplier;

            __result *= multiplier;
            ___float_2 = __result;
        }
    }
}
