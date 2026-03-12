using Vintagestory.API.Common;

namespace WateringCanFill;

public static class ConfigManager
{
    public static WateringCanFillConfig Config = new();

    public static void Load(ICoreAPI api)
    {
        try
        {
            Config = api.LoadModConfig<WateringCanFillConfig>("WateringCanFill.json");

            if (Config == null)
                Config = new WateringCanFillConfig();

            api.StoreModConfig(Config, "WateringCanFill.json");
        }
        catch
        {
            Config = new WateringCanFillConfig();
        }
    }
}