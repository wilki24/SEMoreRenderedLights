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

            foreach (var instruction in instructions)
            {
                // Plugin.Instance.Log.Debug($"opcode: {instruction.opcode.ToString()} - operand: {(instruction.operand != null ? instruction.operand.ToString() : "")}");

                if (instruction.opcode == OpCodes.Ldc_I4_S && instruction.operand is int i && i == 32)
                {
                    Plugin.Instance.Log.Debug($"Max number of spotlights now set to : {maxSpotlights.ToString()}");

                    if (maxSpotlights > sbyte.MaxValue)
                    {
                        // Can't use Ldc_I4_S for integers bigger than a byte.
                        yield return new CodeInstruction(OpCodes.Ldc_I4, maxSpotlights);
                    }
                    else
                    {
                        // We could just  use the new CodeInstruction above, but this retains
                        // the original compiler micro-optimization. I'm doing this for instructive
                        // purposes for the future me or anyone else who stumbles across this.
                        instruction.operand = (sbyte)maxSpotlights;
                    }
                }

                yield return instruction;
            }
        }

        [HarmonyTargetMethod]
        public static MethodBase Target()
        {
            // Class is internal, so we can't reference it here directly, instead we'll use reflection.
            return AccessTools.Method("VRage.Render11.LightingStage.MyLightsRendering:RenderSpotlights");
        }

    }
}
