import { Context } from "../contex";

export function changeSkills(context: Context): void
{
    context.tables.globals.config.SkillFreshEffectiveness = context.config.skillChanges.SkillFreshEffectiveness;
    context.tables.globals.config.SkillFreshPoints = context.config.skillChanges.SkillFreshPoints;
    context.tables.globals.config.SkillMinEffectiveness = context.config.skillChanges.SkillMinEffectiveness;
    context.tables.globals.config.SkillPointsBeforeFatigue = context.config.skillChanges.SkillPointsBeforeFatigue;
}