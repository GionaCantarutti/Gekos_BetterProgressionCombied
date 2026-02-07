using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GekosBetterProgression
{
    [Injectable]
    public class Router(JsonUtil jsonUtil, Callbacks callbacks) : StaticRouter(jsonUtil, [
            new RouteAction<EmptyRequestData>(
                "/server-config-router/skillpoints", async (_, _, _, _) => await callbacks.HandleGetSkillPointConfig()
            ),
            new RouteAction<EmptyRequestData>(
                "/server-config-router/skillsconfig", async (_, _, _, _) => await callbacks.HandleGetSkillsConfig()
            )
        ])
    { }

    [Injectable]
    public class Callbacks(JsonUtil jsonUtil, HttpResponseUtil httpResponseUtil, Context context)
    {
        public ValueTask<string> HandleGetSkillPointConfig()
        {
            return new ValueTask<string>(jsonUtil.Serialize(context.config.skillChanges.skillPointsSystem));
        }

        public ValueTask<string> HandleGetSkillsConfig()
        {
            return new ValueTask<string>(jsonUtil.Serialize(context.config.skillChanges.customMultipliers));
        }
    }
}
