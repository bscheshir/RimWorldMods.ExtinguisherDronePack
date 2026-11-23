using UnityEngine;
using Verse;

namespace ExtinguisherDronePack
{
    public partial class ExtinguisherDronePackMod
    {
        public override string SettingsCategory()
        {
            return "Extinguisher Drone Pack";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("ExtinguisherPack.Settings.DetectRadius".Translate(Settings.detectRadius.ToString("0.#")));
            Settings.detectRadius = listing.Slider(
                Settings.detectRadius,
                ExtinguisherDronePackSettings.MinDetectRadius,
                ExtinguisherDronePackSettings.MaxDetectRadius
            );

            listing.Gap();
            listing.CheckboxLabeled(
                "ExtinguisherPack.Settings.DefaultAutoReload".Translate(),
                ref Settings.defaultAutoReload,
                "ExtinguisherPack.Settings.DefaultAutoReload.Desc".Translate()
            );

            listing.End();
            base.DoSettingsWindowContents(inRect);
        }
    }
}
