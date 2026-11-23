using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ExtinguisherDronePack
{
    internal static class ExtinguisherDroneInterop
    {
        private static readonly Type CompType = AccessTools.TypeByName("ExtinguisherDrone.CompFireExtinguisherDrone");
        private static readonly MethodInfo SetHomeAreaOnlyMethod = CompType != null ? AccessTools.Method(CompType, "SetHomeAreaOnly") : null;
        private static readonly MethodInfo RememberTriggerFireMethod = CompType != null ? AccessTools.Method(CompType, "RememberTriggerFire") : null;

        public static void ConfigureDroneForBackpack(Pawn drone, Thing triggerFire)
        {
            if (drone == null || CompType == null)
                return;

            ThingComp targetComp = null;
            var comps = drone.AllComps;
            for (int i = 0; i < comps.Count; i++)
            {
                if (CompType.IsInstanceOfType(comps[i]))
                {
                    targetComp = comps[i];
                    break;
                }
            }

            if (targetComp == null)
                return;

            SetHomeAreaOnlyMethod?.Invoke(targetComp, new object[] { false });
            if (triggerFire != null)
            {
                RememberTriggerFireMethod?.Invoke(targetComp, new object[] { triggerFire });
            }
        }
    }
}
