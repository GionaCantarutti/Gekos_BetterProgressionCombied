using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace GekosBetterProgression;

// NOTE: Most of this is just dumbly translated legacy code, apologies if this sucks!
public static class StashChanges
{
    private static readonly HashSet<MongoId> STARTING_STASHES = new HashSet<MongoId>(){
        ItemTpl.STASH_STANDARD_STASH_10X30,
        ItemTpl.STASH_LEFT_BEHIND_STASH_10X40,
        ItemTpl.STASH_PREPARE_FOR_ESCAPE_STASH_10X50,
        ItemTpl.STASH_EDGE_OF_DARKNESS_STASH_10X68,
        ItemTpl.STASH_THE_UNHEARD_EDITION_STASH_10X72
    };

    private static readonly Bonus BASIC_STASH_BONUSES = new Bonus
    {
        Id = "64f5b9e5fa34f11b380756c0",
        TemplateId = ItemTpl.STASH_STANDARD_STASH_10X30,
        Type = BonusType.StashSize
    };

    public static void Apply(Context context)
    {
        foreach (var profile in context.tables.Templates.Profiles)
        {
            foreach (var side in new TemplateSide[] { profile.Value.Bear, profile.Value.Usec })
            {
                BotHideoutArea? hideoutArea = side.Character.Hideout.Areas.Find((area) => area.Type == HideoutAreas.Stash);
                hideoutArea.Level = 1;

                List<Item> startingStashItems = side.Character.Inventory.Items.FindAll((i) => STARTING_STASHES.Contains(i.Template));
                foreach (var item in startingStashItems)
                {
                    item.Template = ItemTpl.STASH_STANDARD_STASH_10X30;
                }

                side.Character.Bonuses = side.Character.Bonuses.FindAll((bonus) => bonus.Type != BonusType.StashSize);
                side.Character.Bonuses.Add(BASIC_STASH_BONUSES);
            }
        }

        Dictionary<MongoId, int> stashUpdates = new()
        {
            { ItemTpl.STASH_STANDARD_STASH_10X30, context.config.stashProgression.stashSizes[0] },
            { ItemTpl.STASH_LEFT_BEHIND_STASH_10X40, context.config.stashProgression.stashSizes[1] },
            { ItemTpl.STASH_PREPARE_FOR_ESCAPE_STASH_10X50, context.config.stashProgression.stashSizes[2] },
            { ItemTpl.STASH_EDGE_OF_DARKNESS_STASH_10X68, context.config.stashProgression.stashSizes[3] },
            { ItemTpl.STASH_THE_UNHEARD_EDITION_STASH_10X72, context.config.stashProgression.stashSizes[4] }
        };

        foreach (var stashUpdate in stashUpdates)
        {
            TemplateItem stashItem = context.tables.Templates.Items[stashUpdate.Key];
            GridProperties? stashProperties = stashItem.Properties.Grids.First().Properties;
            stashProperties.CellsV = stashUpdate.Value;
        }

        Dictionary<string, Stage>? hideoutStashStages = context.tables.Hideout.Areas.Find((area) => area.Type == HideoutAreas.Stash).Stages;
        foreach (var stage in hideoutStashStages.Values)
        {
            var currencyRequirements = stage.Requirements.FindAll((req) => Utils.IsCurrency(req.TemplateId) && req.Count != null);
            foreach (var currencyRequirement in currencyRequirements)
            {
                currencyRequirement.Count *= (int)Math.Round(context.config.stashProgression.stashUpgradeCostFactor);
            }

            var loyaltyReqs = stage.Requirements.FindAll((req) => req.LoyaltyLevel != null);
            foreach (var loyaltyReq in loyaltyReqs)
            {
                loyaltyReq.LoyaltyLevel += context.config.stashProgression.stashUpgradeLoyaltyDelta;
            }
        }
    }
}