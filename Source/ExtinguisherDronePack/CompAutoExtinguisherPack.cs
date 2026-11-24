using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace ExtinguisherDronePack
{
    public class CompProperties_AutoExtinguisherPack : CompProperties
    {
        public float detectRadius = ExtinguisherDronePackSettings.DefaultDetectRadius;
        public int checkInterval = ExtinguisherDronePackSettings.DefaultCheckInterval;
        public PawnKindDef droneKind;

        public CompProperties_AutoExtinguisherPack()
        {
            compClass = typeof(CompAutoExtinguisherPack);
        }
    }

    public class CompAutoExtinguisherPack : ThingComp
    {
        public CompProperties_AutoExtinguisherPack Props => (CompProperties_AutoExtinguisherPack)props;

        private bool autoDeployEnabled = true;
        private bool autoReloadEnabled = ExtinguisherDronePackSettings.DefaultAutoReload;
        private int lastAutoDeployTick = -ExtinguisherDronePackSettings.AutoDeployCooldownTicks;
        private ExtinguisherFabricator _fabricator;

        private CompApparelReloadable _chargedComp;
        public CompApparelReloadable ChargedComp => _chargedComp ??= parent.GetComp<CompApparelReloadable>();

        public ExtinguisherFabricator Fabricator => _fabricator ??= new ExtinguisherFabricator(parent);
        private float CurrentDetectRadius => ExtinguisherDronePackMod.Settings?.detectRadius ?? Props.detectRadius;

        public bool AutoReloadEnabled => autoReloadEnabled;
        public bool IsFabricating => Fabricator.IsFabricating;
        public float FabricationProgress => Fabricator.Progress;
        public float FabricationSecondsLeft => Fabricator.SecondsLeft;
        public bool IsVisuallyEmpty => ChargedComp != null && ChargedComp.RemainingCharges <= 0;
        public Graphic EmptyWorldGraphic => Fabricator.GetEmptyWorldGraphic();

        public override void PostPostMake()
        {
            base.PostPostMake();
            if (ExtinguisherDronePackMod.Settings != null)
            {
                autoReloadEnabled = ExtinguisherDronePackMod.Settings.defaultAutoReload;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref autoDeployEnabled, nameof(autoDeployEnabled), true);
            Scribe_Values.Look(ref autoReloadEnabled, nameof(autoReloadEnabled), ExtinguisherDronePackSettings.DefaultAutoReload);
            Scribe_Values.Look(ref lastAutoDeployTick, nameof(lastAutoDeployTick), -ExtinguisherDronePackSettings.AutoDeployCooldownTicks);
            Fabricator.ExposeData();
        }

        public override void CompTick()
        {
            base.CompTick();
            Fabricator.Tick(ChargedComp);
        }

        public void TickEquipped(Pawn wearer)
        {
            Fabricator.Tick(ChargedComp);

            if (!autoDeployEnabled || IsFabricating)
                return;

            if (wearer == null || !wearer.Spawned || wearer.Map == null || wearer.Dead || wearer.Downed)
                return;

            if (!wearer.IsHashIntervalTick(Props.checkInterval))
                return;

            if (Find.TickManager.TicksGame - lastAutoDeployTick < ExtinguisherDronePackSettings.AutoDeployCooldownTicks)
                return;

            if (ChargedComp == null || ChargedComp.RemainingCharges <= 0)
                return;

            float radius = CurrentDetectRadius;
            if (FireDroneDeployer.HasActiveDroneNearby(wearer.Position, wearer.Map, radius, Props.droneKind))
                return;

            Thing triggerFire = FireDroneDeployer.FindFireNearby(wearer.Position, wearer.Map, radius);
            if (triggerFire != null)
            {
                ChargedComp.UsedOnce();
                lastAutoDeployTick = Find.TickManager.TicksGame;
                Fabricator.NotifyGraphicsChanged();
                FireDroneDeployer.SpawnNearWearer(wearer, Props.droneKind, triggerFire);
            }
        }

        public void StartFabrication() => Fabricator.Start(ChargedComp);
        public void NotifyGraphicsChanged() => Fabricator.NotifyGraphicsChanged();

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            var wearer = (parent as Apparel)?.Wearer;
            if (wearer == null || !wearer.IsColonistPlayerControlled)
                yield break;

            float radius = CurrentDetectRadius;

            yield return new Command_ToggleWithRadius
            {
                defaultLabel = "ExtinguisherPack.AutoDeploy".Translate(),
                defaultDesc = "ExtinguisherPack.AutoDeploy.Desc".Translate(radius.ToString("0.#")),
                icon = ExtinguisherDronePackAssets.IconAutoDeploy,
                isActive = () => autoDeployEnabled,
                toggleAction = () => autoDeployEnabled = !autoDeployEnabled,
                onHover = () =>
                {
                    if (wearer.Spawned && wearer.Map != null)
                    {
                        GenDraw.DrawRadiusRing(wearer.Position, radius, Color.cyan);
                    }
                }
            };

            yield return new Command_Toggle
            {
                defaultLabel = "ExtinguisherPack.AutoReload".Translate(),
                defaultDesc = "ExtinguisherPack.AutoReload.Desc".Translate(),
                icon = ExtinguisherDronePackAssets.IconAutoReload,
                isActive = () => autoReloadEnabled,
                toggleAction = () => autoReloadEnabled = !autoReloadEnabled
            };

            if (ChargedComp != null && ChargedComp.RemainingCharges <= 0 && !IsFabricating)
            {
                yield return new Command_Action
                {
                    defaultLabel = "ExtinguisherPack.ManualReload".Translate(),
                    defaultDesc = "ExtinguisherPack.ManualReload.Desc".Translate(),
                    icon = ThingDefOf.ComponentIndustrial.uiIcon,
                    action = () => ExtinguisherPackReloader.OrderManualReload(wearer, ChargedComp)
                };
            }

        }
    }
}
