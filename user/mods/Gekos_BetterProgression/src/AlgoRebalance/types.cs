using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace GekosBetterProgression.AlgoRebalance;

public class ChangedItem
{
    public Item trade;
    public float score;
    public Trader trader;
    public bool logChange;
    public bool isWeapon;
    
    public ChangedItem(Item _trade, float _score, Trader _trader, bool _logChange, bool _isWeapon)
    {
        this.trade = _trade;
        this.score = _score;
        this.trader = _trader;
        this.logChange = _logChange;
        this.isWeapon = _isWeapon;
    }
}