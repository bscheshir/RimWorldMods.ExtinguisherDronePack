using RimWorld;
using RimWorld.Utility;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ExtinguisherDronePack
{
    public static class ExtinguisherPackReloader
    {
        public static List<Thing> PrepareAmmoWithInventoryPriority(Pawn wearer, CompApparelReloadable reloadable, List<Thing> mapAmmo)
        {
            if (wearer == null || reloadable == null || reloadable.AmmoDef == null)
                return mapAmmo;

            int needed = reloadable.MaxAmmoNeeded(true);
            if (needed <= 0)
                return null;

            List<Thing> result = new List<Thing>();
            int collected = 0;

            var inv = wearer.inventory?.innerContainer;
            if (inv != null && wearer.Spawned && wearer.Map != null)
            {
                for (int i = inv.Count - 1; i >= 0 && collected < needed; i--)
                {
                    Thing item = inv[i];
                    if (item.def == reloadable.AmmoDef && item.stackCount > 0)
                    {
                        int toDrop = Mathf.Min(item.stackCount, needed - collected);
                        if (inv.TryDrop(item, wearer.Position, wearer.Map, ThingPlaceMode.Direct, toDrop, out Thing dropped))
                        {
                            dropped.SetForbidden(false, false);
                            result.Add(dropped);
                            collected += dropped.stackCount;
                        }
                    }
                }
            }

            if (collected < needed && !mapAmmo.NullOrEmpty())
            {
                for (int i = 0; i < mapAmmo.Count && collected < needed; i++)
                {
                    Thing item = mapAmmo[i];
                    if (item != null && item.Spawned && !result.Contains(item))
                    {
                        result.Add(item);
                        collected += item.stackCount;
                    }
                }
            }

            return collected >= reloadable.MinAmmoNeeded(true) ? result : null;
        }

        public static void OrderManualReload(Pawn wearer, CompApparelReloadable reloadable, bool silent = false, bool inventoryOnly = false)
        {
            if (wearer == null || reloadable == null)
                return;

            List<Thing> mapAmmo = (!inventoryOnly && wearer.Map != null)
                ? ReloadableUtility.FindEnoughAmmo(wearer, wearer.Position, reloadable, false)
                : null;

            List<Thing> ammo = PrepareAmmoWithInventoryPriority(wearer, reloadable, mapAmmo);
            if (ammo.NullOrEmpty())
            {
                if (!silent)
                {
                    Messages.Message(
                        "ExtinguisherPack.NoAmmo".Translate(reloadable.Props.ammoDef.label),
                        wearer,
                        MessageTypeDefOf.RejectInput,
                        historical: false
                    );
                }
                return;
            }

            Job job = JobMaker.MakeJob(JobDefOf.Reload, reloadable.parent);
            job.targetQueueB = new List<LocalTargetInfo>(ammo.Count);
            for (int i = 0; i < ammo.Count; i++)
            {
                job.targetQueueB.Add(ammo[i]);
            }
            job.count = reloadable.MaxAmmoNeeded(true);
            wearer.jobs.TryTakeOrderedJob(job, JobTag.Misc);
        }
    }
}
