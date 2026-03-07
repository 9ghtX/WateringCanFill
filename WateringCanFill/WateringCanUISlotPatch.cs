using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WateringCanFill;

[HarmonyPatch(typeof(ItemSlot), "TryPutInto")]
public static class WateringCanUiPatch
{
    static bool Prefix(
        ItemSlot __instance,
        ItemSlot sinkSlot,
        ref int movedQuantity)
    {
        var canStack = __instance?.Itemstack;
        if (canStack == null)
            return true;

        if (canStack.Block is not BlockWateringCan can)
            return true;

        var api = __instance.Inventory?.Api;
        var world = api?.World;

        if (world == null)
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

        movedQuantity = 0;

        DebugChat.Msg(world, "Handled by mod UI patch");

        return false;
    }
}