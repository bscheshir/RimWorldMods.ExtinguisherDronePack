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

    public class ExtinguisherDronePackSettings : ModSettings
    {
        public const float DefaultDetectRadius = 12.9f;
        public const float MinDetectRadius = 3.9f;
        public const float MaxDetectRadius = 25.9f;
        public const int DefaultCheckInterval = 60;
        public const int SpawnSearchRadius = 2;
        public const int AutoDeployCooldownTicks = 180;
        public const int DefaultFabricationTicks = 1800; // 30 секунд (60 тиков * 30)
        public const int MinFabricationTicks = 300;      // 5 секунд
        public const int MaxFabricationTicks = 7200;     // 120 секунд
        public const bool DefaultAutoReload = true;

        public float detectRadius = DefaultDetectRadius;
        public int fabricationTicks = DefaultFabricationTicks;
        public bool defaultAutoReload = DefaultAutoReload;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref detectRadius, nameof(detectRadius), DefaultDetectRadius);
            Scribe_Values.Look(ref fabricationTicks, nameof(fabricationTicks), DefaultFabricationTicks);
            Scribe_Values.Look(ref defaultAutoReload, nameof(defaultAutoReload), DefaultAutoReload);
        }
    }
}
