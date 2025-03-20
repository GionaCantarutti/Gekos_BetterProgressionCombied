import fs from "fs";
// eslint-disable-next-line @typescript-eslint/naming-convention
import JSON5 from "json5";
import { DependencyContainer } from "tsyringe";
import { IPostDBLoadMod } from "@spt/models/external/IPostDBLoadMod";
import { DatabaseServer } from "@spt/servers/DatabaseServer";
import { ILogger } from "@spt/models/spt/utils/ILogger";
import { Context } from "./contex";
import { algorithmicallyRebalance } from "./algoRebalancing/core";
import { applySecureContainerChanges } from "./miscChanges/secureContainer";
import { IPreSptLoadMod } from "@spt/models/external/IPreSptLoadMod";
import { getModFolder } from "./utils";
import { changeStashProgression } from "./miscChanges/stashChanges";
import { disableFleaMarket } from "./miscChanges/fleaChanges";
import { changeHidehoutBuildCosts } from "./miscChanges/buildChanges";
import { changeSkills } from "./miscChanges/skills";
import { ItemHelper } from "@spt/helpers/ItemHelper";
import { changeCrafts } from "./miscChanges/craftChanges";
import { addSupportForGPTraders, gainRefRepOnKill, changeRefPurchasingOptions } from "./miscChanges/refRepRework";
import { buffSICCCase } from "./miscChanges/buffSICCCase";
import { removeFirFromFlea, removeFirFromHideout, removeFirFromQuests, removeFirFromRepeatables } from "./miscChanges/firChanges";
import { addCustomTrades } from "./miscChanges/customTrades";
import { changeBitcoinFarming } from "./miscChanges/bitcoinChanges";
import { changePrices } from "./miscChanges/priceChanging";
import { LoggerWrapper } from "./loggerWrapper";
import { changeStackSizes } from "./miscChanges/stackSizeChanges";
import { PresetHelper } from "@spt/helpers/PresetHelper";
import { PresetController } from "@spt/controllers/PresetController";
import { setStartingReputation } from "./miscChanges/startingRep";
import { PreSptModLoader } from "@spt/loaders/PreSptModLoader";
import { join } from "path";
import { StaticRouterModService } from "@spt/services/mod/staticRouter/StaticRouterModService";

class Mod implements IPostDBLoadMod, IPreSptLoadMod
{
    private context: Context;
    
    preSptLoad(container: DependencyContainer): void
    {
        this.context = new Context();
        const preLoader = container.resolve<PreSptModLoader>("PreSptModLoader");
        this.context.logger = new LoggerWrapper(container.resolve<ILogger>("WinstonLogger"), preLoader);

        this.safelyReadConfig();

        this.setupRouterForClientSideData(container);

        this.safeApplyEarlyModifications(container);
    }
    
    postDBLoad(container: DependencyContainer): void
    {
        this.context.database = container.resolve<DatabaseServer>("DatabaseServer");
        this.context.tables = this.context.database.getTables();
        this.context.itemHelper = container.resolve<ItemHelper>("ItemHelper");
        container.resolve<PresetController>("PresetController").initialize();
        this.context.presetHelper = container.resolve<PresetHelper>("PresetHelper");

        this.safelyApplyChanges();

        this.context.logger.success("Finished applying all changes!");
    }

    ///////////////////////////////////////////////////////////////////////////

    setupRouterForClientSideData(container: DependencyContainer): void
    {
        const staticRouter = container.resolve<StaticRouterModService>("StaticRouterModService");

        try
        {
            staticRouter.registerStaticRouter(
                "statsconfigrouter",
                [
                    {
                        url: "/server-config-router/statsconfig",
                        // eslint-disable-next-line @typescript-eslint/no-unused-vars
                        action: (_url: string, _info: any, _sessionID: string, _output: string): Promise<string> =>
                        {
                            return Promise.resolve(JSON.stringify({response: this.context.config.skillChanges.ClientConfigs}));
                        }
                    }
                ],
                "statsconfig"
            )
        }
        catch (error)
        {
            this.context.logger.error("Failed to send skills config data over to the client!");
            if (this.context.config.dev.showFullError)
            {
                this.context.logger.error("Error Details:" + error);
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }
        }
    }

    safeApplyEarlyModifications(container: DependencyContainer): void
    {
        try
        {
            if (this.context.config.refChanges.enabled && this.context.config.refChanges.refStandingOnKill.enable) gainRefRepOnKill(this.context, container);
        }
        catch (error)
        {
            this.context.logger.error("Failed to inject ref rep on PMC kill function!");
            if (this.context.config.dev.showFullError)
            {
                this.context.logger.error("Error Details:" + error);
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }
        }

        try
        {
            if (this.context.config.misc.removeFirFromQuests) removeFirFromRepeatables(this.context, container);
        }
        catch (error)
        {
            this.context.logger.error("Failed to patch daily/weekly quest generator to remove FiR requirement!");
            if (this.context.config.dev.showFullError)
            {
                this.context.logger.error("Error Details:" + error);
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }
        }

        //For ref buying items for GP coins. Requires client patching for fixing wrong GP icon
        try
        {
            addSupportForGPTraders(this.context, container);
        }
        catch (error)
        {
            this.context.logger.error("Failed to patch in support for GP trading!");
            if (this.context.config.dev.showFullError)
            {
                this.context.logger.error("Error Details:" + error);
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }
        }
        
    }

    safelyReadConfig(): void
    {
        try
        {
            const fileContent = fs.readFileSync(join(getModFolder(),"/config/config.json5"), "utf-8");
            this.context.config = JSON5.parse(fileContent);
        }
        catch (error)
        {
            this.context.logger.error("Main config file failed to load!")
            this.context.logger.error("Error Details:" + error);
            this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
        }
    }

    safelyApplyChanges(): void
    {
        const cfg = this.context.config;
        const log = cfg.dev.muteProgressOnServerLoad ? null : this.context.logger;

        log?.info("Running algorithmical rebalancing...");
        this.safelyRunIf(cfg.algorithmicalRebalancing.enable, () => algorithmicallyRebalance(this.context), "Failed to run algorithmical rebalancing!");
        log?.info("Done!");

        log?.info("Changing stack sizes...");
        this.safelyRunIf(true, () => changeStackSizes(this.context), "Failed to apply changes to stack sizes!");
        log?.info("Done!");

        log?.info("Applying secure container changes...");
        this.safelyRunIf(cfg.secureContainerProgression.enable, () => applySecureContainerChanges(this.context), "Failed to apply secure container changes!");
        log?.info("Done!");
        
        log?.info("Applying stash progression changes...");
        this.safelyRunIf(cfg.stashProgression.enable, () => changeStashProgression(this.context), "Failed to apply stash progression changes!");
        log?.info("Done!");

        log?.info("Disabling flea market...");
        this.safelyRunIf(cfg.fleaMarketChanges.disableFleaMarket, () => disableFleaMarket(this.context), "Failed to disable flea market!");
        log?.info("Done!");

        log?.info("Applying changes to hideout build costs...");
        this.safelyRunIf(cfg.hideoutBuildsChanges.enable, () => changeHidehoutBuildCosts(this.context), "Failed to apply changes to hideout build costs!");
        log?.info("Done!");

        log?.info("Applying changes to skills...");
        this.safelyRunIf(cfg.skillChanges.enable, () => changeSkills(this.context), "Failed to apply changes to skills!");
        log?.info("Done!");

        log?.info("Applying changes to craft times and output counts...");
        this.safelyRunIf(true, () => changeCrafts(this.context), "Failed to apply changes to craft times and output counts!");
        log?.info("Done!");

        log?.info("Applying changes to item prices...");
        this.safelyRunIf(true, () => changePrices(this.context), "Failed to apply changes to item prices!");
        log?.info("Done!");

        log?.info("Applying changes to SICC container...");
        this.safelyRunIf(cfg.SICCBuffs.enable, () => buffSICCCase(this.context), "Failed to apply changes to SICC container!");
        log?.info("Done!");

        log?.info("Removing FiR requirements...");
        this.safelyRunIf(cfg.misc.removeFirFromQuests, () => removeFirFromQuests(this.context), "Failed to remove FiR requirements form quests!");
        this.safelyRunIf(cfg.misc.removeFirFromHideout, () => removeFirFromHideout(this.context), "Failed to remove FiR requirements from hideout builds!");
        this.safelyRunIf(cfg.misc.removeFirFromFlea, () => removeFirFromFlea(this.context), "Failed to remove FiR requirements from flea market listings!");
        log?.info("Done!");

        log?.info("Adding custom trades...");
        this.safelyRunIf(cfg.misc.addCustomTrades, () => addCustomTrades(this.context), "Failed to add custom trades!");
        log?.info("Done!");

        log?.info("Applying changes to bitcoin farming...");
        this.safelyRunIf(cfg.bitcoinChanges.enable, () => changeBitcoinFarming(this.context), "Failed to apply changes to bitcoin farming!");
        log?.info("Done!");

        log?.info("Setting initial trader standing...");
        this.safelyRunIf(cfg.bitcoinChanges.enable, () => setStartingReputation(this.context), "Failed to set initial trader standing!");
        log?.info("Done!");

        log?.info("Applying changes to Ref item purchasing...");
        this.safelyRunIf(cfg.refChanges.enable, () => changeRefPurchasingOptions(this.context), "Failed to apply changes to Ref item purchasing!");
        log?.info("Done!");
    }

    safelyRunIf(condition: boolean, func: () => void, message: string): void
    {
        try
        {
            if (condition)
            {
                func();
            }
        }
        catch (error)
        {
            this.context.logger.error(message);
            if (this.context.config.dev.showFullError)
            {
                this.context.logger.error("Error Details:" + error);
                this.context.logger.error("Stack Trace:\n" + (error instanceof Error ? error.stack : "No stack available"));
            }
        }
    }

}

export const mod = new Mod();
