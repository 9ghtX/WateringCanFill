# Watering Can Fill

A small **Vintage Story mod** that allows watering cans to be refilled from **buckets, barrels, and other liquid containers**, instead of only natural water sources.

This improves immersion for players who disable portable water sources or prefer more realistic water logistics.

## Features

-   Fill watering cans from **buckets placed on the ground**
-   Fill watering cans from **barrels**
-   Works with **any liquid container based on `BlockEntityLiquidContainer`**
-   Transfers water in **1 litre portions**
-   **5 litres fully fills a watering can**
-   Prevents barrel UI from opening when using a watering can

## Why this mod exists

In vanilla Vintage Story, watering cans can normally only be refilled from:

-   water blocks (rivers, lakes, etc.)

If portable water sources are disabled for realism, watering cans become unnecessarily inconvenient to refill.

This mod allows refilling from existing liquid containers such as barrels and buckets, which fits better with realistic base setups.

## Compatibility

The mod is designed to work automatically with:

-   vanilla **buckets**
-   vanilla **barrels**
-   modded **liquid containers**

Any block entity derived from `BlockEntityLiquidContainer` should work without additional patches.

## Planned Features

-   Filling watering cans from **cooking containers**
-   Additional compatibility with modded liquid systems if needed

## Technical Notes

Water transfer is handled via Harmony patches and works by reading liquid contents directly from container inventories.

## License

MIT License
