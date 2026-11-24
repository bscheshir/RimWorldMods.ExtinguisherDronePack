using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

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
        public static void Postfix(CompApparelReloadable __instance, bool allowForcedReload, ref bool __result)
        {
            if (!__result)
                return;

            var autoComp = __instance.parent?.GetComp<CompAutoExtinguisherPack>();
            if (autoComp == null)
                return;

            if (autoComp.IsFabricating)
            {
                __result = false;
                return;
            }

            if (!allowForcedReload && !autoComp.AutoReloadEnabled)
            {
                __result = false;
            }
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
