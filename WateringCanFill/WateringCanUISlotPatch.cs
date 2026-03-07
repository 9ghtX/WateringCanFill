using System;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WateringCanFill;

[HarmonyPatch(typeof(ItemSlot), "TryPutInto")]
public static class WateringCanUISlotPatch
{
    const float LitresForFullCan = 5f;

    static bool Prefix(
        ItemSlot __instance,
        ItemSlot sinkSlot,
        ref int movedQuantity)
    {
        if (__instance?.Itemstack == null)
            return true;

        var canStack = __instance.Itemstack;

        if (canStack.Block is not BlockWateringCan wateringCan)
            return true;

        var sourceStack = sinkSlot?.Itemstack;

        if (sourceStack?.Block?.LiquidCode != "water")
            return true;

        var props = sourceStack.Block.Attributes?["waterTightContainerProps"]
            ?.AsObject<WaterTightContainableProps>();

        if (props == null)
            return true;

        float litresPerItem = 1f / props.ItemsPerLitre;
        float availableLitres = sourceStack.StackSize * litresPerItem;

        float secondsPerLitre = wateringCan.CapacitySeconds / LitresForFullCan;

        float remainingSeconds = wateringCan.GetRemainingWateringSeconds(canStack);
        float missingSeconds = wateringCan.CapacitySeconds - remainingSeconds;

        if (missingSeconds <= 0)
            return false;

        float neededLitres = missingSeconds / secondsPerLitre;
        float litresToTransfer = Math.Min(neededLitres, availableLitres);

        float secondsToAdd = litresToTransfer * secondsPerLitre;

        wateringCan.SetRemainingWateringSeconds(
            canStack,
            remainingSeconds + secondsToAdd
        );

        int itemsToTake = (int)Math.Ceiling(litresToTransfer / litresPerItem);

        sourceStack.StackSize -= itemsToTake;

        if (sourceStack.StackSize <= 0)
            sinkSlot.Itemstack = null;

        sinkSlot.MarkDirty();
        __instance.MarkDirty();

        movedQuantity = 0;

        return false;
    }
}