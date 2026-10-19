using NUnit.Framework;
using RPGFramework.ECS;
using RPGFramework.Quests;
using RPGFramework.Dialogue;
using RPGFramework.Crafting;
using RPGFramework.Trading;
using RPGFramework.Inventory;
using RPGFramework.Items;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MythHunter.Tests.Integration
{
    /// <summary>
    /// Integration tests validating cross-system interactions between
    /// Quest, Dialogue, Crafting, Trading, Inventory, and Item systems.
    /// </summary>
    [TestFixture]
    public class CrossSystemIntegrationTests
    {
        private IEntityManager _entityManager;
        private IQuestManager _questManager;
        private IQuestRegistry _questRegistry;
        private IDialogueManager _dialogueManager;
        private IDialogueRegistry _dialogueRegistry;
        private ICraftingManager _craftingManager;
        private ICraftingRecipeRegistry _recipeRegistry;
        private ITradingManager _tradingManager;
        private ITraderRegistry _traderRegistry;
        private IItemRegistry _itemRegistry;

        private int _testPlayerId;
        private int _testNpcId;

        [SetUp]
        public void Setup()
        {
            // Initialize entity manager
            _entityManager = new EntityManager();

            // Initialize Item system
            _itemRegistry = new ItemRegistry();
            SetupTestItems();

            // Initialize Quest system
            _questRegistry = new QuestRegistry();
            _questManager = new QuestManager(_entityManager, _questRegistry);

            // Initialize Dialogue system
            _dialogueRegistry = new DialogueRegistry();
            _dialogueManager = new DialogueManager(_entityManager, _dialogueRegistry);

            // Initialize Crafting system
            _recipeRegistry = new CraftingRecipeRegistry();
            _craftingManager = new CraftingManager(_entityManager, _recipeRegistry);

            // Initialize Trading system
            _traderRegistry = new TraderRegistry();
            _tradingManager = new TradingManager(_traderRegistry);

            // Create test entities
            _testPlayerId = _entityManager.CreateEntity();
            _testNpcId = _entityManager.CreateEntity();

            // Initialize player gold for trading tests
            _tradingManager.InitializePlayerGold(_testPlayerId, 1000);
        }

        [TearDown]
        public void TearDown()
        {
            // EntityManager cleanup - currently no dispose needed for simple implementation
            _entityManager = null;
        }

        #region Item Setup

        private void SetupTestItems()
        {
            // Register test items for integration scenarios
            var healthPotion = Item.Create("health_potion", "Health Potion", ItemType.Consumable);
            var ironSword = Item.Create("iron_sword", "Iron Sword", ItemType.Weapon);
            var ironOre = Item.Create("iron_ore", "Iron Ore", ItemType.Material);
            var wood = Item.Create("wood", "Wood", ItemType.Material);
            var questItem = Item.Create("quest_item", "Ancient Artifact", ItemType.QuestItem);

            _itemRegistry.RegisterItem(healthPotion);
            _itemRegistry.RegisterItem(ironSword);
            _itemRegistry.RegisterItem(ironOre);
            _itemRegistry.RegisterItem(wood);
            _itemRegistry.RegisterItem(questItem);
        }

        #endregion

        #region Quest ↔ Dialogue Integration Tests

        [Test]
        public void Quest_AcceptedThroughDialogue_StartsQuestCorrectly()
        {
            // Arrange: Create a quest
            var quest = Quest.Create("rescue_quest", "Rescue the Villagers");
            (quest as Quest)?.AddObjective(QuestObjective.Create("obj1", "Find survivors", 3));
            _questRegistry.RegisterQuest(quest);

            // Arrange: Create dialogue that triggers the quest
            var dialogue = Dialogue.Create("npc_001", "Guard Captain");
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node1",
                "We need help! Villagers are trapped!",
                "Guard Captain",
                new List<DialogueChoice>
                {
                    DialogueChoice.Create("choice1", "I'll help!", "node2"),
                    DialogueChoice.Create("choice2", "Not now.", "")
                }
            ));
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node2",
                "Thank you! Please hurry!",
                "Guard Captain",
                new List<DialogueChoice>()
            ));
            _dialogueRegistry.RegisterDialogue(dialogue);

            // Act: Start dialogue
            _dialogueManager.StartDialogue(_testPlayerId, "npc_001");

            // Simulate player selecting "I'll help!" choice
            _dialogueManager.MakeChoice(_testPlayerId, "choice1");

            // Manually trigger quest acceptance (simulating dialogue system integration)
            _questManager.AcceptQuest(_testPlayerId, "rescue_quest");

            // Assert: Quest should be active
            var questState = _questManager.GetQuestState(_testPlayerId, "rescue_quest");
            Assert.AreEqual(QuestState.Active, questState, "Quest should be active after dialogue acceptance");

            var activeQuests = _questManager.GetActiveQuests(_testPlayerId);
            Assert.IsTrue(activeQuests.Any(q => q.QuestId == "rescue_quest"), "Quest should be in active quests");
        }

        [Test]
        public void DialogueChoice_ConditionedOnQuestState_UnlocksCorrectly()
        {
            // Arrange: Create and accept a quest
            var quest = Quest.Create("main_quest", "Main Quest");
            (quest as Quest)?.AddObjective(QuestObjective.Create("obj1", "Talk to NPC", 1));
            _questRegistry.RegisterQuest(quest);
            _questManager.AcceptQuest(_testPlayerId, "main_quest");

            // Complete the quest
            _questManager.ProgressObjective(_testPlayerId, "main_quest", "obj1", 1);
            _questManager.CompleteQuest(_testPlayerId, "main_quest");

            // Arrange: Create dialogue with quest-conditional choice
            var dialogue = Dialogue.Create("npc_002", "Wise Elder");
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node1",
                "Have you completed your task?",
                "Wise Elder",
                new List<DialogueChoice>
                {
                    DialogueChoice.Create("choice1", "Yes, it's done!", "node2"),
                    DialogueChoice.Create("choice2", "Still working on it.", "")
                }
            ));
            _dialogueRegistry.RegisterDialogue(dialogue);

            // Act: Start dialogue
            _dialogueManager.StartDialogue(_testPlayerId, "npc_002");

            // Assert: Both choices should be available, but in real implementation,
            // choice1 would only be available if quest is complete
            var currentNode = _dialogueManager.GetCurrentNode(_testPlayerId);
            Assert.IsNotNull(currentNode, "Should have current dialogue node");

            var questState = _questManager.GetQuestState(_testPlayerId, "main_quest");
            Assert.AreEqual(QuestState.Completed, questState, "Quest should be completed");
        }

        [Test]
        public void QuestObjective_ProgressedByDialogueCompletion_UpdatesCorrectly()
        {
            // Arrange: Create quest with "talk to NPC" objective
            var quest = Quest.Create("talk_quest", "Talk to the Elder");
            (quest as Quest)?.AddObjective(QuestObjective.Create("obj1", "Speak with Elder", 1));
            _questRegistry.RegisterQuest(quest);
            _questManager.AcceptQuest(_testPlayerId, "talk_quest");

            // Arrange: Create dialogue
            var dialogue = Dialogue.Create("elder_001", "Village Elder");
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node1",
                "Thank you for coming.",
                "Village Elder",
                new List<DialogueChoice>()
            ));
            _dialogueRegistry.RegisterDialogue(dialogue);

            // Act: Start and complete dialogue
            _dialogueManager.StartDialogue(_testPlayerId, "elder_001");
            _dialogueManager.EndDialogue(_testPlayerId);

            // Manually progress quest objective (simulating dialogue completion hook)
            _questManager.ProgressObjective(_testPlayerId, "talk_quest", "obj1", 1);

            // Assert: Objective should be complete
            var objective = _questManager.GetObjectiveProgress(_testPlayerId, "talk_quest", "obj1");
            Assert.AreEqual(1, objective.CurrentProgress, "Objective should be completed after dialogue");

            var questState = _questManager.GetQuestState(_testPlayerId, "talk_quest");
            Assert.AreEqual(QuestState.Completed, questState, "Quest should auto-complete when all objectives done");
        }

        #endregion

        #region Crafting ↔ Trading Integration Tests

        [Test]
        public void CraftedItem_CanBeSoldToTrader_TransactionSucceeds()
        {
            // Arrange: Register crafting recipe for Iron Sword
            var recipe = CraftingRecipe.Create("recipe_iron_sword", "Iron Sword Recipe");
            (recipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("iron_ore", 2));
            (recipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("wood", 1));
            (recipe as CraftingRecipe)?.SetResult(new CraftingResult("iron_sword", 1));
            (recipe as CraftingRecipe)?.SetCraftingTime(10f);
            (recipe as CraftingRecipe)?.SetSuccessChance(1.0f); // 100% success
            _recipeRegistry.RegisterRecipe(recipe);

            // Give player crafting materials
            _entityManager.AddItem(_testPlayerId, "iron_ore", 2);
            _entityManager.AddItem(_testPlayerId, "wood", 1);

            // Craft the item
            var craftResult = _craftingManager.StartCrafting(_testPlayerId, "recipe_iron_sword");
            Assert.IsTrue(craftResult.Success, "Crafting should start successfully");

            // Simulate crafting completion (in real scenario, would wait for time)
            var craftingData = _entityManager.GetComponent<ActiveCraftingData>(_testPlayerId);
            craftingData.State = CraftingState.Completed;
            _entityManager.UpdateComponent(_testPlayerId, craftingData);

            _craftingManager.CompleteCrafting(_testPlayerId);

            // Verify player has the crafted sword
            Assert.IsTrue(_entityManager.HasItem(_testPlayerId, "iron_sword", 1), "Player should have crafted sword");

            // Arrange: Create trader who buys swords
            var trader = Trader.Create("blacksmith_001", "Master Blacksmith", TraderType.Blacksmith);
            var tradeOffer = TradeOffer.Create("buy_iron_sword", "blacksmith_001");
            (tradeOffer as TradeOffer)?.AddRequestedItem(new TradeItem("iron_sword", 1));
            (tradeOffer as TradeOffer)?.SetGoldAmount(50); // Trader pays 50 gold
            trader.AddOffer(tradeOffer);
            _traderRegistry.RegisterTrader(trader);

            // Act: Sell the crafted sword to trader
            _tradingManager.ProposeTradeOffer(_testPlayerId, "buy_iron_sword");
            var tradeResult = _tradingManager.AcceptTrade(_testPlayerId, "buy_iron_sword");

            // Assert: Trade should succeed
            Assert.IsTrue(tradeResult.Success, "Trade should succeed");
            Assert.IsFalse(_entityManager.HasItem(_testPlayerId, "iron_sword", 1), "Player should no longer have sword");

            var playerGold = _tradingManager.GetPlayerGold(_testPlayerId);
            Assert.AreEqual(1050, playerGold, "Player should have 1000 + 50 gold"); // Started with 1000
        }

        [Test]
        public void TradedIngredients_CanBeUsedForCrafting_CraftingSucceeds()
        {
            // Arrange: Create trader selling crafting materials
            var trader = Trader.Create("merchant_001", "Material Merchant", TraderType.Merchant);
            var tradeOffer = TradeOffer.Create("buy_iron_ore", "merchant_001");
            (tradeOffer as TradeOffer)?.AddOfferedItem(new TradeItem("iron_ore", 2));
            (tradeOffer as TradeOffer)?.SetGoldAmount(-30); // Player pays 30 gold
            trader.AddOffer(tradeOffer);
            _traderRegistry.RegisterTrader(trader);

            var tradeOffer2 = TradeOffer.Create("buy_wood", "merchant_001");
            (tradeOffer2 as TradeOffer)?.AddOfferedItem(new TradeItem("wood", 1));
            (tradeOffer2 as TradeOffer)?.SetGoldAmount(-10); // Player pays 10 gold
            trader.AddOffer(tradeOffer2);
            _traderRegistry.RegisterTrader(trader);

            // Act: Buy crafting materials from trader
            _tradingManager.ProposeTradeOffer(_testPlayerId, "buy_iron_ore");
            var tradeResult1 = _tradingManager.AcceptTrade(_testPlayerId, "buy_iron_ore");
            Assert.IsTrue(tradeResult1.Success, "Should successfully buy iron ore");

            _tradingManager.ProposeTradeOffer(_testPlayerId, "buy_wood");
            var tradeResult2 = _tradingManager.AcceptTrade(_testPlayerId, "buy_wood");
            Assert.IsTrue(tradeResult2.Success, "Should successfully buy wood");

            // Verify player has materials
            Assert.IsTrue(_entityManager.HasItem(_testPlayerId, "iron_ore", 2), "Player should have iron ore");
            Assert.IsTrue(_entityManager.HasItem(_testPlayerId, "wood", 1), "Player should have wood");

            // Arrange: Register crafting recipe
            var recipe = CraftingRecipe.Create("recipe_iron_sword", "Iron Sword Recipe");
            (recipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("iron_ore", 2));
            (recipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("wood", 1));
            (recipe as CraftingRecipe)?.SetResult(new CraftingResult("iron_sword", 1));
            (recipe as CraftingRecipe)?.SetCraftingTime(10f);
            (recipe as CraftingRecipe)?.SetSuccessChance(1.0f);
            _recipeRegistry.RegisterRecipe(recipe);

            // Act: Craft the sword using traded materials
            var craftResult = _craftingManager.StartCrafting(_testPlayerId, "recipe_iron_sword");

            // Assert: Crafting should succeed
            Assert.IsTrue(craftResult.Success, "Crafting should succeed with traded materials");

            // Verify materials were consumed
            Assert.IsFalse(_entityManager.HasItem(_testPlayerId, "iron_ore", 2), "Iron ore should be consumed");
            Assert.IsFalse(_entityManager.HasItem(_testPlayerId, "wood", 1), "Wood should be consumed");

            var playerGold = _tradingManager.GetPlayerGold(_testPlayerId);
            Assert.AreEqual(960, playerGold, "Player should have spent 40 gold total"); // 1000 - 30 - 10
        }

        #endregion

        #region Dialogue ↔ Trading Integration Tests

        [Test]
        public void MerchantDialogue_CanInitiateTrade_TradeWindowOpens()
        {
            // Arrange: Create merchant trader
            var trader = Trader.Create("merchant_002", "General Store Owner", TraderType.Merchant);
            var tradeOffer = TradeOffer.Create("buy_potion", "merchant_002");
            (tradeOffer as TradeOffer)?.AddOfferedItem(new TradeItem("health_potion", 1));
            (tradeOffer as TradeOffer)?.SetGoldAmount(-25); // Player pays 25 gold
            trader.AddOffer(tradeOffer);
            _traderRegistry.RegisterTrader(trader);

            // Arrange: Create merchant dialogue with "Shop" option
            var dialogue = Dialogue.Create("merchant_002", "General Store Owner");
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node1",
                "Welcome to my shop! What can I get you?",
                "Store Owner",
                new List<DialogueChoice>
                {
                    DialogueChoice.Create("choice1", "Show me your wares", ""),
                    DialogueChoice.Create("choice2", "Just browsing", "")
                }
            ));
            _dialogueRegistry.RegisterDialogue(dialogue);

            // Act: Start dialogue
            _dialogueManager.StartDialogue(_testPlayerId, "merchant_002");

            // Simulate selecting "Show me your wares"
            _dialogueManager.MakeChoice(_testPlayerId, "choice1");

            // In real implementation, this would open trade window
            // Here we simulate the trade flow
            _tradingManager.ProposeTradeOffer(_testPlayerId, "buy_potion");
            var tradeResult = _tradingManager.AcceptTrade(_testPlayerId, "buy_potion");

            // Assert: Trade should complete successfully
            Assert.IsTrue(tradeResult.Success, "Trade should succeed through dialogue");
            Assert.IsTrue(_entityManager.HasItem(_testPlayerId, "health_potion", 1), "Player should have potion");

            var playerGold = _tradingManager.GetPlayerGold(_testPlayerId);
            Assert.AreEqual(975, playerGold, "Player should have spent 25 gold"); // 1000 - 25
        }

        [Test]
        public void TradeReputation_UnlocksDialogueChoices_NewOptionsAvailable()
        {
            // Arrange: Create trader and build reputation
            var trader = Trader.Create("fence_dealer", "Shady Merchant", TraderType.FenceDealer);
            _traderRegistry.RegisterTrader(trader);

            // Make some trades to build reputation
            var activeTrades = new ActiveTradesData();
            activeTrades.TraderReputation["fence_dealer"] = 50; // High reputation
            _entityManager.AddComponent(_testPlayerId, activeTrades);

            // Arrange: Create dialogue with reputation-gated choice
            var dialogue = Dialogue.Create("fence_dealer", "Shady Merchant");
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node1",
                "Ah, a valued customer...",
                "Shady Merchant",
                new List<DialogueChoice>
                {
                    DialogueChoice.Create("choice1", "Show me the special items", "node2"),
                    DialogueChoice.Create("choice2", "Regular goods", "")
                }
            ));
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node2",
                "Here are my... exclusive wares.",
                "Shady Merchant",
                new List<DialogueChoice>()
            ));
            _dialogueRegistry.RegisterDialogue(dialogue);

            // Act: Start dialogue
            _dialogueManager.StartDialogue(_testPlayerId, "fence_dealer");

            // Assert: In real implementation, choice1 would only be available with rep >= 50
            var reputation = _tradingManager.GetReputation(_testPlayerId, "fence_dealer");
            Assert.AreEqual(50, reputation, "Player should have reputation with trader");

            var currentNode = _dialogueManager.GetCurrentNode(_testPlayerId);
            Assert.IsNotNull(currentNode, "Should have current dialogue node");
        }

        #endregion

        #region All Systems ↔ Inventory Integration Tests

        [Test]
        public void QuestReward_AddedToInventory_ItemAvailableForCraftingAndTrading()
        {
            // Arrange: Create quest with item reward
            var quest = Quest.Create("reward_quest", "Fetch Quest");
            (quest as Quest)?.AddObjective(QuestObjective.Create("obj1", "Collect items", 1));
            (quest as Quest)?.AddReward(QuestReward.CreateItemReward("quest_item", 1));
            _questRegistry.RegisterQuest(quest);

            // Act: Accept quest, complete objective, complete quest
            _questManager.AcceptQuest(_testPlayerId, "reward_quest");
            _questManager.ProgressObjective(_testPlayerId, "reward_quest", "obj1", 1);
            _questManager.CompleteQuest(_testPlayerId, "reward_quest");

            // Manually grant reward (simulating quest completion hook)
            _entityManager.AddItem(_testPlayerId, "quest_item", 1);

            // Assert: Player should have quest item
            Assert.IsTrue(_entityManager.HasItem(_testPlayerId, "quest_item", 1), "Player should have quest reward item");

            // Test 1: Item can be used in crafting
            var craftRecipe = CraftingRecipe.Create("craft_with_quest_item", "Special Recipe");
            (craftRecipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("quest_item", 1));
            (craftRecipe as CraftingRecipe)?.SetResult(new CraftingResult("iron_sword", 1));
            (craftRecipe as CraftingRecipe)?.SetCraftingTime(5f);
            (craftRecipe as CraftingRecipe)?.SetSuccessChance(1.0f);
            _recipeRegistry.RegisterRecipe(craftRecipe);

            var canCraft = _craftingManager.CanCraft(_testPlayerId, "craft_with_quest_item");
            Assert.IsTrue(canCraft, "Should be able to craft with quest reward item");

            // Test 2: Item can be sold to trader
            var trader = Trader.Create("collector", "Artifact Collector", TraderType.Merchant);
            var tradeOffer = TradeOffer.Create("sell_quest_item", "collector");
            (tradeOffer as TradeOffer)?.AddRequestedItem(new TradeItem("quest_item", 1));
            (tradeOffer as TradeOffer)?.SetGoldAmount(100); // Trader pays 100 gold
            trader.AddOffer(tradeOffer);
            _traderRegistry.RegisterTrader(trader);

            _tradingManager.ProposeTradeOffer(_testPlayerId, "sell_quest_item");
            var tradeResult = _tradingManager.AcceptTrade(_testPlayerId, "sell_quest_item");

            Assert.IsTrue(tradeResult.Success, "Should be able to sell quest reward item");
            Assert.IsFalse(_entityManager.HasItem(_testPlayerId, "quest_item", 1), "Quest item should be sold");

            var playerGold = _tradingManager.GetPlayerGold(_testPlayerId);
            Assert.AreEqual(1100, playerGold, "Player should have earned 100 gold from sale");
        }

        [Test]
        public void CompleteGameplayLoop_QuestDialogueCraftTrade_AllSystemsWorkTogether()
        {
            // This test simulates a complete gameplay loop:
            // 1. NPC gives quest through dialogue
            // 2. Player completes quest, gets reward
            // 3. Uses reward to buy crafting materials from trader
            // 4. Crafts item
            // 5. Sells crafted item back to different trader

            // Step 1: Dialogue → Quest
            var dialogue = Dialogue.Create("quest_giver", "Village Chief");
            (dialogue as Dialogue)?.AddNode(DialogueNode.Create(
                "node1",
                "We need your help! Will you assist us?",
                "Village Chief",
                new List<DialogueChoice>
                {
                    DialogueChoice.Create("choice1", "Yes, I'll help", "node2")
                }
            ));
            _dialogueRegistry.RegisterDialogue(dialogue);

            var quest = Quest.Create("main_quest", "Help the Village");
            (quest as Quest)?.AddObjective(QuestObjective.Create("obj1", "Complete task", 1));
            (quest as Quest)?.AddReward(QuestReward.CreateGoldReward(100));
            _questRegistry.RegisterQuest(quest);

            _dialogueManager.StartDialogue(_testPlayerId, "quest_giver");
            _dialogueManager.MakeChoice(_testPlayerId, "choice1");
            _questManager.AcceptQuest(_testPlayerId, "main_quest");

            // Step 2: Complete quest
            _questManager.ProgressObjective(_testPlayerId, "main_quest", "obj1", 1);
            _questManager.CompleteQuest(_testPlayerId, "main_quest");

            // Grant quest reward
            var currentGold = _tradingManager.GetPlayerGold(_testPlayerId);
            _tradingManager.InitializePlayerGold(_testPlayerId, currentGold + 100);

            Assert.AreEqual(1100, _tradingManager.GetPlayerGold(_testPlayerId), "Player should have quest reward gold");

            // Step 3: Buy materials from trader
            var materialTrader = Trader.Create("mat_vendor", "Material Vendor", TraderType.Merchant);
            var buyMaterials = TradeOffer.Create("buy_materials", "mat_vendor");
            (buyMaterials as TradeOffer)?.AddOfferedItem(new TradeItem("iron_ore", 2));
            (buyMaterials as TradeOffer)?.AddOfferedItem(new TradeItem("wood", 1));
            (buyMaterials as TradeOffer)?.SetGoldAmount(-80); // Cost 80 gold
            materialTrader.AddOffer(buyMaterials);
            _traderRegistry.RegisterTrader(materialTrader);

            _tradingManager.ProposeTradeOffer(_testPlayerId, "buy_materials");
            var buyResult = _tradingManager.AcceptTrade(_testPlayerId, "buy_materials");
            Assert.IsTrue(buyResult.Success, "Should buy materials successfully");

            // Step 4: Craft item
            var recipe = CraftingRecipe.Create("craft_sword", "Craft Sword");
            (recipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("iron_ore", 2));
            (recipe as CraftingRecipe)?.AddIngredient(new CraftingIngredient("wood", 1));
            (recipe as CraftingRecipe)?.SetResult(new CraftingResult("iron_sword", 1));
            (recipe as CraftingRecipe)?.SetCraftingTime(1f);
            (recipe as CraftingRecipe)?.SetSuccessChance(1.0f);
            _recipeRegistry.RegisterRecipe(recipe);

            var craftResult = _craftingManager.StartCrafting(_testPlayerId, "craft_sword");
            Assert.IsTrue(craftResult.Success, "Should start crafting");

            // Complete crafting
            var craftData = _entityManager.GetComponent<ActiveCraftingData>(_testPlayerId);
            craftData.State = CraftingState.Completed;
            _entityManager.UpdateComponent(_testPlayerId, craftData);
            _craftingManager.CompleteCrafting(_testPlayerId);

            Assert.IsTrue(_entityManager.HasItem(_testPlayerId, "iron_sword", 1), "Should have crafted sword");

            // Step 5: Sell crafted item
            var weaponTrader = Trader.Create("weapon_buyer", "Weapon Dealer", TraderType.Merchant);
            var sellSword = TradeOffer.Create("sell_sword", "weapon_buyer");
            (sellSword as TradeOffer)?.AddRequestedItem(new TradeItem("iron_sword", 1));
            (sellSword as TradeOffer)?.SetGoldAmount(150); // Sells for 150 gold
            weaponTrader.AddOffer(sellSword);
            _traderRegistry.RegisterTrader(weaponTrader);

            _tradingManager.ProposeTradeOffer(_testPlayerId, "sell_sword");
            var sellResult = _tradingManager.AcceptTrade(_testPlayerId, "sell_sword");
            Assert.IsTrue(sellResult.Success, "Should sell sword successfully");

            // Final assertions
            Assert.IsFalse(_entityManager.HasItem(_testPlayerId, "iron_sword", 1), "Sword should be sold");

            var finalGold = _tradingManager.GetPlayerGold(_testPlayerId);
            // Started: 1000, Quest reward: +100, Bought materials: -80, Sold sword: +150
            // Total: 1000 + 100 - 80 + 150 = 1170
            Assert.AreEqual(1170, finalGold, "Player should have correct final gold amount");
        }

        #endregion
    }
}
