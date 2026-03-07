using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WateringCanFill;

[HarmonyPatch(typeof(BlockWateringCan), "OnHeldInteractStart")]
public static class WateringCanBlockPatch
{
    static bool Prefix(
        BlockWateringCan __instance,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection? blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handHandling)
    {
        if (blockSel == null)
            return true;

        var world = byEntity.World;

        DebugChat.Msg(world, "Block patch entered");

        var be = world.BlockAccessor.GetBlockEntity(blockSel.Position);

        if (be is not IBlockEntityContainer container)
            return true;

        var inv = container.Inventory;

        if (inv.Count == 0)
            return true;

        foreach (var s in inv)
        {
            if (!WateringLogic.TryTransferWater(
                    __instance,
                    slot,
                    s,
                    world,
                    blockSel.Position))
                return true;
        }

        slot.Itemstack.TempAttributes.SetInt("refilled", 1);

        handHandling = EnumHandHandling.PreventDefault;

        DebugChat.Msg(world, "Handled by mod block patch");

        return false;
    }
}