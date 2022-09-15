namespace Umbrellas_Rebooted
{
    public class UmbrellaConfig
    {
        internal bool RedrawEnabled { get; set; } = true;
        public bool EnableWetness { get; set; } = false;
        public bool NudityCompatibility { get; set; } = false;

        public float StaminaDrainRate { get; set; } = 2.0f;

        public float BestHatsProtection { get; set; } = 0.9f;
        public float GoodHatsProtection { get; set; } = 0.5f;
        public float ShirtsProtection { get; set; } = 0.2f;

        public string GoodRainHats { get; set; } = string.Empty;
        public string BestRainHats { get; set; } = string.Empty;
        public string ShirtNames { get; set; } = string.Empty;

        public string ExceptionLocationNames { get; set; } = string.Empty;
    }
}
