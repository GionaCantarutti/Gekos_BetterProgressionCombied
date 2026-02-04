namespace gekosbetterprogression;

public record GekoConfig
{

    public required SecureContainerProgression secureContainerProgression { get; set; }
    public required FleaMarketChanges fleaMarketChanges { get; set; }
    public required HideoutBuildsChanges hideoutBuildsChanges { get; set; }
    public required StashProgression stashProgression { get; set; }
    public required SkillChanges skillChanges { get; set; }
    public required RefChanges refChanges { get; set; }
    public required SICCBuffs siccBuffs { get; set; }
    public required BitcoinChanges bitcoinChanges { get; set; }
    public required OverrideInitialStanding overrideInitialStanding { get; set; }
    public required Misc misc { get; set; }
    public required AlgorithmicalRebalancing algorithmicalRebalancing { get; set; }
    public required Dev dev { get; set; }

    // ===================== SECURE CONTAINER =====================

    public record SecureContainerProgression
    {
        public bool enable { get; set; }
        public required string starterContainer { get; set; }
        public required Dictionary<string, int[][]> sizeChanges { get; set; }
    }

    // ===================== FLEA =====================

    public record FleaMarketChanges
    {
        public bool disableFleaMarket { get; set; }
        public bool stillAllowKeys { get; set; }
        public required List<string> fleaWhitelist { get; set; }
    }

    // ===================== HIDEOUT =====================

    public record HideoutBuildsChanges
    {
        public bool enable { get; set; }
        public int threshold { get; set; }
        public double factor { get; set; }
        public bool roundDown { get; set; }
    }

    // ===================== STASH =====================

    public record StashProgression
    {
        public bool enable { get; set; }
        public int startingStashLevel { get; set; }
        public required List<int> stashSizes { get; set; }
        public double stashUpgradeCostFactor { get; set; }
        public int stashUpgradeLoyaltyDelta { get; set; }
    }

    // ===================== SKILLS =====================

    public record SkillChanges
    {
        public bool enable { get; set; }

        public double skillFreshEffectiveness { get; set; }
        public int skillFreshPoints { get; set; }
        public int skillPointsBeforeFatigue { get; set; }
        public double skillMinEffectiveness { get; set; }

        public required CustomMultipliers customMultipliers { get; set; }
        public required SkillPointsSystem skillPointsSystem { get; set; }

        public record CustomMultipliers
        {
            public double globalXPMultiplier { get; set; }
            public required Dictionary<string, double> skillXPMultipliers { get; set; }
            public required Dictionary<string, double> skillBuffMultipliers { get; set; }
        }

        public record SkillPointsSystem
        {
            public bool enable { get; set; }
            public double skillPointsPerLevel { get; set; }
            public bool automaticallyRefundOverflows { get; set; }
            public bool enableDeallocation { get; set; }
        }
    }

    // ===================== REF =====================

    public record RefChanges
    {
        public bool enable { get; set; }
        public bool refBuysInGPCoins { get; set; }
        public bool refOnlyBuysDogtags { get; set; }
        public bool refAlsoBuysLegaMedals { get; set; }
        public required RefStandingOnKill refStandingOnKill { get; set; }

        public record RefStandingOnKill
        {
            public bool enable { get; set; }
            public required List<KillRange> repByKillLevel { get; set; }

            public record KillRange
            {
                public required int[] levelRange { get; set; }
                public double rep { get; set; }
            }
        }
    }

    // ===================== SICC =====================

    public record SICCBuffs
    {
        public bool enable { get; set; }
        public bool canHoldWhatDocsCan { get; set; }
        public required List<string> additionalWhitelistedItems { get; set; }
    }

    // ===================== BITCOIN =====================

    public record BitcoinChanges
    {
        public bool enable { get; set; }
        public bool cannotBuyGPU { get; set; }
        public bool overrideValue { get; set; }
        public int value { get; set; }
        public double btcFarmSpeedMult { get; set; }
        public double gpuBoostRate { get; set; }
        public int btcCapacity { get; set; }
    }

    // ===================== INITIAL STANDING =====================

    public record OverrideInitialStanding
    {
        public bool enable { get; set; }
        public double defaultOverride { get; set; }
        public required Dictionary<string, double> individualOverrides { get; set; }
    }

    // ===================== MISC =====================

    public record Misc
    {
        public bool removeFirFromQuests { get; set; }
        public bool removeFirFromHideout { get; set; }
        public bool removeFirFromFlea { get; set; }

        public double craftProductMultiplier { get; set; }
        public double craftTimeMultiplier { get; set; }

        public bool enableExtraQuestRewards { get; set; }
        public bool addCustomTrades { get; set; }

        public required Dictionary<string, int> stackSizeOverride { get; set; }
        public required Dictionary<string, int[]> containerSizeChanges { get; set; }
        public required Dictionary<string, int> priceChanges { get; set; }
    }

    // ===================== ALGORITHMIC REBALANCING =====================

    public record AlgorithmicalRebalancing
    {
        public bool enable { get; set; }
        public bool clampToMaxLevel { get; set; }
        public bool forceClampingOfQuestlockedItems { get; set; }

        public double barterDelta { get; set; }
        public double questLockDelta { get; set; }
        public bool logBartersAndLocks { get; set; }

        public required List<string> excludeTraders { get; set; }
        public required Dictionary<string, double> deltaByTrader { get; set; }

        public required AmmoRules ammoRules { get; set; }
        public required WeaponRules weaponRules { get; set; }
        public required ExplicitLoyaltyDelta explicitLoyaltyDelta { get; set; }
        public required ExplicitLoyaltyOverride explicitLoyaltyOverride { get; set; }

        public record AmmoRules
        {
            public bool enable { get; set; }
            public bool logChanges { get; set; }
            public double globalDelta { get; set; }
            public double defaultBaseLoyaltyByPen { get; set; }

            public required List<AmmoPenRule> ammoBaseLoyaltyByPen { get; set; }
            public required List<CaliberRule> caliberRules { get; set; }
            public required List<DamageRule> damageRules { get; set; }

            public required List<string> ignoreCalibers { get; set; }
            public required AmmoCraftSettings craftSettings { get; set; }

            public record AmmoPenRule
            {
                public required int[] penInterval { get; set; }
                public double baseLoyalty { get; set; }
            }

            public record CaliberRule
            {
                public required string caliber { get; set; }
                public double loyaltyDelta { get; set; }
            }

            public record DamageRule
            {
                public required int[] damageInterval { get; set; }
                public double loyaltyDelta { get; set; }
            }

            public record AmmoCraftSettings
            {
                public bool enable { get; set; }
                public required List<LoyaltyRange> loyaltyToLevelRanges { get; set; }

                public record LoyaltyRange
                {
                    public required double[] range { get; set; }
                    public int level { get; set; }
                }
            }
        }

        public record WeaponRules
        {
            public bool enable { get; set; }
            public bool logChanges { get; set; }
            public double globalDelta { get; set; }
            public bool attachmentsFollowDefaultBuild { get; set; }
            public double advancedAttachmentsDelta { get; set; }

            public double defaultBaseLoyalty { get; set; }
            public required List<WeaponCaliberBase> weaponBaseLoyaltyByCaliber { get; set; }

            public required List<FireModeRule> fireModeRules { get; set; }
            public required List<FireRateRule> fireRateRules { get; set; }

            public required UpshiftRules upshiftRules { get; set; }

            public record WeaponCaliberBase
            {
                public required string caliber { get; set; }
                public double baseLoyalty { get; set; }
            }

            public record FireModeRule
            {
                public required string mode { get; set; }
                public double delta { get; set; }
            }

            public record FireRateRule
            {
                public required int[] rateInterval { get; set; }
                public double delta { get; set; }
            }

            public record UpshiftRules
            {
                public bool enable { get; set; }
                public int shiftAmount { get; set; }
                public bool shiftDownInstead { get; set; }

                public bool devideNicheByFiremode { get; set; }
                public bool devideNicheByCaliber { get; set; }
                public bool devideNicheByBarterType { get; set; }
                public bool devideNicheByQuestLock { get; set; }

                public required Dictionary<string, int> powerLevels { get; set; }
            }
        }

        public record ExplicitLoyaltyDelta
        {
            public required Dictionary<string, double> trades { get; set; }
            public required Dictionary<string, double> items { get; set; }
        }

        public record ExplicitLoyaltyOverride
        {
            public required Dictionary<string, int> trades { get; set; }
            public required Dictionary<string, int> items { get; set; }
        }
    }

    // ===================== DEV =====================

    public record Dev
    {
        public bool muteProgressOnServerLoad { get; set; }
        public bool showFullError { get; set; }
    }
}
