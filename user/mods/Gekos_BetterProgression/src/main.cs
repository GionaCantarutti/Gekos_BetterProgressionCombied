using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using GekosBetterProgression.Changes;
using GekosBetterProgression.AlgoRebalance;

namespace GekosBetterProgression;

/// <summary>
/// This is the replacement for the former package.json data. This is required for all mods.
///
/// This is where we define all the metadata associated with this mod.
/// You don't have to do anything with it, other than fill it out.
/// All properties must be overriden, properties you don't use may be left null.
/// It is read by the mod loader when this mod is loaded.
/// </summary>
public record ModMetadata : AbstractModMetadata
{
    /// <summary>
    /// Any string can be used for a modId, but it should ideally be unique and not easily duplicated
    /// a 'bad' ID would be: "mymod", "mod1", "questmod"
    /// It is recommended (but not mandatory) to use the reverse domain name notation,
    /// see: https://docs.oracle.com/javase/tutorial/java/package/namingpkgs.html
    /// </summary>
    public override string ModGuid { get; init; } = "com.geko.gekosbetterprogression";

    /// <summary>
    /// The name of your mod
    /// </summary>
    public override string Name { get; init; } = "Geko's Better Progression";

    /// <summary>
    /// Who created the mod (you!)
    /// </summary>
    public override string Author { get; init; } = "DrunkGeko";

    /// <summary>
    /// A list of people who helped you create the mod
    /// </summary>
    public override List<string>? Contributors { get; init; } = ["marbL-"];

    /// <summary>
    ///  The version of the mod, follows SEMVER rules (https://semver.org/)
    /// </summary>
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");

    /// <summary>
    /// What version of SPT is your mod made for, follows SEMVER rules (https://semver.org/)
    /// </summary>
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");

    /// <summary>
    /// ModIds that you know cause problems with your mod
    /// </summary>
    public override List<string>? Incompatibilities { get; init; }

    /// <summary>
    /// ModIds your mod REQUIRES to function
    /// </summary>
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }

    /// <summary>
    /// Where to find your mod online
    /// </summary>
    public override string? Url { get; init; } = "https://forge.sp-tarkov.com/mod/2088/gekos-better-progression";

    /// <summary>
    /// Does your mod load bundles? (e.g. new weapon/armor mods)
    /// </summary>
    public override bool? IsBundleMod { get; init; } = false;

    /// <summary>
    /// What Licence does your mod use
    /// </summary>
    public override string License { get; init; } = "MIT";
}

// We want to load after PreSptModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.PreSptModLoader + 1)]
public class PreSPTLoader(
        ISptLogger<PreSPTLoader> logger,
        ItemHelper itemHelper,
        PresetHelper presetHelper,
        ProfileHelper profileHelper,
        ConfigServer configServer,
        HashUtil hashUtil,
        ModHelper modHelper,
        Context context
    ) // We inject a logger for use inside our class, it must have the class inside the diamond <> brackets
    : IOnLoad // Implement the IOnLoad interface so that this mod can do something on server load
{
    public Task OnLoad()
    {

        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        var config = modHelper.GetJsonDataFromFile<GekoConfig>(pathToMod, "config.json5");
        var advancedConfig = modHelper.GetJsonDataFromFile<AdvancedConfig>(pathToMod, "advancedConfig.json5");

        var logWrapper = new LoggerWrapper<PreSPTLoader>(logger);

        context.PreInitialize(itemHelper, presetHelper, profileHelper, configServer, hashUtil, config, advancedConfig, logWrapper);

        return Task.CompletedTask;
    }
}

// We want to load after PostDBModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.TraderRegistration + 100)] //Load a fair bit after traders are registered, so that we can safely modify their assortments
public class PostDBLoader(
    Context context,
    ISptLogger<PostDBLoader> logger,
        DatabaseService databaseService,
        DatabaseServer databaseServer,
        LocaleService localeService
)
    : IOnLoad // Implement the `IOnLoad` interface so that this mod can do something
{

    public Task OnLoad()
    {
        if (!context.IsInitialized)
        {
            throw new Exception("Context was not initialized!");
        }

        var logWrapper = new LoggerWrapper<PostDBLoader>(logger);

        context.PostInitialize(databaseService, databaseServer, databaseService.GetTables(), logWrapper, localeService);

        ApplyPostDBChanges(context);

        logger.Success("Geko's Better Progression finished loading!");

        return Task.CompletedTask;

    }

    private void ApplyPostDBChanges(Context context)
    {
        var cfg = context.config;
        var log = cfg.dev.muteProgressOnServerLoad
            ? null
            : context.logger;

        SafelyRunIf(cfg.algorithmicalRebalancing.enable, () => AlgoRebalance.Core.AlgorithmicallyRebalance(context), log,
            "Running algorithmical rebalancing...",
            "Failed to run algorithmical rebalancing!");

        // SafelyRunIf(true, () => ChangeStackSizes(context), log,
        //     "Changing stack sizes...",
        //     "Failed to apply changes to stack sizes!");

        SafelyRunIf(cfg.secureContainerProgression.enable, () => SecureContainerChanges.Apply(context), log,
            "Applying secure container changes...",
            "Failed to apply secure container changes!");

        SafelyRunIf(cfg.stashProgression.enable, () => StashChanges.Apply(context), log,
            "Applying stash progression changes...",
            "Failed to apply stash progression changes!");

        SafelyRunIf(cfg.fleaMarketChanges.disableFleaMarket, () => FleaChanges.Apply(context), log,
            "Disabling flea market...",
            "Failed to disable flea market!");

        SafelyRunIf(cfg.hideoutBuildsChanges.enable, () => BuildChanges.Apply(context), log,
            "Applying changes to hideout build costs...",
            "Failed to apply changes to hideout build costs!");

        SafelyRunIf(cfg.skillChanges.enable, () => SkillChanges.Apply(context), log,
            "Applying changes to skills...",
            "Failed to apply changes to skills!");

        SafelyRunIf(true, () => CraftingChanges.Apply(context), log,
            "Applying changes to craft times and output counts...",
            "Failed to apply changes to craft times and output counts!");

        SafelyRunIf(true, () => PriceChanges.Apply(context), log,
            "Applying changes to item prices...",
            "Failed to apply changes to item prices!");

        SafelyRunIf(cfg.siccBuffs.enable, () => SICCCaseChanges.Apply(context), log,
            "Applying changes to SICC container...",
            "Failed to apply changes to SICC container!");

        SafelyRunIf(cfg.misc.removeFirFromQuests, () => FirChanges.RemoveFirFromQuests(context), log,
            "Removing FiR requirements from quests...",
            "Failed to remove FiR requirements from quests!");
            
        SafelyRunIf(cfg.misc.removeFirFromQuests, () => FirChanges.RemoveFirFromRepeatables(context), log,
            "Removing FiR requirements from repeatable quests...",
            "Failed to remove FiR requirements from repeatable quests!");

        SafelyRunIf(cfg.misc.removeFirFromHideout, () => FirChanges.RemoveFirFromHideout(context), log,
            "Removing FiR requirements from hideout builds...",
            "Failed to remove FiR requirements from hideout builds!");

        SafelyRunIf(cfg.misc.removeFirFromFlea, () => FirChanges.RemoveFirFromFlea(context), log,
            "Removing FiR requirements from flea market listings...",
            "Failed to remove FiR requirements from flea market listings!");

        SafelyRunIf(cfg.misc.addCustomTrades, () => AdditionalItemsChanges.Apply(context), log,
            "Adding custom items...",
            "Failed to add custom items!");

        SafelyRunIf(cfg.misc.addCustomTrades, () => AdditionalTradesChanges.Apply(context), log,
            "Adding custom trades...",
            "Failed to add custom trades!");

        SafelyRunIf(cfg.bitcoinChanges.enable, () => BitcoinChanges.Apply(context), log,
            "Applying changes to bitcoin farming...",
            "Failed to apply changes to bitcoin farming!");

        SafelyRunIf(cfg.overrideInitialStanding.enable, () => TraderStartRepChanges.Apply(context), log,
            "Setting initial trader standing...",
            "Failed to set initial trader standing!");

        SafelyRunIf(cfg.refChanges.enable, () => RefChanges.Apply(context), log,
            "Applying changes to Ref item purchasing...",
            "Failed to apply changes to Ref item purchasing!");

        SafelyRunIf(cfg.misc.enableExtraQuestRewards, () => AdditionalQuestRewardChanges.Apply(context), log,
            "Adding additional quest rewards...",
            "Failed to add additional quest rewards!");

        SafelyRunIf(cfg.misc.containerSizeChanges.enable, () => ContainerChanges.Apply(context), log,
            "Applying changes to container sizes...",
            "Failed to change sizes of containers!");
    }

    //ToDo: incorporate success logging into this, depending on success return value from called function
    private void SafelyRunIf(bool condition, Func<bool> action, ILoggerWrapper? log, string progressMessage, string failMessage)
    {
        try
        {
            log?.Info(progressMessage);

            if (condition)
            {
                if (action())
                {
                    //log?.Success("Done!");
                }
            }
            
        }
        catch (Exception ex)
        {
            logger.Error(failMessage);

            if (context.config.dev.showFullError)
            {
                logger.Error($"Error Details: {ex.Message}");
                logger.Error($"Stack Trace:\n{ex.StackTrace}");
            }
        }
    }
}