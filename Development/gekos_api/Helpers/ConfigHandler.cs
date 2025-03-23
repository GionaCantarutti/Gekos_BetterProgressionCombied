using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gekos_api.Helpers
{
    class ConfigHandler
    {
        public static SkillsConfig GetSkillsConfig()
        {
            var req = SPT.Common.Http.RequestHandler.GetJson("/server-config-router/skillsconfig");
            ConfigResponse<SkillsConfig> config = JsonConvert.DeserializeObject<ConfigResponse<SkillsConfig>>(req);
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

        [JsonProperty("SkillBuffMultipliers")]
        public Dictionary<string, float> BuffMultis { get; set; }
    }
}
