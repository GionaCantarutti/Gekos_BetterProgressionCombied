using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace gekosbetterprogression;

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
    public override List<string>? Contributors { get; init; }

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

        context.PreInitialize(itemHelper, presetHelper, configServer, hashUtil, config);

        return Task.CompletedTask;
    }
}

// We want to load after PostDBModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class PostDBLoader(
    Context context,
    ISptLogger<PostDBLoader> logger,
        DatabaseService databaseService,
        DatabaseServer databaseServer
)
    : IOnLoad // Implement the `IOnLoad` interface so that this mod can do something
{

    public Task OnLoad()
    {
        if (!context.IsInitialized)
        {
            throw new Exception("Context was not initialized!");
        }
        context.PostInitialize(databaseService, databaseServer, databaseService.GetTables());
        
        logger.Success($"Test: {context.config.algorithmicalRebalancing.ammoRules.ammoBaseLoyaltyByPen[0].baseLoyalty}");

        ApplyPostDBChanges(context, logger);

        logger.Success("Geko's Better Progression finished loading!");

        return Task.CompletedTask;

    }

    private void ApplyPostDBChanges(Context context, ISptLogger<PostDBLoader> logger)
    {
        var cfg = context.config;
        var log = cfg.dev.muteProgressOnServerLoad
            ? null
            : logger;

        log?.Info("Running algorithmical rebalancing...");
        // SafelyRunIf(cfg.algorithmicalRebalancing.enable, () => AlgorithmicallyRebalance(context), "Failed to run algorithmical rebalancing!");
        log?.Success("Done!");

        log?.Info("Changing stack sizes...");
        // SafelyRunIf(true, () => ChangeStackSizes(context), "Failed to apply changes to stack sizes!");
        log?.Success("Done!");

        log?.Info("Applying secure container changes...");
        // SafelyRunIf(cfg.secureContainerProgression.enable, () => ApplySecureContainerChanges(context), "Failed to apply secure container changes!");
        log?.Success("Done!");

        log?.Info("Applying stash progression changes...");
        // SafelyRunIf(cfg.stashProgression.enable, () => ChangeStashProgression(context), "Failed to apply stash progression changes!");
        log?.Success("Done!");

        log?.Info("Disabling flea market...");
        // SafelyRunIf(cfg.fleaMarketChanges.disableFleaMarket, () => DisableFleaMarket(context), "Failed to disable flea market!");
        log?.Success("Done!");

        log?.Info("Applying changes to hideout build costs...");
        // SafelyRunIf(cfg.hideoutBuildsChanges.enable, () => ChangeHideoutBuildCosts(context), "Failed to apply changes to hideout build costs!");
        log?.Success("Done!");

        log?.Info("Applying changes to skills...");
        // SafelyRunIf(cfg.skillChanges.enable, () => ChangeSkills(context), "Failed to apply changes to skills!");
        log?.Success("Done!");

        log?.Info("Applying changes to craft times and output counts...");
        // SafelyRunIf(true, () => ChangeCrafts(context), "Failed to apply changes to craft times and output counts!");
        log?.Success("Done!");

        log?.Info("Applying changes to item prices...");
        // SafelyRunIf(true, () => ChangePrices(context), "Failed to apply changes to item prices!");
        log?.Success("Done!");

        log?.Info("Applying changes to SICC container...");
        // SafelyRunIf(cfg.SICCBuffs.enable, () => BuffSICCCase(context), "Failed to apply changes to SICC container!");
        log?.Success("Done!");

        log?.Info("Removing FiR requirements...");
        // SafelyRunIf(cfg.misc.removeFirFromQuests, () => RemoveFirFromQuests(context), "Failed to remove FiR requirements from quests!");
        // SafelyRunIf(cfg.misc.removeFirFromQuests, () => RemoveFirFromRepeatables(context), "Failed to remove FiR requirements from repeatable quests!");
        // SafelyRunIf(cfg.misc.removeFirFromHideout, () => RemoveFirFromHideout(context), "Failed to remove FiR requirements from hideout builds!");
        // SafelyRunIf(cfg.misc.removeFirFromFlea, () => RemoveFirFromFlea(context), "Failed to remove FiR requirements from flea market listings!");
        log?.Success("Done!");

        log?.Info("Adding custom trades...");
        // SafelyRunIf(cfg.misc.addCustomTrades, () => AddCustomTrades(context), "Failed to add custom trades!");
        log?.Success("Done!");

        log?.Info("Applying changes to bitcoin farming...");
        // SafelyRunIf(cfg.bitcoinChanges.enable, () => ChangeBitcoinFarming(context), "Failed to apply changes to bitcoin farming!");
        log?.Success("Done!");

        log?.Info("Setting initial trader standing...");
        // SafelyRunIf(cfg.bitcoinChanges.enable, () => SetStartingReputation(context), "Failed to set initial trader standing!");
        log?.Success("Done!");

        log?.Info("Applying changes to Ref item purchasing...");
        // SafelyRunIf(cfg.refChanges.enable, () => ChangeRefPurchasingOptions(context), "Failed to apply changes to Ref item purchasing!");
        log?.Success("Done!");

        log?.Info("Adding additional quest rewards...");
        // SafelyRunIf(cfg.misc.enableExtraQuestRewards, () => AddAdditionalQuestRewards(context), "Failed to add additional quest rewards!");
        log?.Success("Done!");
    }

    private void SafelyRunIf(bool condition, Action action, string message)
    {
        try
        {
            if (condition)
            {
                action();
            }
        }
        catch (Exception ex)
        {
            logger.Error(message);

            if (context.config.dev.showFullError)
            {
                logger.Error($"Error Details: {ex.Message}");
                logger.Error($"Stack Trace:\n{ex.StackTrace}");
            }
        }
    }

}