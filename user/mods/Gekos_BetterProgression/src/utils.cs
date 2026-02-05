using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Eft.Trade;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Helpers;
//using gekosbetterprogression.AlgoRebalancing.Types;

namespace GekosBetterProgression;

public static class Utils
{
    public static readonly HashSet<string> Currencies = new()
    {
        "5449016a4bdc2d6f028b456f", // Roubles
        "5696686a4bdc2da3298b456a", // Dollars
        "569668774bdc2da2298b4568", // Euros
        "5d235b4d86f7742e017bc88a", // GP Coin
        "6656560053eaaa7a23349c86"  // Lega Medal
    };

    private static readonly Dictionary<string, int> Purchasability = new();

    // ---------------------------------------------
    // DOGTAGS
    // ---------------------------------------------

    public static List<string> GetDogtagsList(Context context)
    {
        List<string> list = new();

        foreach (var kvp in context.tables.Templates.Items)
        {
            if (kvp.Value.Properties?.DogTagQualities != null)
            {
                list.Add(kvp.Key);
            }
        }

        return list;
    }

    // ---------------------------------------------
    // TRADES
    // ---------------------------------------------

    public static List<(Trader trader, Item trade)> FindTrades(string itemId, Context context)
    {
        List<(Trader, Item)> found = new();

        foreach (var trader in context.tables.Traders.Values)
        {
            if (trader.Assort == null)
                continue;

            foreach (var trade in trader.Assort.Items)
            {
                if (trade.Template == itemId)
                {
                    found.Add((trader, trade));
                }
            }
        }

        return found;
    }

    public static List<string> GetDefaultAttachments(string weaponId, Context context)
    {
        var presets = context.presetHelper.GetDefaultWeaponPresets();

        if (!presets.TryGetValue(weaponId, out var preset) || preset == null)
        {
            return new();
        }

        return preset.Items.Select(x => (string)x.Template).ToList();
    }

    // ---------------------------------------------
    // ATTACHMENTS
    // ---------------------------------------------

    public static List<Item> UnrollAttachments(Item item, List<Item> assort)
    {
        List<Item> attachments = new();

        var children = assort
            .Where(x => x.ParentId == item.Id && x.Template != item.Template)
            .ToList();

        attachments.AddRange(children);

        foreach (var att in children)
        {
            attachments.AddRange(UnrollAttachments(att, assort));
        }

        return attachments;
    }

    public static bool ContainsAttachment(Item item, List<Item> assort, string attachmentId, Context context)
    {
        if (!context.tables.Templates.Items.TryGetValue(item.Template, out var template))
        {
            //context.logger.Warning(
            //    $"Trader item {item._id} with table ID {item._tpl} couldn't be found in the tables!"
            //);
        }
        else
        {
            if (template.Properties?.Slots == null || !template.Properties.Slots.Any())
            {
                return false;
            }
        }

        return UnrollAttachments(item, assort)
            .Any(x => x.Template == attachmentId);
    }

    // ---------------------------------------------
    // INDEXING
    // ---------------------------------------------

    /*
    public static Dictionary<string, ChangedItem> IndexById(
        Dictionary<int, List<ChangedItem>> byTier)
    {
        Dictionary<string, ChangedItem> byId = new();

        foreach (var items in byTier.Values)
        {
            foreach (var item in items)
            {
                byId[item.trade._id] = item;
            }
        }

        return byId;
    }
    */

    // ---------------------------------------------
    // PURCHASABILITY
    // ---------------------------------------------

    /*
    public static bool CanBePurchased(
        string itemId,
        bool excludeBarters,
        bool excludeQuestlocks,
        int tierCutoff,
        List<string> skip,
        Dictionary<string, ChangedItem> tierOverrides,
        Context context)
    {
        if (Purchasability.TryGetValue(itemId, out var cached)
            && cached <= tierCutoff)
        {
            return true;
        }

        foreach (var trader in context.tables.traders.Values)
        {
            if (trader.assort == null)
                continue;

            foreach (var trade in trader.assort.items)
            {
                int loyalty =
                    tierOverrides.ContainsKey(trade._id)
                        ? tierOverrides[trade._id].score
                        : trader.assort.loyal_level_items[trade._id];

                if (LoyaltyFromScore(
                        loyalty,
                        context.config.algorithmicalRebalancing.clampToMaxLevel)
                    > tierCutoff)
                {
                    continue;
                }

                bool match =
                    trade._tpl == itemId &&
                    trader.assort.barter_scheme.ContainsKey(trade._id)
                    || ContainsAttachment(trade, trader.assort.items, itemId, context);

                if (!match)
                    continue;

                if (excludeBarters && IsBarterTrade(trade, trader)) continue;
                if (excludeQuestlocks && IsQuestLocked(trade, trader, context)) continue;
                if (skip.Contains(trade._id)) continue;

                Purchasability[itemId] = Purchasability.ContainsKey(itemId)
                    ? Math.Min(Purchasability[itemId], loyalty)
                    : loyalty;

                return true;
            }
        }

        return false;
    }

    public static bool CanAllAttachmentsBePurchased(
        IItem item,
        List<IItem> assort,
        bool excludeBarters,
        bool excludeQuestlocks,
        int tierCutoff,
        List<string> skip,
        Dictionary<string, ChangedItem> tierOverrides,
        Context context)
    {
        var attachments = UnrollAttachments(item, assort);

        foreach (var att in attachments)
        {
            if (skip.Contains(att._tpl)) continue;

            if (!CanBePurchased(
                    att._tpl,
                    excludeBarters,
                    excludeQuestlocks,
                    tierCutoff,
                    new() { item._id },
                    tierOverrides,
                    context))
            {
                return false;
            }
        }

        return true;
    }
    */

    // ---------------------------------------------
    // QUEST / HIDEOUT
    // ---------------------------------------------

    public static void LockBehindQuest(
        Context context,
        string traderId,
        string trade,
        string lockQuest,
        string itemId,
        string rewardId,
        string targetId)
    {
        var trader = context.tables.Traders[traderId];

        trader.QuestAssort["success"][trade] = lockQuest;

        var rewards = context.tables.Templates.Quests[lockQuest].Rewards?["success"];

        rewards?.Add(new Reward
        {
            Type = RewardType.AssortmentUnlock,
            Index = rewards.Count,
            TraderId = traderId,
            Target = targetId,
            Items = new()
            {
                new Item
                {
                    Id = targetId,
                    Template = itemId
                }
            },
            Id = rewardId
        });
    }

    public static void SetAreaLevelRequirement(HideoutProduction craft, int level)
    {
        foreach (var req in craft.Requirements)
        {
            if (req.RequiredLevel != null)
            {
                req.RequiredLevel = level;
            }
        }
    }

    public static bool IsQuestLockedCraft(HideoutProduction craft)
    {
        return craft.Requirements.Any(x => x.QuestId != null);
    }

    // ---------------------------------------------
    // LOCALES
    // ---------------------------------------------

    public static void AddToLocale(
        Dictionary<string, Dictionary<string, string>> locales,
        string id,
        string name,
        string shortname,
        string description)
    {
        foreach (var locale in locales.Values)
        {
            locale[$"{id} Name"] = name;
            locale[$"{id} ShortName"] = shortname;
            locale[$"{id} Description"] = description;
        }
    }

    // ---------------------------------------------
    // NICHE CHECK
    // ---------------------------------------------

    public static bool ShareSameNiche(
        Item a,
        Item b,
        Trader aTrader,
        Trader bTrader,
        Context context)
    {
        var c = context.config.algorithmicalRebalancing.weaponRules.upshiftRules;

        var aTempl = context.tables.Templates.Items[a.Template];
        var bTempl = context.tables.Templates.Items[b.Template];

        if (c.devideNicheByFiremode &&
            BestFiremode(aTempl) != BestFiremode(bTempl))
            return false;

        if (c.devideNicheByCaliber &&
            aTempl.Properties?.AmmoCaliber != bTempl.Properties?.AmmoCaliber)
            return false;

        if (c.devideNicheByBarterType &&
            IsBarterTrade(a, aTrader) != IsBarterTrade(b, bTrader))
            return false;

        if (c.devideNicheByQuestLock &&
            IsQuestLocked(a, aTrader, context) != IsQuestLocked(b, bTrader, context))
            return false;

        return true;
    }

    // ---------------------------------------------
    // LOYALTY
    // ---------------------------------------------

    public static int LoyaltyFromScore(int score, bool capToMax)
    {
        int max = capToMax ? 4 : 999;
        return Math.Max(1, Math.Min(max, (int)Math.Floor((double)score)));
    }

    public static void SetLoyalty(
        string itemId,
        int loyalty,
        Trader trader,
        bool capToMax)
    {
        trader.Assort.LoyalLevelItems[itemId] =
            LoyaltyFromScore(loyalty, capToMax);
    }

    // ---------------------------------------------
    // BARTER / QUEST LOCK
    // ---------------------------------------------

    public static bool IsBarterTrade(Item trade, Trader trader)
    {
        if (!trader.Assort.BarterScheme.TryGetValue(trade.Id, out var schemes))
            return false;

        foreach (var group in schemes)
        {
            foreach (var ask in group)
            {
                if (!Currencies.Contains(ask.Template))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsQuestLocked(
        Item trade,
        Trader trader,
        Context context)
    {
        try
        {
            var locks = trader.QuestAssort["success"].Keys
                .Concat(trader.QuestAssort["started"].Keys)
                .Concat(trader.QuestAssort["fail"].Keys);

            return locks.Contains(trade.Id);
        }
        catch (Exception ex)
        {
            //context.logger.Warning(
            //    $"Failed to fetch quest locks for {trader.base.name} ({trader.base._id})"
            //);

            //if (context.config.dev.showFullError)
            //{
            //    context.logger.Error(ex.ToString());
            //}

            return false;
        }
    }

    // ---------------------------------------------
    // PATHS
    // ---------------------------------------------

    public static string GetModFolder()
    {
        return System.IO.Path.Combine(AppContext.BaseDirectory, "..");
    }

    // ---------------------------------------------
    // FIREMODE
    // ---------------------------------------------

    public static string BestFiremode(TemplateItem item)
    {
        return PickBestFiremode(
            item.Properties.WeapFireType.ToArray<string>(),
            (item.Properties.BoltAction ?? false) || (!item.Properties.CanQueueSecondShot ?? false));
    }

    public static string PickBestFiremode(
        string[] modes,
        bool isManual)
    {
        if (modes == null)
            return "";

        Dictionary<string, int> ranks = new()
        {
            ["none"] = -9999,
            ["manual"] = 0,
            ["doublet"] = 1,
            ["semiauto"] = 1,
            ["doubleaction"] = 1,
            ["single"] = isManual ? -100 : 1,
            ["burst"] = 2,
            ["fullauto"] = 3
        };

        string best = "none";

        foreach (var mode in modes)
        {
            if (ranks[best] < ranks[mode])
            {
                best = mode;
            }
        }

        if (isManual && ranks[best] < ranks["manual"])
        {
            best = "manual";
        }

        if (best == "manual" || best == "pumpaction" || (isManual && best == "single"))
            return "manual";

        if (best == "single" || best == "doubleaction" || best == "doublet")
            return "semiauto";

        return best;
    }
}
