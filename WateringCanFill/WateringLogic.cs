using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace WateringCanFill;

public static class WateringLogic
{
    public const float LitresForFullCan = 5f;
    public const float LitresPerTransfer = 1f;

    public static bool TryTransferWater(
        BlockWateringCan can,
        ItemSlot canSlot,
        ItemSlot sourceSlot,
        IWorldAccessor world,
        BlockPos? soundPos = null,
        IPlayer? byPlayer = null)
    {
        DebugChat.Msg(world, "Transfer started");

        var sourceStack = sourceSlot?.Itemstack;
        if (sourceStack == null)
        {
            DebugChat.Msg(world, "Source slot empty");
            return false;
        }

        DebugChat.Msg(world, $"Source stack: {sourceStack}");

        var props = sourceStack.Collectible.Attributes?["waterTightContainerProps"]
            ?.AsObject<WaterTightContainableProps>();

        if (props == null)
        {
            DebugChat.Msg(world, "Item has no liquid container props");
            return false;
        }

        var code = sourceStack.Collectible?.Code?.Path;
        if (code != "waterportion")
        {
            DebugChat.Msg(world, $"Liquid is not fresh water: {code ?? "null"}");
            return false;
        }

        DebugChat.Msg(world, "Fresh water detected");

        if (props.ItemsPerLitre <= 0)
        {
            DebugChat.Msg(world, $"Invalid ItemsPerLitre: {props.ItemsPerLitre}");
            return false;
        }

        float litresPerItem = 1f / props.ItemsPerLitre;
        float availableLitres = sourceStack.StackSize * litresPerItem;

        DebugChat.Msg(world,
            $"ItemsPerLitre={props.ItemsPerLitre}, litresPerItem={litresPerItem}, availableLitres={availableLitres}");

        if (availableLitres <= 0f)
        {
            DebugChat.Msg(world, "No liquid available");
            return false;
        }

        float remainingSeconds = can.GetRemainingWateringSeconds(canSlot.Itemstack);
        float missingSeconds = can.CapacitySeconds - remainingSeconds;
        float secondsPerLitre = can.CapacitySeconds / LitresForFullCan;

        DebugChat.Msg(world,
            $"remainingSeconds={remainingSeconds}, missingSeconds={missingSeconds}, secondsPerLitre={secondsPerLitre}");

        if (missingSeconds <= 0f)
        {
            DebugChat.Msg(world, "Can already full");
            return false;
        }

        float litresNeeded = missingSeconds / secondsPerLitre;
        float litresToTransfer = Math.Min(
            LitresPerTransfer,
            Math.Min(availableLitres, litresNeeded)
        );

        DebugChat.Msg(world,
            $"litresNeeded={litresNeeded}, litresToTransfer={litresToTransfer}");

        if (litresToTransfer <= 0f)
        {
            DebugChat.Msg(world, "Nothing to transfer");
            return false;
        }

        float secondsToAdd = litresToTransfer * secondsPerLitre;
        float newSeconds = GameMath.Clamp(
            remainingSeconds + secondsToAdd,
            0,
            can.CapacitySeconds
        );

        can.SetRemainingWateringSeconds(canSlot.Itemstack, newSeconds);

        DebugChat.Msg(world, $"secondsToAdd={secondsToAdd}, newSeconds={newSeconds}");

        int itemsToTake = (int)Math.Ceiling(litresToTransfer * props.ItemsPerLitre);
        itemsToTake = Math.Min(itemsToTake, sourceStack.StackSize);

        DebugChat.Msg(world, $"itemsToTake={itemsToTake}");

        sourceStack.StackSize -= itemsToTake;

        if (sourceStack.StackSize <= 0)
        {
            sourceSlot.Itemstack = null;
            DebugChat.Msg(world, "Source emptied");
        }

        sourceSlot.MarkDirty();
        canSlot.MarkDirty();

        if (soundPos != null)
        {
            world.PlaySoundAt(
                new AssetLocation("sounds/block/water"),
                soundPos.X,
                soundPos.Y,
                soundPos.Z,
                byPlayer
            );
            DebugChat.Msg(world, "Played water sound");
        }

        DebugChat.Msg(world, "Transfer complete");
        return true;
    }
}