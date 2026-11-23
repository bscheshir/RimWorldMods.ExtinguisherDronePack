using RimWorld;
using Verse;

namespace ExtinguisherDronePack
{
    public class Verb_LaunchProjectileExtinguisherPack : Verb_LaunchProjectileStatic
    {
        private CompApparelReloadable FindPackComp()
        {
            if (DirectOwner is CompApparelReloadable direct)
                return direct;

            if (ReloadableCompSource != null)
                return ReloadableCompSource;

            if (EquipmentSource != null)
            {
                var comp = EquipmentSource.GetComp<CompApparelReloadable>();
                if (comp != null)
                    return comp;
            }

            var worn = CasterPawn?.apparel?.WornApparel;
            if (worn != null)
            {
                for (int i = 0; i < worn.Count; i++)
                {
                    if (worn[i].GetComp<CompAutoExtinguisherPack>() != null)
                        return worn[i].GetComp<CompApparelReloadable>();
                }
            }

            return null;
        }

        protected override bool TryCastShot()
        {
            var reloadable = FindPackComp();
            if (reloadable != null && !reloadable.CanBeUsed(out _))
                return false;

            bool result = base.TryCastShot();
            if (result && reloadable != null)
            {
                reloadable.UsedOnce();
            }
            return result;
        }
    }

    public class Projectile_ExtinguisherPack : Projectile_SpawnsPawnZeroAge
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            Map map = Map;
            IntVec3 pos = Position;
            PawnKindDef kind = def.projectile.spawnsPawnKind;
            Faction faction = Launcher?.Faction ?? Faction.OfPlayer;

            base.Impact(hitThing, blockedByShield);

            if (map == null || kind == null)
                return;

            int numCells = GenRadial.NumCellsInRadius(ExtinguisherDronePackSettings.SpawnSearchRadius);
            for (int i = 0; i < numCells; i++)
            {
                IntVec3 c = pos + GenRadial.RadialPattern[i];
                if (!c.InBounds(map))
                    continue;

                var list = c.GetThingList(map);
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j] is Pawn p && p.kindDef == kind && p.Faction == faction && p.ageTracker.AgeBiologicalTicks <= 5)
                    {
                        ExtinguisherDroneInterop.ConfigureDroneForBackpack(p, null);
                    }
                }
            }
        }
    }
}
