using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Utils;

namespace GekosBetterProgression;

[Injectable]
public class Context
{
    
    public DatabaseService databaseService;
    public DatabaseServer databaseServer;
    public DatabaseTables tables;
    public ItemHelper itemHelper;
    public PresetHelper presetHelper;
    public ConfigServer sptConfig;
    public HashUtil hashUtil;
    public GekoConfig config;
    public AdvancedConfig advancedConfig;
    public ILoggerWrapper logger;
    public LocaleService localeService;


    public bool IsInitialized => config != null;

    public void PreInitialize(
        ItemHelper _itemHelper,
        PresetHelper _presetHelper,
        ConfigServer _sptConfig,
        HashUtil _hashUtil,
        GekoConfig _config,
        AdvancedConfig _advancedConfig,
        ILoggerWrapper _logger
    )
    {
        this.itemHelper = _itemHelper;
        this.presetHelper = _presetHelper;
        this.sptConfig = _sptConfig;
        this.hashUtil = _hashUtil;
        this.config = _config;
        this.advancedConfig = _advancedConfig;
        this.logger = _logger;
    }

    public void PostInitialize(
        DatabaseService _databaseService,
        DatabaseServer _databaseServer,
        DatabaseTables _tables,
        ILoggerWrapper _logger,
        LocaleService _localeService
    )
    {
        this.databaseService = _databaseService;
        this.databaseServer = _databaseServer;
        this.tables = _tables;
        this.logger = _logger;
        this.localeService = _localeService;
    }

}