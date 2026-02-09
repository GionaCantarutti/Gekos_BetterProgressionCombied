namespace GekosBetterProgression.Changes;

public class AdditionalQuestRewardChanges
{
    public static bool Apply(Context context)
    {
        Utils.ApplyAdditionalQuestRewards(context, context.advancedConfig.additionalQuestRewards);
        
        return true;
    }
}