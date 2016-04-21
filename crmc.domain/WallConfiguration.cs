namespace crmc.domain
{
    public class WallConfiguration
    {
        public int Id { get; set; }
        public int MinFontSize { get; set; }
        public int MaxFontSize { get; set; }
        public int KioskEntryTopMargin { get; set; }
        public int ScreenBottomMargin { get; set; }
        public double GeneralRotationDelay { get; set; }
        public double PriorityRotationDelay { get; set; }
        public int KioskDisplayRecycleCount { get; set; }
        public double Volume { get; set; }

        public double GrowAnimationDuration { get; set; }
        public double ShrinkAnimationDuration { get; set; }
        public double FallAnimationDurationTimeModifier { get; set; }

        public virtual ConfigurationMode ConfigurationMode { get; set; }
        public bool IsCurrent { get; set; }
    }

    public enum ConfigurationMode
    {
        Normal,
        Backup
    }
}