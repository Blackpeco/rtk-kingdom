namespace TsOnline
{
    /// <summary>
    /// Immutable description of an elemental status. Step 1 data only.
    /// </summary>
    public readonly struct ElementStatusSpec
    {
        public readonly ElementType SourceElement;
        public readonly ElementStatusKind Kind;
        public readonly int DurationTurns;
        public readonly string DescriptionEn;
        public readonly string DescriptionTh;

        public ElementStatusSpec(
            ElementType sourceElement,
            ElementStatusKind kind,
            int durationTurns,
            string descriptionEn,
            string descriptionTh)
        {
            SourceElement = sourceElement;
            Kind = kind;
            DurationTurns = durationTurns;
            DescriptionEn = descriptionEn;
            DescriptionTh = descriptionTh;
        }
    }
}
