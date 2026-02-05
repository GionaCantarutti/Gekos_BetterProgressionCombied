using System.Collections.Generic;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression;

public record AdvancedConfig
{
    public required AdvancedSecureContainerChanges advancedSecureContainerChanges { get; init; }

    public record AdvancedSecureContainerChanges
    {
        public required AdditionalQuestRewards additionalQuestRewards { get; init; }

        public record AdditionalQuestRewards
        {
            // Key = questId
            public required Dictionary<string, Reward> started { get; init; }

            // Key = questId
            public required Dictionary<string, Reward> success { get; init; }

        }

    }

}