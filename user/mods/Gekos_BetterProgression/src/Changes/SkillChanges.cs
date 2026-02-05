using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPTarkov.Server.Core.Models.Eft;

namespace GekosBetterProgression.Changes;

public class SkillChanges()
{
    public static void Apply(Context context)
    {
        var skillConfig = context.config.skillChanges;

        SPTarkov.Server.Core.Models.Eft.Common.Config eftConfig = context.databaseService.GetGlobals().Configuration;
        eftConfig.SkillFreshEffectiveness = skillConfig.skillFreshEffectiveness;
        eftConfig.SkillFreshPoints = skillConfig.skillFreshPoints;
        eftConfig.SkillPointsBeforeFatigue = skillConfig.skillPointsBeforeFatigue;
        eftConfig.SkillMinEffectiveness = skillConfig.skillMinEffectiveness;
        
    }
}
