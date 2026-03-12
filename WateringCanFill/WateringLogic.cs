using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace WateringCanFill;

public static class WateringLogic
{
    public const float LitresForFullCan = 5f;

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

        DebugChat.Msg(world,
            $"remainingSeconds={remainingSeconds}, missingSeconds={missingSeconds}");

        if (missingSeconds <= 0f)
        {
            DebugChat.Msg(world, "Can already full");
            return false;
        }

        float secondsToAdd;

        // --- Используем проценты если они заданы ---
        if (ConfigManager.Config.FillPercentPerTransfer > 0)
        {
            float percent = ConfigManager.Config.FillPercentPerTransfer / 100f;

            secondsToAdd = can.CapacitySeconds * percent;

            DebugChat.Msg(world,
                $"Using percent mode: {ConfigManager.Config.FillPercentPerTransfer}% → secondsToAdd={secondsToAdd}");
        }
        else
        {
            float secondsPerLitre = can.CapacitySeconds / LitresForFullCan;

            float litresToTransfer = Math.Min(
                ConfigManager.Config.LitresPerTransfer,
                Math.Min(availableLitres, missingSeconds / secondsPerLitre)
            );

            DebugChat.Msg(world,
                $"Using litre mode: litresToTransfer={litresToTransfer}");

            if (litresToTransfer <= 0f)
            {
                DebugChat.Msg(world, "Nothing to transfer");
                return false;
            }

            secondsToAdd = litresToTransfer * secondsPerLitre;
        }

        float newSeconds = GameMath.Clamp(
            remainingSeconds + secondsToAdd,
            0,
            can.CapacitySeconds
        );

        can.SetRemainingWateringSeconds(canSlot.Itemstack, newSeconds);

        DebugChat.Msg(world,
            $"secondsToAdd={secondsToAdd}, newSeconds={newSeconds}");

        // --- списываем воду из контейнера ---

        float secondsPerLitreReal = can.CapacitySeconds / LitresForFullCan;

        float litresUsed = secondsToAdd / secondsPerLitreReal;

        int itemsToTake = (int)Math.Ceiling(litresUsed * props.ItemsPerLitre);

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