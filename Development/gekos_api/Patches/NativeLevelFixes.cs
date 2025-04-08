using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gekos_api.Patches
{
    class LevelExpFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(SkillClass), nameof(SkillClass.LevelExp));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }

    }

    class CalculateExpOnFirstLevelsFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillClass), nameof(SkillClass.CalculateExpOnFirstLevels));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }
    }

    class BaseProgressFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(SkillClass), nameof(SkillClass.BaseProgress));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }
    }

    class ProgressValueFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(SkillClass), nameof(SkillClass.ProgressValue));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }
    }

    class OnTriggerFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillClass), nameof(SkillClass.OnTrigger));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }
    }

    class Method4Fix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillClass), nameof(SkillClass.method_4));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }
    }

    class LevelProgressFix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(SkillClass), nameof(SkillClass.LevelProgress));
        }

        [PatchPrefix]
        static bool Prefix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = true;
            return true;
        }

        [PatchPostfix]
        static void Postfix()
        {
            AdditionalSkillLevels.ExposeNativeLevel = false;
        }
    }
}
