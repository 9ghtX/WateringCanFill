using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace WateringCanFill;

[HarmonyPatch(typeof(BlockEntityBarrel), "OnPlayerRightClick")]
public static class BarrelUIPatch
{
    static bool Prefix(BlockEntityBarrel __instance, IPlayer byPlayer)
    {
        var slot = byPlayer?.InventoryManager?.ActiveHotbarSlot;
        var stack = slot?.Itemstack;

        if (stack?.Block is BlockWateringCan)
        {
            DebugChat.Msg(__instance.Api.World, "Barrel UI blocked (watering can in hand)");
            return false;
        }

        return true;
    }
}