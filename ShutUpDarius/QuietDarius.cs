using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;
using BattleTech;
using HBS.Collections;
using Newtonsoft.Json;

namespace ShutUpDarius
{
    public static partial class Core
    {
        public const string ModName = "QuietDarius";
        public const string ModId   = "Battletech.realitymachina.ShutUpDarius";

        internal static Settings ModSettings = new Settings();
        internal static string ModDirectory;

        public static void Init(string directory, string settingsJson)
        {
            ModDirectory = directory;
            try
            {
                ModSettings = JsonConvert.DeserializeObject<Settings>(settingsJson);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ModSettings = new Settings();
            }

            HarmonyInstance.DEBUG = ModSettings.debug;
            var harmony = HarmonyInstance.Create(ModId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void OnNotificationDismissed()
        {
        }
    }

    [HarmonyPatch(typeof(SimGameState), "ShowMechWarriorTrainingNotif")]
    public static class BattleTech_Pilot_ShowMechWarriorTrainingNotif_Patch
    {
        private static readonly TagSet PilotFatigueTags = new TagSet("pilot_lightinjury", "pilot_fatigued");

        static bool Prefix(SimGameState __instance)
        {
            //no easy way to avoid incompatibilites here: we're completely overriding the logic so we can do what we need to do
            return false;
        }

        static void Postfix(SimGameState __instance)
        {
            if (!Core.ModSettings.WarnAboutTraining) return;

            var pilotsToTrain = 0;
            var pilots = new List<Pilot>(__instance.PilotRoster) {__instance.Commander};
            foreach (var pilot in pilots)
            {
                if (pilot.pilotDef.PilotTags.ContainsAny(PilotFatigueTags)) continue;
                var foundTrainable = false;
                var source = new int[] {pilot.Gunnery, pilot.Tactics, pilot.Guts, pilot.Piloting};
                for (var i = 0; i < source.Length && !foundTrainable; i++)
                {
                    if (source[i] >= Core.ModSettings.PilotSkillLevelForWhichWarningNotNecessary) continue;
                    if (pilot.UnspentXP <= __instance.GetLevelCost(source[i] + 1)) continue;
                    pilotsToTrain++;
                    foundTrainable = true;
                }

                if (foundTrainable) pilotsToTrain++;
            }

            if (pilotsToTrain >= Core.ModSettings.MinimumPilotsNeedingTrainingForWarning)
            {
                __instance
                    .GetInterruptQueue()
                    .QueuePauseNotification(
                        "MechWarrior Training Required",
                        $"Our MechWarriors are gaining in experience and need your guidance, {__instance.Commander.Callsign}. If you head to the Barracks, you can direct their training.",
                        __instance.GetCrewPortrait(SimGameCrew.Crew_Darius),
                        null,
                        new Action(Core.OnNotificationDismissed),
                        "Continue",
                        null,
                        null);
            }
        }
    }
}