using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;

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
    ISptLogger<PreSPTLoader> logger) // We inject a logger for use inside our class, it must have the class inside the diamond <> brackets
    : IOnLoad // Implement the IOnLoad interface so that this mod can do something on server load
{
    public Task OnLoad()
    {

    }
}

// We want to load after PostDBModLoader is complete, so we set our type priority to that, plus 1.
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class PostDBLoader(
    ISptLogger<PostDBLoader> logger, // We are injecting a logger similar to example 1, but notice the class inside <> is different
    DatabaseService databaseService)
    : IOnLoad // Implement the `IOnLoad` interface so that this mod can do something
{
    public Task OnLoad()
    {

    }

}