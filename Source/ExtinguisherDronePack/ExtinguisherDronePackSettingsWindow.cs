using UnityEngine;
using Verse;

namespace ExtinguisherDronePack
{
    public partial class ExtinguisherDronePackMod
    {
        public override string SettingsCategory() => "ExtinguisherPack.SettingsTitle".Translate();

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("ExtinguisherPack.Settings.DetectRadius".Translate(Settings.detectRadius.ToString("0.0")));
            Settings.detectRadius = listing.Slider(
                Settings.detectRadius,
                ExtinguisherDronePackSettings.MinDetectRadius,
                ExtinguisherDronePackSettings.MaxDetectRadius
            );

            listing.Gap(12f);
            float fabSeconds = Settings.fabricationTicks / 60f;
            listing.Label("ExtinguisherPack.Settings.FabricationTime".Translate(fabSeconds.ToString("0.0")));
            int newFabTicks = Mathf.RoundToInt(listing.Slider(
                Settings.fabricationTicks,
                ExtinguisherDronePackSettings.MinFabricationTicks,
                ExtinguisherDronePackSettings.MaxFabricationTicks
            ) / 60f) * 60;
            Settings.fabricationTicks = newFabTicks;

            listing.Gap(12f);
            listing.CheckboxLabeled(
                "ExtinguisherPack.Settings.DefaultAutoReload".Translate(),
                ref Settings.defaultAutoReload,
                "ExtinguisherPack.Settings.DefaultAutoReload.Desc".Translate()
            );

            listing.Gap(8f);
            listing.CheckboxLabeled(
                "ExtinguisherPack.Settings.AutoReloadWhenDrafted".Translate(),
                ref Settings.autoReloadWhenDrafted,
                "ExtinguisherPack.Settings.AutoReloadWhenDrafted.Desc".Translate()
            );

            listing.Gap(12f);
            if (listing.ButtonText("ExtinguisherPack.Settings.Reset".Translate()))
            {
                Settings.detectRadius = ExtinguisherDronePackSettings.DefaultDetectRadius;
                Settings.fabricationTicks = ExtinguisherDronePackSettings.DefaultFabricationTicks;
                Settings.defaultAutoReload = ExtinguisherDronePackSettings.DefaultAutoReload;
                Settings.autoReloadWhenDrafted = ExtinguisherDronePackSettings.DefaultAutoReloadWhenDrafted;
            }

            listing.End();
        }
    }
}
