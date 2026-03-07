using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace WateringCanFill;

public class WateringCanFillModSystem : ModSystem
{
    private Harmony? harmony;

    public override void Start(ICoreAPI api)
    {
        harmony = new Harmony("wateringcanfill");
        harmony.PatchAll();

        api.Logger.Notification("WateringCanFill: patches applied");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        api.ChatCommands
            .Create("wateringdebug")
            .WithDescription("Toggle WateringCanFill debug chat output")
            .HandleWith(_ =>
            {
                DebugState.Enabled = !DebugState.Enabled;

                api.ShowChatMessage(
                    "[WateringCanFill] Debug " +
                    (DebugState.Enabled ? "enabled" : "disabled")
                );

                return TextCommandResult.Success();
            });
    }

    public override void Dispose()
    {
        harmony?.UnpatchAll("wateringcanfill");
        harmony = null;
    }
}