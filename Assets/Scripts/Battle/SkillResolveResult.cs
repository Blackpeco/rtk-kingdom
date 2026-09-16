namespace TsOnline
{
    public readonly struct SkillHit
    {
        public readonly BattleUnit Target;
        public readonly DamageResult Result;
        public readonly int AppliedAmount;
        public readonly bool AppliedStatus;

        public SkillHit(BattleUnit target, DamageResult result, int appliedAmount, bool appliedStatus)
        {
            Target = target;
            Result = result;
            AppliedAmount = appliedAmount;
            AppliedStatus = appliedStatus;
        }
    }

    public readonly struct SkillResolveResult
    {
        public readonly bool Success;
        public readonly string FailReason;
        public readonly SkillDefinition Skill;
        public readonly SkillHit[] Hits;

        public SkillResolveResult(bool success, string failReason, SkillDefinition skill, SkillHit[] hits)
        {
            Success = success;
            FailReason = failReason;
            Skill = skill;
            Hits = hits ?? new SkillHit[0];
        }

        public static SkillResolveResult Fail(string reason)
        {
            return new SkillResolveResult(false, reason, null, new SkillHit[0]);
        }
    }
}
