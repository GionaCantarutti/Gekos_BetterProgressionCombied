using EFT;
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
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace gekos_api.Patches
{

    class SkillButtons : ModulePatch
    {
        private const string OBJECT_NAME = "skillpoint_buttons";

        private static GameObject buttonsPrefab;
        private static TMP_FontAsset font;
        private static Material fontMaterial;

        private static PointsConfig config;

        static SkillButtons()
        {
            buttonsPrefab = Utils.LoadGameObject("skillsbutton.bundle", "Buttons Panel");
            config = ConfigHandler.GetPointsConfig();
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillIcon), nameof(SkillPanel.Show));
        }

        [PatchPostfix]
        static void Postfix(ref SkillIcon __instance, SkillClass skill)
        {
            if (!config.enable) return;

            Transform icon = __instance.transform.Find("Skill Icon");
            //No need to do anything if the buttons have already been created
            if (icon.Find(OBJECT_NAME) != null) return;

            if (font == null)
            {
                TextMeshProUGUI reference = icon.transform.Find("Level Panel").Find("Level").GetComponent<TextMeshProUGUI>();
                font = reference.font;
            }
            if (fontMaterial == null)
            {
                TextMeshProUGUI reference = icon.transform.Find("Level Panel").Find("Level").GetComponent<TextMeshProUGUI>();
                fontMaterial = reference.fontMaterial;
            }

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

            List<Transform> buttons = t.Find("Layout").GetChildren();
            foreach (Transform b in buttons) {
                //Fix fonts
                TextMeshProUGUI text = b.Find("Text (TMP)").gameObject.GetComponent<TextMeshProUGUI>();
                text.font = font;
                text.fontMaterial = fontMaterial;
            }

            SkillPanel skillPanel = __instance.gameObject.GetComponent<SkillPanel>();
            SkillIcon sIcon = __instance;

            //Add button functionality
            buttons[0].GetComponent<Button>().onClick.AddListener(delegate() {
                ChangeSkillLevel(skill.Id, 1, skillPanel, sIcon);
            });
            buttons[1].GetComponent<Button>().onClick.AddListener(delegate () {
                ChangeSkillLevel(skill.Id, -1, skillPanel, sIcon);
            });
        }

        private static void ChangeSkillLevel(ESkillId skillId, float delta, SkillPanel panel, SkillIcon icon)
        {
            bool res = AdditionalSkillLevels.TryDeltaLevels(skillId, delta);
            if (res) {
                panel.method_1(); //Update visuals of the panel
                UpdateSkillLevel(icon);
            }
        }

        private static void UpdateSkillLevel(SkillIcon icon)
        {
            // Get the private fields thanks to reflections
            FieldInfo levelPanelField = typeof(SkillIcon).GetField("_levelPanel", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo skillClassField = typeof(SkillIcon).GetField("skillClass", BindingFlags.NonPublic | BindingFlags.Instance);

            if (levelPanelField != null && skillClassField != null)
            {
                SkillLevelPanel levelPanel = (SkillLevelPanel)levelPanelField.GetValue(icon);
                SkillClass skillClass = (SkillClass)skillClassField.GetValue(icon);

                if (levelPanel != null && skillClass != null)
                {
                    levelPanel.SetLevel(skillClass);
                }
            }
            else
            {
                UnityEngine.Debug.LogError("Reflection failed: Could not find _levelPanel or skillClass");
            }
        }

    }
}
