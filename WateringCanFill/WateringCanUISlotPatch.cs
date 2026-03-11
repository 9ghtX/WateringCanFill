using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WateringCanFill;

public static class WateringCanUiPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(
        typeof(ItemSlot),
        nameof(ItemSlot.TryPutInto),
        new[] { typeof(IWorldAccessor), typeof(ItemSlot), typeof(int) }
    )]
    static bool Prefix(
        ItemSlot __instance,
        IWorldAccessor world,
        ItemSlot sinkSlot,
        int quantity,
        ref int __result)
    {
        var canStack = __instance?.Itemstack;
        if (canStack == null)
            return true;

        if (canStack.Collectible is not BlockWateringCan can)
            return true;

        DebugChat.Msg(world, "UI patch entered");

        if (sinkSlot?.Itemstack == null)
        {
            DebugChat.Msg(world, "UI sink slot empty");
            return true;
        }

        if (!WateringLogic.TryTransferWater(
                can,
                __instance,
                sinkSlot,
                world))
        {
            DebugChat.Msg(world, "Transfer from UI slot did not happen");
            return true;
        }

        __result = 0;

        DebugChat.Msg(world, "Handled by mod UI patch");

        return false;
    }
}
