import { IItem } from "@spt/models/eft/common/tables/IItem";
import { ITrader } from "@spt/models/eft/common/tables/ITrader";

export class ChangedItem
{
    trade: IItem; score: number; trader: ITrader; logChange: boolean; isWeapon: boolean;

    constructor(trade: IItem, score: number, trader: ITrader, logChange: boolean, isWeapon: boolean)
    {
        this.trade = trade;
        this.score = score;
        this.trader = trader;
        this.logChange = logChange;
        this.isWeapon = isWeapon;
    }
}