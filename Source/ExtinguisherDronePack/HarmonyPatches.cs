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
            if (!__result || allowForcedReload)
                return;

            var autoComp = __instance.parent?.GetComp<CompAutoExtinguisherPack>();
            if (autoComp != null && !autoComp.AutoReloadEnabled)
            {
                __result = false;
            }
        }
    }
}
