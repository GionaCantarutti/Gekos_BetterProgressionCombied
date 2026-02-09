using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;

namespace GekosBetterProgression.AlgoRebalance;

public static class Ammo
{
    public static float CalculateAmmoLoyalty(Item item, Context context)
    {
        var itemTemplate = context.tables.Templates.Items[item.Template];
        return ScoreAmmo(itemTemplate, context);
    }

    public static float ScoreAmmo(TemplateItem itemTemplate, Context context)
    {
        var config = context.config.algorithmicalRebalancing.ammoRules;

        float loyalty = (float)config.defaultBaseLoyaltyByPen;

        var props = itemTemplate.Properties;
        if (props != null)
        {
            // Base level from penetration
            foreach (var rule in config.ammoBaseLoyaltyByPen)
            {
                if (props.PenetrationPower >= rule.penInterval[0] && props.PenetrationPower < rule.penInterval[1])
                {
                    loyalty = (float)rule.baseLoyalty;
                }
            }

            // Modify by caliber
            foreach (var rule in config.caliberRules)
            {
                if (props.Caliber == rule.caliber)
                {
                    loyalty += (float)rule.loyaltyDelta;
                }
            }

            // Modify by damage (accounting for projectile count)
            foreach (var rule in config.damageRules)
            {
                var totalDamage = props.Damage * props.ProjectileCount;
                if (totalDamage >= rule.damageInterval[0] && totalDamage < rule.damageInterval[1])
                {
                    loyalty += (float)rule.loyaltyDelta;
                }
            }
        }

        loyalty += (float)config.globalDelta;

        return loyalty;
    }

    public static void RebalanceAmmoCrafts(Context context)
    {
        var config = context.config.algorithmicalRebalancing.ammoRules;

        List<HideoutProduction>? crafts = context.tables.Hideout.Production.Recipes;

        if (crafts == null)
        {
            context.logger.Warning("Failed to fetch hideout crafts");
            return;
        }

        foreach (var craft in crafts)
        {
            var ammoId = craft.EndProduct;
            if (!context.itemHelper.IsOfBaseclass(ammoId, BaseClasses.AMMO)) continue;

            float score = ScoreAmmo(context.tables.Templates.Items[ammoId], context);

            if (Utils.IsQuestLockedCraft(craft)) score += (float)context.config.algorithmicalRebalancing.questLockDelta;

            foreach (var map in config.craftSettings.loyaltyToLevelRanges)
            {
                if (score >= map.range[0] && score < map.range[1])
                {
                    Utils.SetAreaLevelRequirement(craft, map.level);
                }
            }
        }
    }
}
