namespace GekosBetterProgression.Changes;

public class AdditionalQuestRewardChanges
{
    public static void Apply(Context context)
    {
        Utils.ApplyAdditionalQuestRewards(context, context.advancedConfig.additionalQuestRewards);
    }
}