using RimWorld;
using RimWorld.Utility;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace ExtinguisherDronePack
{
    internal static class FireDroneDeployer
    {
        public static bool HasActiveDroneNearby(IntVec3 center, Map map, float radius, PawnKindDef droneKind)
        {
            if (droneKind == null)
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
                    if (thingList[j] is Pawn p && !p.Dead && p.kindDef == droneKind)
                        return true;
                }
            }
            return false;
        }

        public static Thing FindFireNearby(IntVec3 center, Map map, float radius)
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

        public static void SpawnNearWearer(Pawn wearer, PawnKindDef droneKind, Thing triggerFire)
        {
            if (droneKind == null)
                return;

            IntVec3 spawnCell = CellFinder.RandomClosewalkCellNear(
                wearer.Position,
                wearer.Map,
                ExtinguisherDronePackSettings.SpawnSearchRadius,
                c => c.Standable(wearer.Map)
            );
            if (!spawnCell.IsValid)
                spawnCell = wearer.Position;

            Pawn drone = PawnGenerator.GeneratePawn(new PawnGenerationRequest(
                droneKind,
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
