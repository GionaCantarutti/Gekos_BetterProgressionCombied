namespace GekosBetterProgression.Changes;

internal class ContainerChanges()
{
    public static bool Apply(Context context)
    {
        foreach (KeyValuePair<string, int[]> item in context.config.misc.containerSizeChanges.changes)
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

        return true;
    }
}