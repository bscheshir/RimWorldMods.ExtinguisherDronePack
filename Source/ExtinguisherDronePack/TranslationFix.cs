using HarmonyLib;
using RimWorld;
using Verse;

namespace ExtinguisherDronePack
{
    public static class TranslationFix
    {
        private const string TipChargesKey = "ExtinguisherPack.TipCharges";
        private static CompProperties_ApparelReloadable cachedPackProps;

        private static CompProperties_ApparelReloadable PackProps
        {
            get
            {
                if (cachedPackProps == null)
                {
                    var packDef = DefDatabase<ThingDef>.GetNamedSilentFail("Apparel_PackExtinguisher");
                    cachedPackProps = packDef?.GetCompProperties<CompProperties_ApparelReloadable>();
                }
                return cachedPackProps;
            }
        }

        [HarmonyPatch(typeof(CompApparelVerbOwner_Charged), nameof(CompApparelVerbOwner_Charged.CompTipStringExtra))]
        public static class Patch_CompApparelVerbOwner_Charged_CompTipStringExtra
        {
            public static bool Prefix(CompApparelVerbOwner_Charged __instance, ref string __result)
            {
                if (__instance.Props == PackProps)
                {
                    __result = "\n\n" + TipChargesKey.Translate(__instance.RemainingCharges, __instance.MaxCharges);
                    return false;
                }
                return true;
            }
        }
    }
}
