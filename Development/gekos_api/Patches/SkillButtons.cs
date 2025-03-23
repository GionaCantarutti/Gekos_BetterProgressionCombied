using EFT.UI;
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
using UnityEngine.UI;

namespace gekos_api.Patches
{

    class SkillButtons : ModulePatch
    {
        private const string OBJECT_NAME = "skillpoint_buttons";

        private static GameObject buttonsPrefab;

        static SkillButtons()
        {
            buttonsPrefab = Utils.LoadGameObject("skillsbutton.bundle", "Buttons Panel");
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillIcon), nameof(SkillPanel.Show));
        }

        [PatchPostfix]
        static void Postfix(ref SkillIcon __instance)
        {
            Transform icon = __instance.transform.Find("Skill Icon");
            //No need to do anything if the buttons have already been created
            if (icon.Find(OBJECT_NAME) != null) return;

            GameObject newButtons = GameObject.Instantiate(buttonsPrefab);
            newButtons.name = OBJECT_NAME;
            Transform t = newButtons.transform;
            t.SetParent(icon);
            t.localPosition = Vector3.zero;
            t.localScale = Vector3.one;
            RectTransform rt = newButtons.GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            Image i = newButtons.GetComponent<Image>();
            i.enabled = false; //Hide debugging panel
        }

    }
}
