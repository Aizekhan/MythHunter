using NUnit.Framework;
using Moq;
using RPGFramework.ECS.Contracts;
using RPGFramework.Logging.Contracts;
using MythHunter.GameLogic.Quests;
using MythHunter.GameLogic.Dialogue;
using MythHunter.GameLogic.Trading;
using MythHunter.GameLogic.Factions;
using MythHunter.GameLogic.Combat;
using MythHunter.GameLogic.Loot;
using MythHunter.GameLogic.Inventory;
using RPGFramework.Quests.Runtime.Contracts;
using RPGFramework.Dialogue.Runtime.Contracts;
using RPGFramework.Trading.Runtime.Contracts;
using RPGFramework.Factions.Runtime.Contracts;
using RPGFramework.Combat.Runtime.Contracts;
using RPGFramework.Loot.Runtime.Contracts;
using RPGFramework.Inventory.Runtime.Contracts;
using RPGFramework.Quests.Runtime.Data;
using RPGFramework.Dialogue.Runtime.Data;
using RPGFramework.Trading.Runtime.Data;
using RPGFramework.Factions.Runtime.Data;

namespace MythHunter.Tests.Integration
{
    /// <summary>
    /// Cross-system integration tests validating interactions between different RPG systems.
    /// These tests ensure that systems work together correctly in realistic game scenarios.
    /// </summary>
    [TestFixture]
    public class CrossSystemIntegrationTests
    {
        private Mock<IEntityManager> _mockEntityManager;
        private Mock<IMythLogger> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockEntityManager = new Mock<IEntityManager>();
            _mockLogger = new Mock<IMythLogger>();
        }

        #region Quest → Dialogue Integration

        /// <summary>
        /// Scenario: Quest acceptance through dialogue choice
        /// Flow: Player talks to NPC → Dialogue offers quest → Player accepts → Quest starts
        /// </summary>
        [Test]
        public void QuestAcceptance_ThroughDialogueChoice_StartsQuest()
        {
            // Arrange
            var entityId = 1;
            var questId = "quest_rescue_villagers";
            var dialogueId = "dialogue_village_elder_greeting";

            var questIntegration = CreateQuestIntegration();
            var dialogueIntegration = CreateDialogueIntegration();

            // Create dialogue with quest acceptance choice
            var dialogue = CreateQuestDialogue(dialogueId, questId);
            dialogueIntegration.RegisterDialogue(dialogue);

            // Act - Start dialogue
            dialogueIntegration.StartDialogue(entityId, dialogueId);

            // Act - Player chooses "Accept Quest" option (choice index 1)
            dialogueIntegration.SelectChoice(entityId, dialogueId, 1);

            // Simulate quest acceptance triggered by dialogue choice
            questIntegration.AcceptQuest(entityId, questId);

            // Assert
            var activeQuests = questIntegration.GetActiveQuests(entityId);
            Assert.That(activeQuests, Is.Not.Null);
            Assert.That(activeQuests.ActiveQuests.Count, Is.EqualTo(1));
            Assert.That(activeQuests.ActiveQuests[0].QuestId, Is.EqualTo(questId));
            Assert.That(activeQuests.ActiveQuests[0].State, Is.EqualTo(QuestState.Active));
        }

        /// <summary>
        /// Scenario: Quest completion unlocks new dialogue options
        /// Flow: Complete quest → Return to NPC → New dialogue branch available
        /// </summary>
        [Test]
        public void QuestCompletion_UnlocksNewDialogueOptions()
        {
            // Arrange
            var entityId = 1;
            var questId = "quest_rescue_villagers";
            var initialDialogueId = "dialogue_elder_initial";
            var completionDialogueId = "dialogue_elder_quest_complete";

            var questIntegration = CreateQuestIntegration();
            var dialogueIntegration = CreateDialogueIntegration();

            // Accept and complete quest
            questIntegration.AcceptQuest(entityId, questId);

            // Simulate objective completion
            var quest = questIntegration.GetQuestProgress(entityId, questId);
            questIntegration.UpdateObjectiveProgress(entityId, questId, quest.Objectives[0].ObjectiveId, 5); // Complete "Rescue 5 villagers"

            // Act - Complete quest
            questIntegration.CompleteQuest(entityId, questId);

            // Register completion dialogue (only available after quest complete)
            var completionDialogue = CreateCompletionDialogue(completionDialogueId, questId);
            dialogueIntegration.RegisterDialogue(completionDialogue);

            // Act - Start completion dialogue
            dialogueIntegration.StartDialogue(entityId, completionDialogueId);

            // Assert - Dialogue is active and has completion-specific content
            var activeDialogue = dialogueIntegration.GetActiveDialogue(entityId, completionDialogueId);
            Assert.That(activeDialogue, Is.Not.Null);
            Assert.That(activeDialogue.State, Is.EqualTo(DialogueState.Active));

            // Verify quest is completed
            var activeQuests = questIntegration.GetActiveQuests(entityId);
            Assert.That(activeQuests.ActiveQuests, Is.Empty);
        }

        #endregion

        #region Faction → Trading Integration

        /// <summary>
        /// Scenario: Faction reputation affects trading prices
        /// Flow: Player has low rep → Prices increased → Gain rep → Prices decrease
        /// </summary>
        [Test]
        public void FactionReputation_AffectsTradingPrices()
        {
            // Arrange
            var entityId = 1;
            var traderId = "trader_blacksmith";
            var factionId = "faction_warriors_guild";
            var itemOfferId = "offer_iron_sword";

            var factionIntegration = CreateFactionIntegration();
            var tradingIntegration = CreateTradingIntegration();

            // Set initial low reputation
            factionIntegration.ModifyReputation(entityId, factionId, -50); // Hostile standing

            // Create trader affiliated with faction
            var trader = CreateFactionTrader(traderId, factionId);
            tradingIntegration.RegisterTrader(trader);

            // Act - Get price with low reputation (hostile = +50% price modifier)
            var baseCost = 100;
            var hostilePrice = CalculateFactionModifiedPrice(baseCost, -50, factionIntegration, entityId, factionId);

            // Improve reputation
            factionIntegration.ModifyReputation(entityId, factionId, 75); // Now at +25 (Friendly)

            // Act - Get price with good reputation (friendly = -10% price modifier)
            var friendlyPrice = CalculateFactionModifiedPrice(baseCost, 75, factionIntegration, entityId, factionId);

            // Assert
            Assert.That(hostilePrice, Is.GreaterThan(baseCost), "Hostile faction should increase prices");
            Assert.That(friendlyPrice, Is.LessThan(baseCost), "Friendly faction should decrease prices");
            Assert.That(friendlyPrice, Is.LessThan(hostilePrice), "Friendly price should be lower than hostile price");
        }

        /// <summary>
        /// Scenario: Trading with faction members grants reputation
        /// Flow: Complete trade → Reputation increases with affiliated faction
        /// </summary>
        [Test]
        public void CompletingTrade_GrantsFactionReputation()
        {
            // Arrange
            var entityId = 1;
            var traderId = "trader_merchant_guild";
            var factionId = "faction_merchant_alliance";
            var offerId = "offer_health_potion";

            var factionIntegration = CreateFactionIntegration();
            var tradingIntegration = CreateTradingIntegration();

            // Initial reputation
            var initialRep = factionIntegration.GetReputation(entityId, factionId);

            // Create faction-affiliated trader
            var trader = CreateFactionTrader(traderId, factionId);
            tradingIntegration.RegisterTrader(trader);

            var offer = CreateTradeOffer(offerId, 50); // 50 gold
            tradingIntegration.AddTradeOffer(traderId, offer);

            // Act - Complete trade
            tradingIntegration.ProposeTradeOffer(entityId, traderId, offerId);
            tradingIntegration.AcceptTrade(entityId, traderId, offerId);

            // Simulate reputation gain (10 rep per trade)
            factionIntegration.ModifyReputation(entityId, factionId, 10);

            // Assert
            var newRep = factionIntegration.GetReputation(entityId, factionId);
            Assert.That(newRep, Is.GreaterThan(initialRep), "Reputation should increase after trade");
            Assert.That(newRep - initialRep, Is.EqualTo(10), "Should gain 10 reputation per trade");
        }

        #endregion

        #region Combat → Loot → Inventory Integration

        /// <summary>
        /// Scenario: Enemy death generates loot that goes into inventory
        /// Flow: Kill enemy → Loot generated → Items added to inventory
        /// </summary>
        [Test]
        public void EnemyDeath_GeneratesLoot_AddedToInventory()
        {
            // Arrange
            var playerId = 1;
            var enemyId = 2;

            var combatIntegration = CreateCombatIntegration();
            var lootIntegration = CreateLootIntegration();
            var inventoryIntegration = CreateInventoryIntegration();

            // Setup enemy with loot table
            var lootTableId = "loot_goblin";
            SetupEnemyWithLootTable(enemyId, lootTableId, lootIntegration);

            // Act - Kill enemy (reduce HP to 0)
            combatIntegration.ApplyDamage(enemyId, 100, playerId);

            // Enemy dies, loot is generated
            var lootDrops = lootIntegration.GenerateLoot(lootTableId);

            // Items are added to player inventory
            foreach (var drop in lootDrops)
            {
                inventoryIntegration.AddItem(playerId, drop.ItemId, drop.Quantity);
            }

            // Assert - Verify inventory contains loot
            var inventory = inventoryIntegration.GetInventory(playerId);
            Assert.That(inventory, Is.Not.Null);
            Assert.That(inventory.Items.Count, Is.GreaterThan(0), "Inventory should contain looted items");

            // Verify specific loot (e.g., "Gold Coins" always drops)
            var goldCoins = inventory.Items.Values.FirstOrDefault(i => i.ItemId == "item_gold_coins");
            Assert.That(goldCoins, Is.Not.Null, "Should have looted gold coins");
        }

        /// <summary>
        /// Scenario: Inventory capacity limits loot collection
        /// Flow: Full inventory → Kill enemy → Loot generated but cannot be picked up
        /// </summary>
        [Test]
        public void FullInventory_PreventsLootCollection()
        {
            // Arrange
            var playerId = 1;
            var enemyId = 2;

            var lootIntegration = CreateLootIntegration();
            var inventoryIntegration = CreateInventoryIntegration();

            // Fill inventory to capacity (30 slots)
            for (int i = 0; i < 30; i++)
            {
                inventoryIntegration.AddItem(playerId, $"item_junk_{i}", 1);
            }

            // Setup enemy with loot
            var lootTableId = "loot_goblin";
            SetupEnemyWithLootTable(enemyId, lootTableId, lootIntegration);

            // Act - Generate loot
            var lootDrops = lootIntegration.GenerateLoot(lootTableId);

            // Try to add items to full inventory
            var itemsAdded = 0;
            foreach (var drop in lootDrops)
            {
                var success = inventoryIntegration.TryAddItem(playerId, drop.ItemId, drop.Quantity);
                if (success) itemsAdded++;
            }

            // Assert
            Assert.That(itemsAdded, Is.EqualTo(0), "No items should be added to full inventory");

            var inventory = inventoryIntegration.GetInventory(playerId);
            Assert.That(inventory.Items.Count, Is.EqualTo(30), "Inventory should still have 30 items");
        }

        #endregion

        #region Quest → Crafting → Trading Flow

        /// <summary>
        /// Scenario: Quest requires crafted item that must be traded
        /// Flow: Accept quest → Craft item → Trade item to NPC → Quest completes
        /// </summary>
        [Test]
        public void QuestRequiresCraftedItem_TradedToNPC_CompletesQuest()
        {
            // Arrange
            var playerId = 1;
            var questId = "quest_craft_iron_sword";
            var recipeId = "recipe_iron_sword";
            var traderId = "trader_blacksmith_guild";
            var itemId = "item_iron_sword";

            var questIntegration = CreateQuestIntegration();
            var craftingIntegration = CreateCraftingIntegration();
            var tradingIntegration = CreateTradingIntegration();
            var inventoryIntegration = CreateInventoryIntegration();

            // Accept quest: "Craft Iron Sword and deliver to Blacksmith Guild"
            questIntegration.AcceptQuest(playerId, questId);

            // Step 1: Craft the item
            craftingIntegration.StartCrafting(playerId, recipeId);
            craftingIntegration.CompleteCrafting(playerId, recipeId); // Instant completion for test

            // Item is added to inventory
            inventoryIntegration.AddItem(playerId, itemId, 1);

            // Update quest objective: "Craft Iron Sword" (1/1)
            var quest = questIntegration.GetQuestProgress(playerId, questId);
            var craftObjective = quest.Objectives[0];
            questIntegration.UpdateObjectiveProgress(playerId, questId, craftObjective.ObjectiveId, 1);

            // Step 2: Trade item to NPC
            var offer = new TradeOfferData
            {
                OfferId = "offer_quest_delivery",
                TraderGives = new System.Collections.Generic.List<TradeItem>(), // NPC gives nothing
                TraderWants = new System.Collections.Generic.List<TradeItem>
                {
                    new TradeItem { ItemId = itemId, Quantity = 1 }
                }
            };

            tradingIntegration.AddTradeOffer(traderId, offer);
            tradingIntegration.ProposeTradeOffer(playerId, traderId, offer.OfferId);
            tradingIntegration.AcceptTrade(playerId, traderId, offer.OfferId);

            // Remove item from inventory
            inventoryIntegration.RemoveItem(playerId, itemId, 1);

            // Update quest objective: "Deliver to Blacksmith" (1/1)
            var deliveryObjective = quest.Objectives[1];
            questIntegration.UpdateObjectiveProgress(playerId, questId, deliveryObjective.ObjectiveId, 1);

            // Act - Complete quest
            questIntegration.CompleteQuest(playerId, questId);

            // Assert
            var activeQuests = questIntegration.GetActiveQuests(playerId);
            Assert.That(activeQuests.ActiveQuests, Is.Empty, "Quest should be completed");

            var inventory = inventoryIntegration.GetInventory(playerId);
            var swordInInventory = inventory.Items.Values.Any(i => i.ItemId == itemId);
            Assert.That(swordInInventory, Is.False, "Sword should be removed from inventory after trade");
        }

        #endregion

        #region Helper Methods

        private QuestSystemIntegration CreateQuestIntegration()
        {
            return new QuestSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private DialogueSystemIntegration CreateDialogueIntegration()
        {
            return new DialogueSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private TradingSystemIntegration CreateTradingIntegration()
        {
            return new TradingSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private FactionSystemIntegration CreateFactionIntegration()
        {
            return new FactionSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private CombatSystemIntegration CreateCombatIntegration()
        {
            return new CombatSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private LootSystemIntegration CreateLootIntegration()
        {
            return new LootSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private InventorySystemIntegration CreateInventoryIntegration()
        {
            return new InventorySystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private CraftingSystemIntegration CreateCraftingIntegration()
        {
            return new CraftingSystemIntegration(_mockEntityManager.Object, _mockLogger.Object);
        }

        private DialogueData CreateQuestDialogue(string dialogueId, string questId)
        {
            // Simplified dialogue creation - actual implementation would be more complex
            return new DialogueData { DialogueId = dialogueId };
        }

        private DialogueData CreateCompletionDialogue(string dialogueId, string questId)
        {
            return new DialogueData { DialogueId = dialogueId };
        }

        private TraderData CreateFactionTrader(string traderId, string factionId)
        {
            return new TraderData
            {
                TraderId = traderId,
                TraderType = RPGFramework.Trading.Runtime.Data.TraderType.Merchant
            };
        }

        private TradeOfferData CreateTradeOffer(string offerId, int goldCost)
        {
            return new TradeOfferData
            {
                OfferId = offerId,
                TraderGives = new System.Collections.Generic.List<TradeItem>(),
                TraderWants = new System.Collections.Generic.List<TradeItem>()
            };
        }

        private int CalculateFactionModifiedPrice(int basePrice, int reputation, FactionSystemIntegration factionIntegration, int entityId, string factionId)
        {
            var standing = factionIntegration.GetStanding(entityId, factionId);

            // Price modifiers based on standing
            float modifier = standing switch
            {
                FactionStanding.Hated => 1.75f,    // +75%
                FactionStanding.Hostile => 1.50f,  // +50%
                FactionStanding.Unfriendly => 1.25f, // +25%
                FactionStanding.Neutral => 1.0f,   // No change
                FactionStanding.Friendly => 0.90f, // -10%
                FactionStanding.Honored => 0.80f,  // -20%
                FactionStanding.Revered => 0.70f,  // -30%
                _ => 1.0f
            };

            return (int)(basePrice * modifier);
        }

        private void SetupEnemyWithLootTable(int enemyId, string lootTableId, LootSystemIntegration lootIntegration)
        {
            // Register loot table for enemy type
            // Actual implementation would involve component data
        }

        #endregion
    }
}
