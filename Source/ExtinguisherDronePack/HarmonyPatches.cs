using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace ExtinguisherDronePack
{
    [HarmonyPatch(typeof(Pawn), "Tick")]
    public static class Patch_Pawn_Tick
    {
        public static void Postfix(Pawn __instance)
        {
            var worn = __instance.apparel?.WornApparel;
            if (worn == null)
                return;

            for (int i = 0; i < worn.Count; i++)
            {
                var autoComp = worn[i].GetComp<CompAutoExtinguisherPack>();
                if (autoComp != null)
                {
                    autoComp.TickEquipped(__instance);
                }
            }
        }
    }

    [HarmonyPatch(typeof(Command), nameof(Command.GizmoOnGUI))]
    public static class Patch_Command_GizmoOnGUI
    {
        public static void Prefix(Command __instance)
        {
            if (__instance is Command_VerbTarget verbCmd && verbCmd.verb is Verb_LaunchProjectileExtinguisherPack)
            {
                verbCmd.defaultLabel = "ExtinguisherPack.ManualDeploy".Translate();
                verbCmd.defaultDesc = "ExtinguisherPack.ManualDeploy.Desc".Translate();
            }
        }

        public static void Postfix(Command __instance, Vector2 topLeft, float maxWidth)
        {
            if (__instance is Command_VerbTarget verbCmd && verbCmd.verb is Verb_LaunchProjectileExtinguisherPack packVerb)
            {
                var autoComp = packVerb.FindAutoComp();
                if (autoComp != null && autoComp.IsFabricating)
                {
                    float width = __instance.GetWidth(maxWidth);
                    Rect rect = new Rect(topLeft.x, topLeft.y, width, Gizmo.Height);

                    float fillWidth = rect.width * autoComp.FabricationProgress;
                    Rect fillRect = new Rect(rect.x, rect.y, fillWidth, rect.height);
                    GUI.DrawTexture(fillRect, ExtinguisherDronePackAssets.ProgressFillTex);

                    Text.Anchor = TextAnchor.MiddleCenter;
                    Text.Font = GameFont.Small;
                    Widgets.Label(rect, autoComp.FabricationSecondsLeft.ToString("0.0") + " " + "LetterSecond".Translate());
                    Text.Anchor = TextAnchor.UpperLeft;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CompApparelReloadable), nameof(CompApparelReloadable.CanBeUsed))]
    public static class Patch_CompApparelReloadable_CanBeUsed
    {
        public static void Postfix(CompApparelReloadable __instance, ref string reason, ref bool __result)
        {
            if (!__result)
                return;

            var autoComp = __instance.parent?.GetComp<CompAutoExtinguisherPack>();
            if (autoComp != null && autoComp.IsFabricating)
            {
                __result = false;
                reason = "ExtinguisherPack.FabricatingDisabled".Translate(autoComp.FabricationSecondsLeft.ToString("0.0"));
            }
        }
    }

    [HarmonyPatch(typeof(CompApparelReloadable), nameof(CompApparelReloadable.NeedsReload))]
    public static class Patch_CompApparelReloadable_NeedsReload
    {
        public static void Postfix(CompApparelReloadable __instance, ref bool __result)
        {
            if (!__result)
                return;

            var autoComp = __instance.parent?.GetComp<CompAutoExtinguisherPack>();
            if (autoComp != null && autoComp.IsFabricating)
            {
                __result = false;
            }
        }
    }

    [HarmonyPatch(typeof(JobGiver_Reload), "TryGiveJob")]
    public static class Patch_JobGiver_Reload_TryGiveJob
    {
        public static bool Prefix(Pawn pawn, ref Job __result)
        {
            var worn = pawn?.apparel?.WornApparel;
            if (worn == null)
                return true;

            for (int i = 0; i < worn.Count; i++)
            {
                var autoComp = worn[i].GetComp<CompAutoExtinguisherPack>();
                if (autoComp != null && !autoComp.AutoReloadEnabled)
                {
                    var reloadable = worn[i].GetComp<CompApparelReloadable>();
                    if (reloadable != null && reloadable.NeedsReload(false) && !HasOtherReloadable(pawn, worn[i]))
                    {
                        __result = null;
                        return false;
                    }
                }
            }

            return true;
        }

        public static void Postfix(Pawn pawn, Job __result)
        {
            if (__result == null || __result.def != JobDefOf.Reload)
                return;

            var packThing = __result.targetA.Thing;
            if (packThing == null || packThing.TryGetComp<CompAutoExtinguisherPack>() == null)
                return;

            var reloadable = packThing.TryGetComp<CompApparelReloadable>();
            if (reloadable == null || pawn?.inventory?.innerContainer == null)
                return;

            List<Thing> currentQueue = new List<Thing>();
            if (__result.targetQueueB != null)
            {
                for (int i = 0; i < __result.targetQueueB.Count; i++)
                {
                    if (__result.targetQueueB[i].Thing != null)
                    {
                        currentQueue.Add(__result.targetQueueB[i].Thing);
                    }
                }
            }

            List<Thing> prioritized = ExtinguisherPackReloader.PrepareAmmoWithInventoryPriority(pawn, reloadable, currentQueue);
            if (!prioritized.NullOrEmpty())
            {
                __result.targetQueueB = new List<LocalTargetInfo>(prioritized.Count);
                for (int i = 0; i < prioritized.Count; i++)
                {
                    __result.targetQueueB.Add(prioritized[i]);
                }
            }
        }

        private static bool HasOtherReloadable(Pawn pawn, Apparel excluded)
        {
            var worn = pawn.apparel.WornApparel;
            for (int i = 0; i < worn.Count; i++)
            {
                if (worn[i] == excluded)
                    continue;

                var comp = worn[i].GetComp<CompApparelReloadable>();
                if (comp != null && comp.NeedsReload(false))
                    return true;
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(CompApparelReloadable), nameof(CompApparelReloadable.ReloadFrom))]
    public static class Patch_CompApparelReloadable_ReloadFrom
    {
        public static void Postfix(CompApparelReloadable __instance)
        {
            var autoComp = __instance.parent?.GetComp<CompAutoExtinguisherPack>();
            if (autoComp != null && __instance.RemainingCharges > 0)
            {
                autoComp.StartFabrication();
            }
        }
    }

    [HarmonyPatch(typeof(Thing), nameof(Thing.Graphic), MethodType.Getter)]
    public static class Patch_Thing_Graphic
    {
        public static void Postfix(Thing __instance, ref Graphic __result)
        {
            if (__instance is ThingWithComps twc)
            {
                var autoComp = twc.GetComp<CompAutoExtinguisherPack>();
                if (autoComp != null && autoComp.IsVisuallyEmpty)
                {
                    __result = autoComp.EmptyWorldGraphic;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ApparelGraphicRecordGetter), nameof(ApparelGraphicRecordGetter.TryGetGraphicApparel))]
    public static class Patch_ApparelGraphicRecordGetter_TryGetGraphicApparel
    {
        public static void Prefix(Apparel apparel, ref string __state)
        {
            var autoComp = apparel?.GetComp<CompAutoExtinguisherPack>();
            if (autoComp != null && autoComp.IsVisuallyEmpty && ExtinguisherDronePackAssets.EmptyPackWorldGraphic != null)
            {
                __state = apparel.def.apparel.wornGraphicPath;
                apparel.def.apparel.wornGraphicPath = ExtinguisherDronePackAssets.PackTexPathEmpty;
            }
        }

        public static void Finalizer(Apparel apparel, string __state)
        {
            if (__state != null && apparel?.def?.apparel != null)
            {
                apparel.def.apparel.wornGraphicPath = __state;
            }
        }
    }
}
