﻿using StardewArchipelago.Archipelago;
using StardewArchipelago.GameModifications;
using StardewArchipelago.GameModifications.Modded;
using StardewArchipelago.Locations.CodeInjections.Vanilla.MonsterSlayer;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;

namespace StardewArchipelago.Locations.CodeInjections.Initializers
{
    public static class CodeInjectionInitializer
    {
        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, StardewItemManager itemManager, WeaponsManager weaponsManager, SeedShopStockModifier seedShopStockModifier, JunimoShopGenerator junimoShopGenerator, Friends friends)
        {
            var shopReplacer = new ShopReplacer(monitor, modHelper, archipelago, locationChecker);
            VanillaCodeInjectionInitializer.Initialize(monitor, modHelper, archipelago, state, locationChecker, itemManager, weaponsManager, shopReplacer, friends);
            if (archipelago.SlotData.Mods.IsModded)
            {
                ModCodeInjectionInitializer.Initialize(monitor, modHelper, archipelago, locationChecker, shopReplacer, seedShopStockModifier, junimoShopGenerator);
            }
        }
    }
}
