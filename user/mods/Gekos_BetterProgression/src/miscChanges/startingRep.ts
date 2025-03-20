import { IProfileSides } from "@spt/models/eft/common/tables/IProfileTemplate";
import { Context } from "../contex";

export function setStartingReputation(context: Context): void
{
    const config = context.config.overrideInitialStanding;
    const profileTemplates = context.tables.templates.profiles;

    for (const profile of Object.values(profileTemplates)) for (const template of [(profile as IProfileSides).bear, (profile as IProfileSides).usec])
    {
        template.trader.initialStanding["default"] = config.defaultOverride;
        
        for (const trader of Object.keys(context.tables.traders))
        {
            template.trader.initialStanding[trader] = config.defaultOverride;
        }

        for (const [trader, standing] of Object.entries(config.indivudalOverrides as Record<string, number>))
        {
            template.trader.initialStanding[trader] = standing;
        }
        
    }
}