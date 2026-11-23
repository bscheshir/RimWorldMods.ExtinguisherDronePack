using HarmonyLib;
using RimWorld;
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
            if (autoComp != null && autoComp.IsVisuallyEmpty)
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
