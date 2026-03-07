using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace WateringCanFill;

[HarmonyPatch(typeof(BlockWateringCan), "OnHeldInteractStart")]
public static class WateringCanPatch
{
    const float LitresForFullCan = 5f;

    static bool Prefix(
        BlockWateringCan __instance,
        ItemSlot slot,
        EntityAgent byEntity,
        BlockSelection blockSel,
        EntitySelection entitySel,
        bool firstEvent,
        ref EnumHandHandling handHandling
    )
    {
        if (blockSel == null || byEntity.Controls.ShiftKey)
            return true;

        var world = byEntity.World;
        var pos = blockSel.Position;

        var block = world.BlockAccessor.GetBlock(pos);
        var be = world.BlockAccessor.GetBlockEntity(pos);

        if (be == null)
            return true;

        var invProvider = be as IBlockEntityContainer;
        if (invProvider == null)
            return true;

        var inv = invProvider.Inventory;
        if (inv == null || inv.Count == 0)
            return true;

        var slot0 = inv[0];
        var stack = slot0?.Itemstack;

        if (stack == null)
            return true;

        if (stack.Block == null || stack.Block.LiquidCode != "water")
            return true;

        var props = stack.Block.Attributes?["waterTightContainerProps"]
            ?.AsObject<WaterTightContainableProps>();

        if (props == null)
            return true;

        float litresPerItem = 1f / props.ItemsPerLitre;
        float availableLitres = stack.StackSize * litresPerItem;

        float secondsPerLitre = __instance.CapacitySeconds / LitresForFullCan;

        float remainingSeconds = __instance.GetRemainingWateringSeconds(slot.Itemstack);
        float missingSeconds = __instance.CapacitySeconds - remainingSeconds;

        if (missingSeconds <= 0)
            return false;

        float neededLitres = missingSeconds / secondsPerLitre;
        float litresToTransfer = Math.Min(neededLitres, availableLitres);

        float secondsToAdd = litresToTransfer * secondsPerLitre;

        __instance.SetRemainingWateringSeconds(
            slot.Itemstack,
            remainingSeconds + secondsToAdd
        );

        int itemsToTake = (int)Math.Ceiling(litresToTransfer / litresPerItem);

        stack.StackSize -= itemsToTake;

        if (stack.StackSize <= 0)
            slot0.Itemstack = null;

        slot0.MarkDirty();
        slot.MarkDirty();

        handHandling = EnumHandHandling.PreventDefault;
        return false;
    }
}