using Verse;

namespace ExtinguisherDronePack
{
    public class ExtinguisherDronePackSettings : ModSettings
    {
        public const float DefaultDetectRadius = 12.9f;
        public const float MinDetectRadius = 3.9f;
        public const float MaxDetectRadius = 25.9f;
        public const int DefaultCheckInterval = 60;
        public const int SpawnSearchRadius = 2;
        public const int AutoDeployCooldownTicks = 180;
        public const int DefaultFabricationTicks = 1800; // 30 секунд (60 тиков * 30)
        public const int MinFabricationTicks = 300;      // 5 секунд
        public const int MaxFabricationTicks = 7200;     // 120 секунд
        public const bool DefaultAutoReload = true;
        public const bool DefaultAutoReloadWhenDrafted = false;

        public float detectRadius = DefaultDetectRadius;
        public int fabricationTicks = DefaultFabricationTicks;
        public bool defaultAutoReload = DefaultAutoReload;
        public bool autoReloadWhenDrafted = DefaultAutoReloadWhenDrafted;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref detectRadius, nameof(detectRadius), DefaultDetectRadius);
            Scribe_Values.Look(ref fabricationTicks, nameof(fabricationTicks), DefaultFabricationTicks);
            Scribe_Values.Look(ref defaultAutoReload, nameof(defaultAutoReload), DefaultAutoReload);
            Scribe_Values.Look(ref autoReloadWhenDrafted, nameof(autoReloadWhenDrafted), DefaultAutoReloadWhenDrafted);
        }
    }
}
