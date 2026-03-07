using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace WateringCanFill;

[HarmonyPatch(typeof(Block), "OnBlockInteractStart")]
public static class LiquidContainerPatch
{
    static bool Prefix(
        Block __instance,
        IWorldAccessor world,
        IPlayer? byPlayer,
        BlockSelection? blockSel)
    {
        if (blockSel == null || byPlayer == null)
            return true;

        var slot = byPlayer.InventoryManager.ActiveHotbarSlot;
        var stack = slot?.Itemstack;

        if (stack?.Block is not BlockWateringCan can)
            return true;

        var be = world.BlockAccessor.GetBlockEntity(blockSel.Position);

        if (be is not BlockEntityLiquidContainer container)
            return true;

        DebugChat.Msg(world, "Liquid container detected");

        if (container.Inventory.Count < 1)
            return true;

        ItemSlot liquidSlot = null;

        for (int i = 0; i < container.Inventory.Count; i++)
        {
            if (!container.Inventory[i].Empty)
            {
                liquidSlot = container.Inventory[i];
                break;
            }
        }

        if (liquidSlot == null)
        {
            DebugChat.Msg(world, "No liquid slot found");
            return true;
        }

        DebugChat.Msg(world, $"Liquid stack: {liquidSlot.Itemstack}");

        if (!WateringLogic.TryTransferWater(
                can,
                slot,
                liquidSlot,
                world,
                blockSel.Position,
                byPlayer))
        {
            DebugChat.Msg(world, "Transfer failed");
            return true;
        }

        DebugChat.Msg(world, "Transfer handled by LiquidContainerPatch");

        byPlayer.Entity.Controls.HandUse = EnumHandInteract.None;

        return false;
    }
}