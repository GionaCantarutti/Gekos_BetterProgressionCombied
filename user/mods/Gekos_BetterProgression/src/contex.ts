import { ItemHelper } from "@spt/helpers/ItemHelper";
import { IDatabaseTables } from "@spt/models/spt/server/IDatabaseTables";
import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { LoggerWrapper } from "./loggerWrapper";
import { PresetHelper } from "@spt/helpers/PresetHelper";

export class Context
{

    public database: DatabaseServer;
    public tables: IDatabaseTables;
    public logger: LoggerWrapper;
    public itemHelper: ItemHelper;
    public presetHelper: PresetHelper;
    public config;

    constructor(
        _config?: any, /*Not sure how to type this, dynamically type checked languages are weird*/
        _database?: DatabaseServer, _tables?: IDatabaseTables, _logger?: LoggerWrapper, _itemHelper?: ItemHelper, _preetHelper?: PresetHelper
    )
    {
        this.database = _database;
        this.tables = _tables;
        this.logger = _logger;
        this.config = _config;
        this.itemHelper = _itemHelper;
        this.presetHelper = _preetHelper;
    }

    public logObject(object: any): void
    {
        this.logger?.info(JSON.stringify(object, null, 2));
    }

}