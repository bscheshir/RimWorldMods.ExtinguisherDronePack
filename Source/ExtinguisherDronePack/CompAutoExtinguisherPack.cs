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

        private CompApparelReloadable _chargedComp;
        private CompApparelReloadable ChargedComp => _chargedComp ??= parent.GetComp<CompApparelReloadable>();

        private float CurrentDetectRadius => ExtinguisherDronePackMod.Settings?.detectRadius ?? Props.detectRadius;

        public bool AutoReloadEnabled => autoReloadEnabled;

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
        }

        public void TickEquipped(Pawn wearer)
        {
            if (!autoDeployEnabled)
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
            if (HasActiveDroneNearby(wearer.Position, wearer.Map, radius))
                return;

            Thing triggerFire = FindFireNearby(wearer.Position, wearer.Map, radius);
            if (triggerFire != null)
            {
                DeployDroneAuto(wearer, triggerFire);
            }
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var gizmo in base.CompGetWornGizmosExtra())
            {
                yield return gizmo;
            }

            var apparel = parent as Apparel;
            var wearer = apparel?.Wearer;
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
        }

        private bool HasActiveDroneNearby(IntVec3 center, Map map, float radius)
        {
            if (Props.droneKind == null)
                return false;

            int numCells = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = center + GenRadial.RadialPattern[i];
                if (!cell.InBounds(map))
                    continue;

                var thingList = cell.GetThingList(map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    if (thingList[j] is Pawn p && !p.Dead && p.kindDef == Props.droneKind)
                        return true;
                }
            }
            return false;
        }

        private Thing FindFireNearby(IntVec3 center, Map map, float radius)
        {
            int numCells = GenRadial.NumCellsInRadius(radius);
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 cell = center + GenRadial.RadialPattern[i];
                if (!cell.InBounds(map))
                    continue;

                var thingList = cell.GetThingList(map);
                for (int j = 0; j < thingList.Count; j++)
                {
                    Thing t = thingList[j];
                    if (t is Fire || t.IsBurning())
                        return t;
                }
            }
            return null;
        }

        private void DeployDroneAuto(Pawn wearer, Thing triggerFire)
        {
            if (Props.droneKind == null || ChargedComp == null || ChargedComp.RemainingCharges <= 0)
                return;

            ChargedComp.UsedOnce();
            lastAutoDeployTick = Find.TickManager.TicksGame;

            IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(
                wearer.Position,
                wearer.Map,
                ExtinguisherDronePackSettings.SpawnSearchRadius,
                c => c.Standable(wearer.Map)
            );
            if (!spawnCell.IsValid)
                spawnCell = wearer.Position;

            Pawn drone = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                Props.droneKind,
                wearer.Faction,
                PawnGenerationContext.NonPlayer,
                forceGenerateNewPawn: true,
                allowDead: false,
                allowDowned: false,
                canGeneratePawnRelations: false,
                fixedBiologicalAge: 0f,
                fixedChronologicalAge: 0f
            ));

            GenSpawn.Spawn(drone, spawnCell, wearer.Map);
            ExtinguisherDroneInterop.ConfigureDroneForBackpack(drone, triggerFire);
            EffecterDefOf.Skip_EntryNoDelay?.Spawn(spawnCell, wearer.Map)?.Cleanup();
        }
    }
}
