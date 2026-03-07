using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace WateringCanFill;

public static class DebugChat
{
    public static void Msg(IWorldAccessor world, string msg)
    {
        if (!DebugState.Enabled)
            return;

        if (world.Api is ICoreClientAPI capi)
        {
            capi.ShowChatMessage("[WateringCanFill] " + msg);
        }
    }
}