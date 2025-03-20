import { Context } from "../contex";
import { currencies } from "../utils";

export function changeHidehoutBuildCosts(context: Context): void
{
    const config = context.config.hideoutBuildsChanges;
    const hideoutAreas = context.tables.hideout.areas;

    for (const area of hideoutAreas) for (const stage of Object.values(area.stages))
    {
        const nonCurrencyReq = stage.requirements.filter((req) => !currencies.includes(req.templateId))
        for (const req of nonCurrencyReq)
        {
            if (req.count)
            {
                req.count -= config.threshold;
                if (req.count > 0) req.count *= config.factor;
                req.count += config.threshold;
                if (config.roundDown)
                {
                    req.count = Math.floor(req.count);
                }
                else
                {
                    req.count = Math.ceil(req.count);
                }
            }
        }
    }
}