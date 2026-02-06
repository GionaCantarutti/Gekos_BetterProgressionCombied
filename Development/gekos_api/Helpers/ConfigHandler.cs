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
            SkillsConfig config = JsonConvert.DeserializeObject<SkillsConfig>(req);
            return config;
        }

        public static PointsConfig GetPointsConfig()
        {
            var req = SPT.Common.Http.RequestHandler.GetJson("/server-config-router/skillpoints");
            PointsConfig config = JsonConvert.DeserializeObject<PointsConfig>(req);
            return config;
        }
    }

    public class SkillsConfig
    {

        [JsonProperty("GlobalXPMultiplier")]
        public float GlobalMultiplier { get; set; }

        [JsonProperty("SkillXPMultipliers")]
        public Dictionary<string, float> SkillMultipliers { get; set; }

        [JsonProperty("SkillBuffMultipliers")]
        public Dictionary<string, float> BuffMultis { get; set; }

    }

    public class PointsConfig
    {
        [JsonProperty("enable")]
        public bool enable { get; set; }

        [JsonProperty("skillPointsPerLevel")]
        public float skillPointsPerLevel { get; set; }

        [JsonProperty("automaticallyRefundOverflows")]
        public bool automaticallyRefundOverflows { get; set; }

        [JsonProperty("enableDeallocation")]
        public bool enableDeallocation { get; set; }
    }
}
