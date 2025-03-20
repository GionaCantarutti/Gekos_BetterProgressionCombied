using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gekos_api.Utils
{
    class ConfigHandler
    {
        public static SkillsConfig GetStatsConfig()
        {
            var req = SPT.Common.Http.RequestHandler.GetJson("/server-config-router/statsconfig");
            ConfigResponse<SkillsConfig> config = JsonConvert.DeserializeObject<ConfigResponse<SkillsConfig>>(req);
            Plugin.LogSource.LogMessage($"got the config!\n{config.Response.GlobalMultiplier}\n{config.Response.SkillMultipliers}");
            return config.Response;
        }
    }

    class ConfigResponse<T> { public T Response { get; set; } }

    class SkillsConfig
    {
        [JsonProperty("GlobalXPMultiplier")]
        public float GlobalMultiplier { get; set; }

        [JsonProperty("SkillXPMultipliers")]
        public Dictionary<string, float> SkillMultipliers { get; set; }
    }
}
