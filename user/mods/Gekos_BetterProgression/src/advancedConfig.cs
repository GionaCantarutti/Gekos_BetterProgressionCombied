using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Server;

namespace GekosBetterProgression;

public record AdvancedConfig
{
    public required AdvancedSecureContainerChanges advancedSecureContainerChanges { get; init; }
    public required AdditionalQuestRewards additionalQuestRewards { get; init; }
    public required Dictionary<string, CustomTrade> customTrades { get; init; }
    public required Dictionary<string, TemplateItem> customItems { get; init; }
    public required Dictionary<string, List<Buff>> customBuffs { get; init; }
    public required Dictionary<string, CustomLocale> customLocales { get; init; }

    public record AdvancedSecureContainerChanges
    {
        public required AdditionalQuestRewards additionalQuestRewards { get; init; }
    }

    public record AdditionalQuestRewards
    {
        // Key = questId
        public required Dictionary<string, Reward> started { get; init; }

        // Key = questId
        public required Dictionary<string, Reward> success { get; init; }
    }

    public record CustomTrade
    {
        public required List<Item> items { get; init; }
        public required Dictionary<string, List<List<BarterScheme>>> barterScheme { get; init; }
        public required Dictionary<string, int> loyalLevelItems { get; init; }
        public required Dictionary<string, QuestLock> questLocks { get; init; }
    }

    public record CustomLocale
    {
        public required string Name { get; init; }
        public required string ShortName { get; init; }
        public required string Description { get; init; }
    }

    public record QuestLock
    {
        public required string quest { get; init; }
        public required string rewardId { get; init; }
        public required string targetId { get; init; }
    }
}