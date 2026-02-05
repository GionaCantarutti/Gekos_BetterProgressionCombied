using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GekosBetterProgression.Changes;

internal class ContainerChanges()
{
    public static void Apply(Context context)
    {
        foreach (KeyValuePair<string, int[]> item in context.config.misc.containerSizeChanges)
        {
            if (!context.tables.Templates.Items.ContainsKey(item.Key))
            {
                continue;
            }

            int sizeH = item.Value[0];
            int sizeV = item.Value[1];
            var containerProps = context.tables.Templates.Items[item.Key]?.Properties?.Grids?.First().Properties;

            if (containerProps is null)
            {
                context.logger.Error($"Could not acces properties of container: {item.Key}");
            } else
            {
                containerProps.CellsH = sizeH;
                containerProps.CellsV = sizeV;
            }
        }
    }
}