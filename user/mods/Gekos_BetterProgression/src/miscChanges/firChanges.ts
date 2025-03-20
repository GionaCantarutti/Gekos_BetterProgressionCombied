import { IStageRequirement } from "@spt/models/eft/hideout/IHideoutArea";
import { Context } from "../contex";
import { DependencyContainer } from "tsyringe";
import { RepeatableQuestGenerator } from "@spt/generators/RepeatableQuestGenerator";
import { IQuestCondition } from "@spt/models/eft/common/tables/IQuest";

export function removeFirFromQuests(context: Context): void
{
    const quests = context.tables.templates.quests;
    const locales = context.tables.locales.global;
    const foundInRaidRegex = new RegExp("Find.*in raid", "i");
    const inRaidRegex = new RegExp("in raid", "i");

    for (const quest of Object.values(quests))
    {
        const sets = [quest.conditions.AvailableForFinish, quest.conditions.AvailableForStart, quest.conditions.Fail, quest.conditions.Started, quest.conditions.Success]

        for (const set of sets)
        {
            if (set == null) continue;

            //Allow handing over of non-FiR items
            for (const cond of set)
            {
                if (cond.conditionType == "HandoverItem" || cond.conditionType === "FindItem")
                {
                    cond.onlyFoundInRaid = false;
                }
            }
        }
    }

    //Change the locales to remove "in raid"
    for (const [lang, locale] of Object.entries(locales)) for (const [key, text] of Object.entries(locale))
    {
        if (foundInRaidRegex.test(text))
        {
            locales[lang][key] = text.replace(inRaidRegex, "");
        }
    }
}

export function removeFirFromFlea(context: Context): void
{
    context.tables.globals.config.RagFair.isOnlyFoundInRaidAllowed = false;
}

export function removeFirFromHideout(context: Context): void
{

    const hideoutAreas = context.tables.hideout.areas;

    for (const area of hideoutAreas) for (const stage of Object.values(area.stages))
    {
        const itemReq = stage.requirements.filter((req) => req.type == "Item");

        for (const req of itemReq as IStageRequirement[])
        {
            req.isSpawnedInSession = false;
        }
    }

}

export function removeFirFromRepeatables(context: Context, container: DependencyContainer): void
{
    container.afterResolution("RepeatableQuestGenerator", (_t, result: RepeatableQuestGenerator) =>
    {
        // We want to replace the original method logic with something different
        const old = (result as any).generateCompletionAvailableForFinish.bind(result);
        (result as any).generateCompletionAvailableForFinish = (itemTpl: string, value: number) =>
        {
            const condition: IQuestCondition = old(itemTpl, value); //Add old logic back in

            try
            {
                condition.onlyFoundInRaid = false;
            
                return condition;
            }
            catch (error)
            {
                this.context.logger.error("Error Details:" + "Something went wrong when trying set FiR requirement to false for repeatable quests!");
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
                return condition;
            }
        };
        // The modifier Always makes sure this replacement method is ALWAYS replaced
    }, {frequency: "Always"});
}