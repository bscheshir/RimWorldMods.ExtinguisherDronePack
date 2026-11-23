using System.Reflection;
using HarmonyLib;
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
    }

    public class ExtinguisherDronePackSettings : ModSettings
    {
        public const float DefaultDetectRadius = 12.9f;
        public const float MinDetectRadius = 3.9f;
        public const float MaxDetectRadius = 25.9f;
        public const int DefaultCheckInterval = 60;
        public const int SpawnSearchRadius = 2;
        public const int AutoDeployCooldownTicks = 180;
        public const bool DefaultAutoReload = true;

        public float detectRadius = DefaultDetectRadius;
        public bool defaultAutoReload = DefaultAutoReload;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref detectRadius, nameof(detectRadius), DefaultDetectRadius);
            Scribe_Values.Look(ref defaultAutoReload, nameof(defaultAutoReload), DefaultAutoReload);
        }
    }
}
