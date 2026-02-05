using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using GekosBetterProgression;
using JetBrains.Annotations;
using System.Text.RegularExpressions;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace gekos_server.Changes;

public class FirChanges()
{

    public static void RemoveFirFromQuests(Context context)
    {

        Regex foundInRaidRegex = new Regex("Find.*in raid", RegexOptions.IgnoreCase);
        Regex inRaidRegex = new Regex("in raid", RegexOptions.IgnoreCase);

        foreach (Quest quest in context.tables.Templates.Quests.Values)
        {
            var sets = new List<List<QuestCondition>?> {
                quest.Conditions.AvailableForFinish,
                quest.Conditions.AvailableForStart,
                quest.Conditions.Fail,
                quest.Conditions.Started,
                quest.Conditions.Success
            };

            foreach (var set in sets)
            {
                if (set == null)
                {
                    continue;
                }

                foreach (var condition in set)
                {
                    if (condition.ConditionType == "HandoverItem" || condition.ConditionType == "FindItem")
                    {
                        condition.OnlyFoundInRaid = false;
                    }
                }
            }
        }

        var locales = context.tables.Locales.Global;

        // Remove "in raid" from locale text
        foreach (var lang in locales.Keys)
        {
            var locale = locales[lang].Value;
            if (locale is null) continue;

            foreach (var key in locale.Keys.ToList())
            {
                string text = locale[key];

                if (foundInRaidRegex.IsMatch(text))
                {
                    locale[key] = inRaidRegex.Replace(text, "");
                }
            }
        }
    }

    public static void RemoveFirFromFlea(Context context)
    {
        context.tables.Globals.Configuration.RagFair.IsOnlyFoundInRaidAllowed = false;
    }

    public static void RemoveFirFromHideout(Context context)
    {
        List<HideoutArea> hideoutAreas = context.tables.Hideout.Areas;

        foreach (var area in hideoutAreas)
        {
            foreach (var stage in area.Stages.Values)
            {
                List<StageRequirement>? itemReq = stage.Requirements?.FindAll(item => item.Type == "Item");

                if (itemReq is null)
                {
                    context.logger.Error($"Something went wrong when fetching requirements for {area.Type}");
                    continue;
                }

                foreach (var req in itemReq)
                {
                    req.IsSpawnedInSession = false;
                }
            }
        }
    }

    public static void RemoveFirFromRepeatables(Context context)
    {
        var questConfig = context.sptConfig.GetConfigByString<QuestConfig>("Quest");

        if (questConfig?.RepeatableQuests == null)
        {
            context.logger.Warning("Repeatable quest config not found, skipping FiR removal");
            return;
        }

        foreach (var repeatable in questConfig.RepeatableQuests)
        {
            foreach (var completion in repeatable.QuestConfig.CompletionConfig)
            {
                completion.RequiredItemsAreFiR = false;
            }
        }
    }
}