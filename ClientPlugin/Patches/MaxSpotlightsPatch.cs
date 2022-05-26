using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ClientPlugin.Patches
{
    [HarmonyPatch]
    class MaxSpotlightsPatch
    {
        [HarmonyPrefix]
        private static void Prefix()
        {
            // This method returns immediately if the number of spotlights is 32 or less. So, we'll
            // just always return to allow for infinite spotlights. TODO: Make this adjustable and 
            // use a transpiler
            return;
        }

        [HarmonyTargetMethod]
        public static MethodBase Target()
        {
            return AccessTools.Method("VRage.Render11.LightingStage.MyLightsRendering:CullSpotLights");
        }
    }

    [HarmonyPatch]
    class RenderSpotlightsPatch
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                yield return instruction;
            }
        }

        [HarmonyTargetMethod]
        public static MethodBase Target()
        {
            return AccessTools.Method("VRage.Render11.LightingStage.MyLightsRendering:RenderSpotlights");
        }

    }
}
