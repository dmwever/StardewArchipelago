﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Tiles;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.Locations.CodeInjections
{
    public class MineshaftInjections
    {
        public const string RECEIVED_MINE_ELEVATOR_KEY = "MineElevator_Received_Level_Key";

        private static IMonitor _monitor;
        private static Action<string> _addCheckedLocation;
        private static ModPersistence _modPersistence;

        public MineshaftInjections(IMonitor monitor, Action<string> addCheckedLocation)
        {
            _monitor = monitor;
            _addCheckedLocation = addCheckedLocation;
            _modPersistence = new ModPersistence();
        }

        public static bool CheckForAction_MineshaftChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value || Game1.mine == null)
                {
                    return true; // run original logic
                }

                if (__instance.items.Count <= 0)
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                Game1.mine.chestConsumed();
                var obj = __instance.items[0];
                __instance.items[0] = null;
                __instance.items.RemoveAt(0);
                __result = true;

                _addCheckedLocation($"The Mines Floor {Game1.mine.mineLevel} Treasure");

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForAction_MineshaftChest_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool AddLevelChests_Level120_Prefix(MineShaft __instance)
        {
            try
            {
                if (__instance.mineLevel != 120 || Game1.player.chestConsumedMineLevels.ContainsKey(120))
                {
                    return true; // run original logic
                }

                Game1.player.completeQuest(18);
                Game1.getSteamAchievement("Achievement_TheBottom");
                var chestPosition = new Vector2(9f, 9f);
                var items = new List<Item>();
                items.Add(new MeleeWeapon(8));
                __instance.overlayObjects[chestPosition] = new Chest(0, items, chestPosition)
                {
                    Tint = Color.Pink
                };

                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(AddLevelChests_Level120_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static void EnterMine_SendElevatorCheck_PostFix(int whatLevel)
        {
            try
            {
                if (whatLevel < 5 || whatLevel > 120 || whatLevel % 5 != 0)
                {
                    return;
                }

                _addCheckedLocation($"The Mines Floor {whatLevel} Elevator");
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_LoadElevatorMenu_Prefix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static bool PerformAction_LoadElevatorMenu_Prefix(GameLocation __instance, string action, Farmer who,
            Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer || action.Split(' ')[0] != "MineElevator")
                {
                    return true; // run original logic
                }

                CreateElevatorMenuIfUnlocked();
                __result = true;
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_LoadElevatorMenu_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        public static bool CheckAction_LoadElevatorMenu_Prefix(MineShaft __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                Tile tile = __instance.map.GetLayer("Buildings").PickTile(new Location(tileLocation.X * 64, tileLocation.Y * 64), viewport.Size);

                if (tile == null || !who.IsLocalPlayer || tile.TileIndex != 112 || __instance.mineLevel > 120)
                {
                    return true; // run original logic
                }

                CreateElevatorMenuIfUnlocked();
                __result = true;
                return false; // don't run original logic

            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckAction_LoadElevatorMenu_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static void CreateElevatorMenuIfUnlocked()
        {
            InitializeMineElevatorModDataValues();
            var numberOfMineElevatorReceived = _modPersistence.GetAsInt(RECEIVED_MINE_ELEVATOR_KEY);
            var mineLevelUnlocked = numberOfMineElevatorReceived * 5;
            mineLevelUnlocked = Math.Min(120, Math.Max(0, mineLevelUnlocked));

            if (mineLevelUnlocked < 5)
            {
                Game1.drawObjectDialogue(
                    Game1.parseText(Game1.content.LoadString("Strings\\Locations:Mines_MineElevator_NotWorking")));
            }
            else
            {
                var previousMaxLevel = MineShaft.lowestLevelReached;
                MineShaft.lowestLevelReached = mineLevelUnlocked;
                Game1.activeClickableMenu = new MineElevatorMenu();
                MineShaft.lowestLevelReached = previousMaxLevel;
            }
        }

        public static void InitializeMineElevatorModDataValues()
        {
            _modPersistence.InitializeModDataValue(RECEIVED_MINE_ELEVATOR_KEY, "0");
        }
    }
}