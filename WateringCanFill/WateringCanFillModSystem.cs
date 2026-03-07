using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Client;

namespace WateringCanFill;

public class WateringCanFillModSystem : ModSystem
{
    private Harmony harmony;

    public override void Start(ICoreAPI api)
    {
        harmony = new Harmony("wateringcanfill");
        harmony.PatchAll();

        if (api is ICoreClientAPI capi)
        {
            capi.Event.RegisterGameTickListener(_ =>
            {
                var player = capi.World.Player;
                if (player == null)
                {
                    capi.ShowChatMessage(
                        $"Didn't get player from API"
                    );
                    return;
                }

                capi.ShowChatMessage(
                    $"Got player from API!"
                );

                var slot = player.InventoryManager.ActiveHotbarSlot;
                var stack = slot?.Itemstack;

                if (stack == null)
                {
                    capi.ShowChatMessage(
                        $"Didn't  get stack from Hotbar Slot"
                    );
                    return;
                }
                
                capi.ShowChatMessage(
                    $"Got stack from Hotbar Slot!"
                );

                capi.ShowChatMessage(
                    $"Code: {stack.Collectible.Code} | Class: {stack.Collectible.GetType().FullName}"
                );

            }, 2000);
        }
    }
}