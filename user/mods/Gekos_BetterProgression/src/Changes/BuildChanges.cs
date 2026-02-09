namespace GekosBetterProgression.Changes;

public class BuildChanges
{

    public static bool Apply(Context context)
    {
        GekoConfig.HideoutBuildsChanges config = context.config.hideoutBuildsChanges;

        foreach (var area in context.tables.Hideout.Areas)
        {
            foreach (var stage in area.Stages.Values)
            {
                var nonCurrencyReq = stage.Requirements.FindAll((req) => !Utils.IsCurrency(req.TemplateId));
                foreach (var requirement in nonCurrencyReq)
                {
                    if (requirement.Count != null && requirement.Count != 0)
                    {
                        float newCount = (float)requirement.Count;

                        newCount -= config.threshold;
                        if (newCount > 0)
                        {
                            newCount *= (float)config.factor;
                        }
                        newCount += config.threshold;

                        if (config.roundDown)
                        {
                            requirement.Count = (int)Math.Floor(newCount);
                        }
                        else
                        {
                            requirement.Count = (int)Math.Ceiling(newCount);
                        }

                    }
                }
            }
        }

        return true;
    }

}
