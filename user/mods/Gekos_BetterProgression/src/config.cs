namespace gekosbetterprogression;

 public class Config
{

    public static Config ParseConfig(string path) {
        var json = File.ReadAllText(path);

        var settings = new JsonSerializerSettings
        {
            CommentHandling = CommentHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Ignore,
            FloatParseHandling = FloatParseHandling.Double
        };

        return JsonConvert.DeserializeObject<Config>(json, settings);
    }

    public SecureContainerProgression SecureContainerProgression { get; set; }
    public FleaMarketChanges FleaMarketChanges { get; set; }
    public HideoutBuildsChanges HideoutBuildsChanges { get; set; }
    public StashProgression StashProgression { get; set; }
    public SkillChanges SkillChanges { get; set; }
    public RefChanges RefChanges { get; set; }
    public SICCBuffs SICCBuffs { get; set; }
    public BitcoinChanges BitcoinChanges { get; set; }
    public OverrideInitialStanding OverrideInitialStanding { get; set; }
    public Misc Misc { get; set; }
    public AlgorithmicalRebalancing AlgorithmicalRebalancing { get; set; }
    public Dev Dev { get; set; }

    // =====================================
    // SECURE CONTAINER
    // =====================================

    public class SecureContainerProgression
    {
        public bool Enable { get; set; }
        public string StarterContainer { get; set; }
        public Dictionary<string, int[][]> SizeChanges { get; set; }
    }

    // =====================================
    // FLEA
    // =====================================

    public class FleaMarketChanges
    {
        public bool DisableFleaMarket { get; set; }
        public bool StillAllowKeys { get; set; }
        public List<string> FleaWhitelist { get; set; }
    }

    // =====================================
    // HIDEOUT BUILDS
    // =====================================

    public class HideoutBuildsChanges
    {
        public bool Enable { get; set; }
        public int Threshold { get; set; }
        public double Factor { get; set; }
        public bool RoundDown { get; set; }
    }

    // =====================================
    // STASH
    // =====================================

    public class StashProgression
    {
        public bool Enable { get; set; }
        public int StartingStashLevel { get; set; }
        public List<int> StashSizes { get; set; }
        public double StashUpgradeCostFactor { get; set; }
        public int StashUpgradeLoyaltyDelta { get; set; }
    }

    // =====================================
    // SKILLS
    // =====================================

    public class SkillChanges
    {
        public bool Enable { get; set; }

        public double SkillFreshEffectiveness { get; set; }
        public int SkillFreshPoints { get; set; }
        public int SkillPointsBeforeFatigue { get; set; }
        public double SkillMinEffectiveness { get; set; }

        public CustomMultipliers CustomMultipliers { get; set; }
        public SkillPointsSystem SkillPointsSystem { get; set; }

        public class CustomMultipliers
        {
            public double GlobalXPMultiplier { get; set; }
            public Dictionary<string, double> SkillXPMultipliers { get; set; }
            public Dictionary<string, double> SkillBuffMultipliers { get; set; }
        }

        public class SkillPointsSystem
        {
            public bool Enable { get; set; }
            public double SkillPointsPerLevel { get; set; }
            public bool AutomaticallyRefundOverflows { get; set; }
            public bool EnableDeallocation { get; set; }
        }
    }

    // =====================================
    // REF
    // =====================================

    public class RefChanges
    {
        public bool Enable { get; set; }
        public bool RefBuysInGPCoins { get; set; }
        public bool RefOnlyBuysDogtags { get; set; }
        public bool RefAlsoBuysLegaMedals { get; set; }
        public RefStandingOnKill RefStandingOnKill { get; set; }

        public class RefStandingOnKill
        {
            public bool Enable { get; set; }
            public List<KillRange> RepByKillLevel { get; set; }

            public class KillRange
            {
                public int[] LevelRange { get; set; }
                public double Rep { get; set; }
            }
        }
    }

    // =====================================
    // SICC
    // =====================================

    public class SICCBuffs
    {
        public bool Enable { get; set; }
        public bool CanHoldWhatDocsCan { get; set; }
        public List<string> AdditionalWhitelistedItems { get; set; }
    }

    // =====================================
    // BITCOIN
    // =====================================

    public class BitcoinChanges
    {
        public bool Enable { get; set; }
        public bool CannotBuyGPU { get; set; }
        public bool OverrideValue { get; set; }
        public int Value { get; set; }
        public double BtcFarmSpeedMult { get; set; }
        public double GpuBoostRate { get; set; }
        public int BtcCapacity { get; set; }
    }

    // =====================================
    // INITIAL STANDING
    // =====================================

    public class OverrideInitialStanding
    {
        public bool Enable { get; set; }
        public double DefaultOverride { get; set; }
        public Dictionary<string, double> IndividualOverrides { get; set; }
    }

    // =====================================
    // MISC
    // =====================================

    public class Misc
    {
        public bool RemoveFirFromQuests { get; set; }
        public bool RemoveFirFromHideout { get; set; }
        public bool RemoveFirFromFlea { get; set; }

        public double CraftProductMultiplier { get; set; }
        public double CraftTimeMultiplier { get; set; }

        public bool EnableExtraQuestRewards { get; set; }
        public bool AddCustomTrades { get; set; }

        public Dictionary<string, int> StackSizeOverride { get; set; }
        public Dictionary<string, int[]> ContainerSizeChanges { get; set; }
        public Dictionary<string, int> PriceChanges { get; set; }
    }

    // =====================================
    // ALGORITHMIC REBALANCING
    // =====================================

    public class AlgorithmicalRebalancing
    {
        public bool Enable { get; set; }
        public bool ClampToMaxLevel { get; set; }
        public bool ForceClampingOfQuestlockedItems { get; set; }

        public double BarterDelta { get; set; }
        public double QuestLockDelta { get; set; }
        public bool LogBartersAndLocks { get; set; }

        public List<string> ExcludeTraders { get; set; }
        public Dictionary<string, double> DeltaByTrader { get; set; }

        public AmmoRules AmmoRules { get; set; }
        public WeaponRules WeaponRules { get; set; }
        public ExplicitLoyaltyDelta ExplicitLoyaltyDelta { get; set; }
        public ExplicitLoyaltyOverride ExplicitLoyaltyOverride { get; set; }

        // -------- AMMO --------

        public class AmmoRules
        {
            public bool Enable { get; set; }
            public bool LogChanges { get; set; }
            public double GlobalDelta { get; set; }
            public double DefaultBaseLoyaltyByPen { get; set; }

            public List<AmmoPenRule> AmmoBaseLoyaltyByPen { get; set; }
            public List<CaliberRule> CaliberRules { get; set; }
            public List<DamageRule> DamageRules { get; set; }

            public List<string> IgnoreCalibers { get; set; }
            public AmmoCraftSettings CraftSettings { get; set; }

            public class AmmoPenRule
            {
                public int[] PenInterval { get; set; }
                public double BaseLoyalty { get; set; }
            }

            public class CaliberRule
            {
                public string Caliber { get; set; }
                public double LoyaltyDelta { get; set; }
            }

            public class DamageRule
            {
                public int[] DamageInterval { get; set; }
                public double LoyaltyDelta { get; set; }
            }

            public class AmmoCraftSettings
            {
                public bool Enable { get; set; }
                public List<LoyaltyRange> LoyaltyToLevelRanges { get; set; }

                public class LoyaltyRange
                {
                    public double[] Range { get; set; }
                    public int Level { get; set; }
                }
            }
        }

        // -------- WEAPONS --------

        public class WeaponRules
        {
            public bool Enable { get; set; }
            public bool LogChanges { get; set; }
            public double GlobalDelta { get; set; }
            public bool AttachmentsFollowDefaultBuild { get; set; }
            public double AdvancedAttachmentsDelta { get; set; }

            public double DefaultBaseLoyalty { get; set; }
            public List<WeaponCaliberBase> WeaponBaseLoyaltyByCaliber { get; set; }

            public List<FireModeRule> FireModeRules { get; set; }
            public List<FireRateRule> FireRateRules { get; set; }

            public UpshiftRules UpshiftRules { get; set; }

            public class WeaponCaliberBase
            {
                public string Caliber { get; set; }
                public double BaseLoyalty { get; set; }
            }

            public class FireModeRule
            {
                public string Mode { get; set; }
                public double Delta { get; set; }
            }

            public class FireRateRule
            {
                public int[] RateInterval { get; set; }
                public double Delta { get; set; }
            }

            public class UpshiftRules
            {
                public bool Enable { get; set; }
                public int ShiftAmount { get; set; }
                public bool ShiftDownInstead { get; set; }

                public bool DevideNicheByFiremode { get; set; }
                public bool DevideNicheByCaliber { get; set; }
                public bool DevideNicheByBarterType { get; set; }
                public bool DevideNicheByQuestLock { get; set; }

                public Dictionary<string, int> PowerLevels { get; set; }
            }
        }

        // -------- EXPLICIT --------

        public class ExplicitLoyaltyDelta
        {
            public Dictionary<string, double> Trades { get; set; }
            public Dictionary<string, double> Items { get; set; }
        }

        public class ExplicitLoyaltyOverride
        {
            public Dictionary<string, int> Trades { get; set; }
            public Dictionary<string, int> Items { get; set; }
        }
    }

    // =====================================
    // DEV
    // =====================================

    public class Dev
    {
        public bool MuteProgressOnServerLoad { get; set; }
        public bool ShowFullError { get; set; }
    }
}