using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            // Class is internal, so we can't reference it here directly, instead we'll use reflection.
            return AccessTools.Method("VRage.Render11.LightingStage.MyLightsRendering:CullSpotLights");
        }
    }

    [HarmonyPatch]
    class RenderSpotlightsPatch
    {
        /* The max allowed spotlights value is hardcoded, so all we need to do here is find
         * the instruction that's pushing 32 onto the stack
        IL_022d: ldloc.2      // num
        IL_022e: ldc.i4.s     32 // 0x20
        IL_0230: blt.s        IL_0234
        */

        const int maxSpotlights = 64; // TODO: Replace this with config value

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Instance.Log.Debug($"entering max spotlights transpiler");

            CodeMatcher matcher = new(instructions);
            matcher.MatchStartForward(new CodeMatch(OpCodes.Ldc_I4_S, 32))
                .ThrowIfInvalid("SeMoreRenderedLights could not find the right code sequence.");

            // Can't use Ldc_I4_S for integers bigger than a byte.
            // We could always use Ldc_I4_S, but this retains the original compiler
            // micro-optimization. I'm doing this for instructive purposes for
            // the future me or anyone else who stumbles across this.
            var oc = maxSpotlights <= sbyte.MaxValue ? OpCodes.Ldc_I4_S : OpCodes.Ldc_I4;

            matcher.Set(oc, maxSpotlights);
  
            return matcher.Instructions();
        }

        [HarmonyTargetMethod]
        public static MethodBase Target()
        {
            // Class is internal, so we can't reference it here directly, instead we'll use reflection.
            return AccessTools.Method("VRage.Render11.LightingStage.MyLightsRendering:RenderSpotlights");
        }

    }
}
