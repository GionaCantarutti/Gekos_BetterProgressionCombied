using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.DI;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Linq;
using System.ComponentModel;
using SPTarkov.Server.Core.Models.Common;

namespace GekosBetterProgression.Changes;

public class SecureContainerChanges()
{
    public static void Apply(Context context)
    {
        ApplySizeChanges(context);
        ApplyAdditionalQuestRewards(context);
        ApplyStarterContainer(context);
    }

    public static void ApplyAdditionalQuestRewards(Context context)
    {
        Utils.ApplyAdditionalQuestRewards(context, context.advancedConfig.advancedSecureContainerChanges.additionalQuestRewards);
    }

    public static void ApplySizeChanges(Context context)
    {
        foreach (KeyValuePair<string, int[][]> item in context.config.secureContainerProgression.sizeChanges)
        {
            TemplateItem containerItem = context.tables.Templates.Items[item.Key];
            Grid? gridTemplate = containerItem.Properties?.Grids?.First();
            int[][] gridSizes = item.Value;
            List<Grid> newGrids = new();

            if (gridTemplate is null)
            {
                context.logger.Error("Failed to fetch grid template");
                return;
            }

            if (containerItem.Properties is null)
            {
                context.logger.Error("Container has no properties");
                return;
            }

            for (int i = 0; i < gridSizes.Length; i++)
            {
                int cellsH = gridSizes[i][0];
                int cellsV = gridSizes[i][1];
                Grid newGrid = new();

                newGrid.Name = (i + 1).ToString();
                newGrid.Id = context.hashUtil.GetHashCode().ToString();
                newGrid.Parent = "664a55d84a90fc2c8a6305c9";
                newGrid.Properties = new GridProperties
                {
                    Filters = gridTemplate.Properties?.Filters,
                    CellsH = cellsH,
                    CellsV = cellsV,
                    MinCount = 0,
                    MaxCount = 0,
                    MaxWeight = 0,
                    IsSortingTable = false
                };
                newGrid.Prototype = "55d329c24bdc2d892f8b4567";

                newGrids.Add(newGrid);
            }

            containerItem.Properties.Grids = newGrids;
        }
    }

    public static void ApplyStarterContainer(Context context)
    {
        Dictionary<string, ProfileSides> profileTemplates = context.tables.Templates.Profiles;
        foreach (KeyValuePair<string, ProfileSides> item in profileTemplates)
        {
            Item? bearContainer = item.Value.Bear?.Character?.Inventory?.Items?.Find((Item x) => (x.SlotId == "SecuredContainer"));
            if (bearContainer is not null)
            {
                bearContainer.Template = context.config.secureContainerProgression.starterContainer;
            }

            Item? usecContainer = item.Value.Usec?.Character?.Inventory?.Items?.Find((Item x) => (x.SlotId == "SecuredContainer"));
            if (usecContainer is not null)
            {
                usecContainer.Template = context.config.secureContainerProgression.starterContainer;
            }
        }
    }
}
