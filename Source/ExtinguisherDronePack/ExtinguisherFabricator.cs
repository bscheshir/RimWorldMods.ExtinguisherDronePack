using HarmonyLib;
using RimWorld;
using System.Reflection;
using Verse;
using Verse.Sound;

namespace ExtinguisherDronePack
{
    public class ExtinguisherFabricator : IExposable
    {
        private static readonly FieldInfo RemainingChargesField = AccessTools.Field(typeof(CompApparelReloadable), "remainingCharges");

        private readonly ThingWithComps parent;
        private int ticksRemaining;
        private int ticksTotal;

        public ExtinguisherFabricator(ThingWithComps parent)
        {
            this.parent = parent;
        }

        public bool IsFabricating => ticksRemaining > 0;
        public float Progress => ticksTotal > 0 ? UnityEngine.Mathf.Clamp01(1f - (float)ticksRemaining / ticksTotal) : 1f;
        public float SecondsLeft => ticksRemaining / 60f;

        public void ExposeData()
        {
            Scribe_Values.Look(ref ticksRemaining, "fabricationTicksRemaining", 0);
            Scribe_Values.Look(ref ticksTotal, "fabricationTicksTotal", 0);
        }

        public Graphic GetEmptyWorldGraphic() => ExtinguisherDronePackAssets.EmptyPackWorldGraphic ?? parent.DefaultGraphic;

        public void Start(CompApparelReloadable reloadable)
        {
            if (reloadable == null || RemainingChargesField == null)
                return;

            int current = reloadable.RemainingCharges;
            if (current > 0)
            {
                RemainingChargesField.SetValue(reloadable, current - 1);
            }

            int duration = ExtinguisherDronePackMod.Settings?.fabricationTicks ?? ExtinguisherDronePackSettings.DefaultFabricationTicks;
            ticksTotal = duration;
            ticksRemaining = duration;
            NotifyGraphicsChanged();
        }

        public void Tick(CompApparelReloadable reloadable)
        {
            if (ticksRemaining <= 0)
                return;

            ticksRemaining--;
            if (ticksRemaining > 0)
                return;

            ticksRemaining = 0;
            ticksTotal = 0;

            if (reloadable != null && RemainingChargesField != null)
            {
                int next = UnityEngine.Mathf.Min(reloadable.MaxCharges, reloadable.RemainingCharges + 1);
                RemainingChargesField.SetValue(reloadable, next);
            }

            NotifyGraphicsChanged();

            var wearer = (parent as Apparel)?.Wearer;
            if (wearer != null && wearer.Spawned)
            {
                SoundDefOf.Tick_High.PlayOneShot(new TargetInfo(wearer.Position, wearer.Map));
            }
        }

        public void NotifyGraphicsChanged()
        {
            var wearer = (parent as Apparel)?.Wearer;
            wearer?.Drawer?.renderer?.SetAllGraphicsDirty();

            if (parent.Spawned && parent.Map != null)
            {
                parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
            }
        }
    }
}
