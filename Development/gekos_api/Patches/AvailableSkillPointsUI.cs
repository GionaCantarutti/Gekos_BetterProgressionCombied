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

namespace gekos_api.Patches
{
    class AvailableSkillPointsUI : ModulePatch
    {
        private const string POINTS_OBJECT_NAME = "Available Points Text";
        private const string NOTIFICATION_OBJ_NAME = "Points Notification";
        private static TextMeshProUGUI tmp;

        private static PointsConfig config;

        static AvailableSkillPointsUI()
        {
            config = ConfigHandler.GetPointsConfig();
        }

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SkillsAndMasteringScreen), nameof(SkillsAndMasteringScreen.Show));
        }

        [PatchPostfix]
        static void Postfix(ref SkillsAndMasteringScreen __instance)
        {
            if (!config.enable) return;

            HandlePointsText(__instance);

            //ToDo: notification thingy
            /*
            Transform SkillsTab = GameObject.Find("InventoryScreen").transform.Find("Tab Bar").Find("Tabs").Find("Skills");
            GameObject NewMessages = GameObject.Find("Chat").transform.GetChild(1).GetChild(0).gameObject;

            if (SkillsTab.Find(NOTIFICATION_OBJ_NAME) != null) return;

            GameObject notification = GameObject.Instantiate(NewMessages);
            notification.name = NOTIFICATION_OBJ_NAME;
            notification.transform.SetParent(SkillsTab);
            notification.transform.localPosition = Vector3.zero;

            TextMeshProUGUI tmp = notification.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            tmp.text = AdditionalSkillLevels.GetAvailableSkillPoints().ToString();
            */

        }

        private static void HandlePointsText(SkillsAndMasteringScreen __instance)
        {
            Transform TopPanel = __instance.gameObject.transform.Find("TopPanel");
            Transform ProgressPanel = TopPanel.Find("Progress Panel");

            if (tmp != null)
            {
                TextMeshProUGUI doubleCheck = ProgressPanel.Find(POINTS_OBJECT_NAME)?.gameObject.GetComponent<TextMeshProUGUI>();
                if (doubleCheck != null)
                {
                    UpdatePoints();
                    return; //Only do the rest if object is missing
                }
            }

            Transform CurrentText = ProgressPanel.Find("Current Text");

            GameObject availableLevels = GameObject.Instantiate(CurrentText.gameObject);
            availableLevels.name = POINTS_OBJECT_NAME;
            availableLevels.transform.SetParent(ProgressPanel);

            RectTransform rt = availableLevels.GetComponent<RectTransform>();
            rt.position = new Vector3(CurrentText.position.x * 1.8f, CurrentText.position.y, rt.position.z);
            rt.localScale = Vector3.one;

            tmp = availableLevels.GetComponent<TextMeshProUGUI>();
            tmp.text = "<color=#949286>available points: </color> PLACEHOLDER";
            UpdatePoints();
        }

        public static void UpdatePoints()
        {
            int pointsVal = AdditionalSkillLevels.GetAvailableSkillPoints();
            string points = pointsVal > 0 ? $"<color=#85FF9E>{pointsVal}</color>" : $"{pointsVal}";
            tmp.text = $"<color=#949286>available points: </color> {points}";
        }
    }
}
