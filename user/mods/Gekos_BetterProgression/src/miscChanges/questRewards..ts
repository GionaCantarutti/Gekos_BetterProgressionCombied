import { IReward } from "@spt/models/eft/common/tables/IReward";
import { Context } from "../contex";
import fs from "fs";
// eslint-disable-next-line @typescript-eslint/naming-convention
import JSON5 from "json5";
import { getModFolder } from "../utils";

export function addAdditionalQuestRewards(context: Context): void
{

    //Read rewards config
    const fileContent = fs.readFileSync(getModFolder() + "/config/advanced/additionalQuestRewards.json5", "utf-8");
    const secureConfig = JSON5.parse(fileContent);

    for (const [condition, questsWithRewards] of Object.entries(secureConfig))
    {
        for (const [quest, extraReward] of Object.entries(questsWithRewards))
        {
            if (condition == "Success") context.tables.templates.quests[quest].rewards.Success.push(extraReward as IReward);
            if (condition == "Started") context.tables.templates.quests[quest].rewards.Started.push(extraReward as IReward);
        }
    }

}