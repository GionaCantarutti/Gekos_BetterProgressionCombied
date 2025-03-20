import { Context } from "../contex";

export function changeStackSizes(context: Context): void
{
    //Change stack sizes
    for (const [item, stackSize] of Object.entries(context.config.misc.stackSizeOverride as Record<string, number>))
    {
        context.tables.templates.items[item]._props.StackMaxSize = stackSize;
    }
}