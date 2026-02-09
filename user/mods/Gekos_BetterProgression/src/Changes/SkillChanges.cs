namespace GekosBetterProgression.Changes;

public class SkillChanges()
{
    public static bool Apply(Context context)
    {
        var skillConfig = context.config.skillChanges;

        SPTarkov.Server.Core.Models.Eft.Common.Config eftConfig = context.databaseService.GetGlobals().Configuration;
        eftConfig.SkillFreshEffectiveness = skillConfig.skillFreshEffectiveness;
        eftConfig.SkillFreshPoints = skillConfig.skillFreshPoints;
        eftConfig.SkillPointsBeforeFatigue = skillConfig.skillPointsBeforeFatigue;
        eftConfig.SkillMinEffectiveness = skillConfig.skillMinEffectiveness;
        
        return true;
    }
}
