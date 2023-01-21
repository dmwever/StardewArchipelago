﻿using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Quests;

namespace StardewArchipelago.Locations
{
    public class LocationPatcher
    {
        private readonly ArchipelagoClient _archipelago;
        private readonly Harmony _harmony;

        public LocationPatcher(IMonitor monitor, ArchipelagoClient archipelago, BundleReader bundleReader, IModHelper modHelper, Harmony harmony, LocationChecker locationChecker)
        {
            _archipelago = archipelago;
            _harmony = harmony;
            LocationsCodeInjections.Initialize(monitor, modHelper, _archipelago, bundleReader, locationChecker);
        }

        public void ReplaceAllLocationsRewardsWithChecks()
        {
            RemoveDefaultRewardsOnAllBundles();
            ReplaceCommunityCenterBundlesWithChecks();
            ReplaceCommunityCenterAreasWithChecks();
            ReplaceBackPackUpgradesWithChecks();
            ReplaceMineshaftChestsWithChecks();
            ReplaceElevatorsWithChecks();
            ReplaceToolUpgradesWithChecks();
            ReplaceFishingRodsWithChecks();
            ReplaceSkillsWithChecks();
            ReplaceQuestsWithChecks();
            ReplaceCarpenterBuildingsWithChecks();
            ReplaceWizardBuildingsWithChecks();
            ReplaceIsolatedEventsWithChecks();
            ReplaceArcadeMachinesWithChecks();
        }

        private static void RemoveDefaultRewardsOnAllBundles()
        {
            foreach (var key in Game1.netWorldState.Value.BundleData.Keys)
            {
                var splitKey = key.Split('/');
                var value = Game1.netWorldState.Value.BundleData[key];
                var splitValue = value.Split('/');

                var areaName = splitKey[0];
                var bundleReward = splitValue[1];

                if (bundleReward == "")
                {
                    continue;
                }

                Game1.netWorldState.Value.BundleData[key] = value.Replace(bundleReward, "");
            }
        }

        private void ReplaceCommunityCenterBundlesWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(JunimoNoteMenu), nameof(JunimoNoteMenu.checkForRewards)),
                postfix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.CheckForRewards_PostFix))
            );
        }

        private void ReplaceCommunityCenterAreasWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(CommunityCenter), "doAreaCompleteReward"),
                prefix: new HarmonyMethod(typeof(CommunityCenterInjections), nameof(CommunityCenterInjections.DoAreaCompleteReward_AreaLocations_Prefix))
            );
        }

        private void ReplaceBackPackUpgradesWithChecks()
        {
            if (_archipelago.SlotData.BackpackProgression == BackpackProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.PerformAction_BuyBackpack_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(BackpackInjections), nameof(BackpackInjections.AnswerDialogueAction_BackPackPurchase_Prefix))
            );

            // This would need a transpile patch for SeedShop.draw and I don't think it's worth it.
            // _harmony.Patch(
            //     original: AccessTools.Method(typeof(SeedShop), nameof(SeedShop.draw)),
            //     transpiler: new HarmonyMethod(typeof(BackpackInjections), nameof(LocationsCodeInjections.AnswerDialogueAction_BackPackPurchase_Prefix)));
        }

        private void ReplaceMineshaftChestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Chest), nameof(Chest.checkForAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.CheckForAction_MineshaftChest_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), "addLevelChests"),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.AddLevelChests_Level120_Prefix))
            );
        }

        private void ReplaceToolUpgradesWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(ScytheInjections), nameof(ScytheInjections.PerformAction_GoldenScythe_Prefix))
            );

            if (_archipelago.SlotData.ToolProgression == ToolProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(ToolInjections), nameof(ToolInjections.AnswerDialogueAction_ToolUpgrade_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Tool), nameof(Tool.actionWhenPurchased)),
                prefix: new HarmonyMethod(typeof(ToolInjections), nameof(ToolInjections.ActionWhenPurchased_ToolUpgrade_Prefix))
            );
        }

        private void ReplaceFishingRodsWithChecks()
        {
            if (_archipelago.SlotData.ToolProgression == ToolProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.skipEvent)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.SkipEvent_BambooPole_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_awardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.AwardFestivalPrize_BambooPole_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getFishShopStock)),
                prefix: new HarmonyMethod(typeof(FishingRodInjections), nameof(FishingRodInjections.GetFishShopStock_Prefix))
            );
        }

        private void ReplaceElevatorsWithChecks()
        {
            if (_archipelago.SlotData.ElevatorProgression == ElevatorProgression.Vanilla)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.enterMine)),
                postfix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.EnterMine_SendElevatorCheck_PostFix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.PerformAction_LoadElevatorMenu_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkAction)),
                prefix: new HarmonyMethod(typeof(MineshaftInjections), nameof(MineshaftInjections.CheckAction_LoadElevatorMenu_Prefix))
            );
        }

        private void ReplaceSkillsWithChecks()
        {
            if (_archipelago.SlotData.SkillProgression == SkillsProgression.Vanilla)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                    prefix: new HarmonyMethod(typeof(SkillInjections), nameof(SkillInjections.GainExperience_NormalExperience_Prefix))
                );
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                prefix: new HarmonyMethod(typeof(SkillInjections), nameof(SkillInjections.GainExperience_ArchipelagoExperience_Prefix))
            );
        }

        private void ReplaceQuestsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Quest), nameof(Quest.questComplete)),
                prefix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.QuestComplete_LocationInsteadOfReward_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_removeQuest)),
                postfix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.Command_RemoveQuest_CheckLocation_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Mountain), nameof(Mountain.checkAction)),
                prefix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.CheckAction_AdventurerGuild_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.PerformAction_MysteriousQiLumberPile_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Town), "mgThief_afterSpeech"),
                prefix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.MgThief_AfterSpeech_WinterMysteryFinished_Prefix))
            );

            var desiredSkillsPageCtorParameters = new[] { typeof(int), typeof(int), typeof(int), typeof(int) };
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(SkillsPage), desiredSkillsPageCtorParameters),
                postfix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.SkillsPageCtor_BearKnowledge_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Object), "getPriceAfterMultipliers"),
                postfix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.GetPriceAfterMultipliers_BearKnowledge_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Event), nameof(Event.command_awardFestivalPrize)),
                prefix: new HarmonyMethod(typeof(QuestInjections), nameof(QuestInjections.Command_AwardFestivalPrize_QiMilk_Prefix))
            );
        }

        private void ReplaceCarpenterBuildingsWithChecks()
        {
            if (_archipelago.SlotData.BuildingProgression != BuildingProgression.Shuffled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.AnswerDialogueAction_CarpenterConstruct_Prefix))
            );

            var desiredOverloadParameters = new[] { typeof(string), typeof(Response[]), typeof(string) };
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.createQuestionDialogue), desiredOverloadParameters),
                prefix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.CreateQuestionDialogue_CarpenterDialogOptions_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(Utility), nameof(Utility.getCarpenterStock)),
                postfix: new HarmonyMethod(typeof(CarpenterInjections), nameof(CarpenterInjections.GetCarpenterStock_PurchasableChecks_Postfix))
            );
        }

        private void ReplaceWizardBuildingsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction)),
                prefix: new HarmonyMethod(typeof(WizardInjections), nameof(WizardInjections.PerformAction_WizardBook_Prefix))
            );
        }

        private void ReplaceIsolatedEventsWithChecks()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(Woods), nameof(Woods.checkAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.CheckAction_OldMasterCanolli_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.answerDialogueAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.AnswerDialogueAction_BeachBridge_Prefix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Beach), nameof(Beach.checkAction)),
                prefix: new HarmonyMethod(typeof(IsolatedEventInjections), nameof(IsolatedEventInjections.CheckAction_BeachBridge_Prefix))
            );
        }

        private void ReplaceArcadeMachinesWithChecks()
        {
            if (_archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.Disabled)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.usePowerup)),
                prefix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.UsePowerup_PrairieKingBossBeaten_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), nameof(MineCart.EndCutscene)),
                prefix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.EndCutscene_JunimoKartLevelComplete_Prefix))
            );

            if (_archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.Victories)
            {
                return;
            }

            _harmony.Patch(
                original: AccessTools.Method(typeof(AbigailGame.CowboyMonster), nameof(AbigailGame.CowboyMonster.getLootDrop)),
                prefix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.GetLootDrop_ExtraLoot_Prefix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), "restartLevel"),
                postfix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.RestartLevel_NewGame_Postfix))
            );

            _harmony.Patch(
                original: AccessTools.Method(typeof(MineCart), nameof(MineCart.UpdateFruitsSummary)),
                postfix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.UpdateFruitsSummary_ExtraLives_Postfix))
            );

            var desiredAbigailGameCtorParameters = new[] { typeof(bool) };
            _harmony.Patch(
                original: AccessTools.Constructor(typeof(AbigailGame), desiredAbigailGameCtorParameters),
                postfix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.AbigailGameCtor_Equipments_Postfix))
            );

            if (_archipelago.SlotData.ArcadeMachineProgression == ArcadeProgression.FullShuffling)
            {
                _harmony.Patch(
                    original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.startShoppingLevel)),
                    postfix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.StartShoppingLevel_ShopBasedOnSentChecks_PostFix))
                );

                _harmony.Patch(
                    original: AccessTools.Method(typeof(AbigailGame), nameof(AbigailGame.tick)),
                    postfix: new HarmonyMethod(typeof(ArcadeMachineInjections), nameof(ArcadeMachineInjections.Tick_Shopping_PostFix))
                );
            }
        }
    }
}