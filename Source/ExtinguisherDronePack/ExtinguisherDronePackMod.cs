using HarmonyLib;
using RimWorld;
using System.Reflection;
using UnityEngine;
using Verse;

namespace ExtinguisherDronePack
{
    public partial class ExtinguisherDronePackMod : Mod
    {
        public static ExtinguisherDronePackSettings Settings { get; private set; }

        public ExtinguisherDronePackMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<ExtinguisherDronePackSettings>();
        }
    }

    [StaticConstructorOnStartup]
    public static class HarmonyInitializer
    {
        static HarmonyInitializer()
        {
            var harmony = new Harmony("com.BSCheshir.ExtinguisherDronePack");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [StaticConstructorOnStartup]
    public static class ExtinguisherDronePackAssets
    {
        public static readonly Texture2D IconAutoDeploy = ContentFinder<Texture2D>.Get("Things/Pawn/Drone/ExtinguisherDrone/ExtinguisherDrone_east", true);
        public static readonly Texture2D IconAutoReload = ContentFinder<Texture2D>.Get("UI/AutoChargePack", true);
        public static readonly Texture2D ProgressFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.28f, 0.68f, 0.35f, 0.45f));

        public const string PackTexPathEmpty = "HunterDronePack/Apparel_PackHunter_Extinguisher_Empty";

        public static readonly Graphic EmptyPackWorldGraphic = ContentFinder<Texture2D>.Get(PackTexPathEmpty, false) != null
            ? GraphicDatabase.Get<Graphic_Single>(PackTexPathEmpty, ShaderDatabase.Cutout, new Vector2(0.7f, 0.7f), Color.white)
            : null;
    }
}
