using RPGFramework.Trading;

namespace MythHunter.GameLogic.Trading
{
    /// <summary>
    /// Sample traders for MythHunter with pre-configured offers.
    /// Demonstrates all trader types and trading mechanics.
    /// </summary>
    public static class SampleTraders
    {
        /// <summary>
        /// Create General Merchant - buys and sells common items.
        /// </summary>
        public static Trader CreateGeneralMerchant()
        {
            var trader = Trader.Create("general_merchant", "General Merchant", TraderType.Merchant);
            trader.SetReputationGain(5);

            // Offer 1: Buy potions
            var buyPotions = TradeOffer.Create("general_offer_buy_potions", "general_merchant");
            (buyPotions as TradeOffer)?.AddOfferedItem(new TradeItem("item_gold_coin", 50, 0));
            (buyPotions as TradeOffer)?.AddRequestedItem(new TradeItem("item_minor_healing_potion", 5, 0));
            (buyPotions as TradeOffer)?.SetGoldAmount(0);
            trader.AddOffer(buyPotions);

            // Offer 2: Sell basic sword
            var sellSword = TradeOffer.Create("general_offer_sell_sword", "general_merchant");
            (sellSword as TradeOffer)?.AddOfferedItem(new TradeItem("item_iron_sword", 1, 0));
            (sellSword as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 200, 0));
            (sellSword as TradeOffer)?.SetGoldAmount(-200);
            trader.AddOffer(sellSword);

            // Offer 3: Bulk trade
            var bulkTrade = TradeOffer.Create("general_offer_bulk_supplies", "general_merchant");
            (bulkTrade as TradeOffer)?.AddOfferedItem(new TradeItem("item_iron_ingot", 10, 0));
            (bulkTrade as TradeOffer)?.AddOfferedItem(new TradeItem("item_leather_scrap", 5, 0));
            (bulkTrade as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 500, 0));
            (bulkTrade as TradeOffer)?.SetGoldAmount(-500);
            trader.AddOffer(bulkTrade);

            return trader;
        }

        /// <summary>
        /// Create Master Blacksmith - sells weapons and armor.
        /// Requires high reputation.
        /// </summary>
        public static Trader CreateMasterBlacksmith()
        {
            var trader = new Trader("master_blacksmith", "Master Blacksmith", TraderType.Blacksmith,
                priceModifier: 1.0f, minimumReputation: 50);
            trader.SetReputationGain(15);

            // Offer 1: Legendary sword (reputation locked)
            var legendarySword = TradeOffer.Create("blacksmith_offer_legendary_sword", "master_blacksmith");
            (legendarySword as TradeOffer)?.AddOfferedItem(new TradeItem("item_legendary_sword", 1, 3));
            (legendarySword as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 1000, 0));
            (legendarySword as TradeOffer)?.AddRequestedItem(new TradeItem("item_dragon_scale", 2, 0));
            (legendarySword as TradeOffer)?.AddCondition(TradeConditionData.RequireReputation("master_blacksmith", 75));
            (legendarySword as TradeOffer)?.SetGoldAmount(-1000);
            (legendarySword as TradeOffer)?.SetMaxUses(1);
            trader.AddOffer(legendarySword);

            // Offer 2: Quality armor
            var qualityArmor = TradeOffer.Create("blacksmith_offer_quality_armor", "master_blacksmith");
            (qualityArmor as TradeOffer)?.AddOfferedItem(new TradeItem("item_steel_chest_plate", 1, 2));
            (qualityArmor as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 500, 0));
            (qualityArmor as TradeOffer)?.SetGoldAmount(-500);
            trader.AddOffer(qualityArmor);

            // Offer 3: Weapon repair (unlimited use)
            var weaponRepair = TradeOffer.Create("blacksmith_offer_repair", "master_blacksmith");
            (weaponRepair as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 100, 0));
            (weaponRepair as TradeOffer)?.SetGoldAmount(-100);
            (weaponRepair as TradeOffer)?.SetMaxUses(0); // Unlimited
            trader.AddOffer(weaponRepair);

            return trader;
        }

        /// <summary>
        /// Create Alchemist Shop - sells potions and ingredients.
        /// </summary>
        public static Trader CreateAlchemistShop()
        {
            var trader = new Trader("alchemist_shop", "Alchemist's Emporium", TraderType.Alchemist,
                priceModifier: 1.0f, minimumReputation: 0);
            trader.SetReputationGain(10);

            // Offer 1: Healing potion pack
            var healingPotions = TradeOffer.Create("alchemist_offer_healing_pack", "alchemist_shop");
            (healingPotions as TradeOffer)?.AddOfferedItem(new TradeItem("item_major_healing_potion", 5, 0));
            (healingPotions as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 250, 0));
            (healingPotions as TradeOffer)?.SetGoldAmount(-250);
            trader.AddOffer(healingPotions);

            // Offer 2: Rare ingredients
            var rareIngredients = TradeOffer.Create("alchemist_offer_rare_ingredients", "alchemist_shop");
            (rareIngredients as TradeOffer)?.AddOfferedItem(new TradeItem("item_moonflower_essence", 3, 0));
            (rareIngredients as TradeOffer)?.AddOfferedItem(new TradeItem("item_dragon_blood", 1, 0));
            (rareIngredients as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 400, 0));
            (rareIngredients as TradeOffer)?.SetGoldAmount(-400);
            trader.AddOffer(rareIngredients);

            // Offer 3: Buy ingredients
            var buyIngredients = TradeOffer.Create("alchemist_offer_buy_ingredients", "alchemist_shop");
            (buyIngredients as TradeOffer)?.AddOfferedItem(new TradeItem("item_gold_coin", 100, 0));
            (buyIngredients as TradeOffer)?.AddRequestedItem(new TradeItem("item_herb_bundle", 10, 0));
            (buyIngredients as TradeOffer)?.SetGoldAmount(0);
            trader.AddOffer(buyIngredients);

            return trader;
        }

        /// <summary>
        /// Create Shady Fence Dealer - sells stolen goods.
        /// No reputation requirement, illegal items.
        /// </summary>
        public static Trader CreateShadyFenceDealer()
        {
            var trader = new Trader("shady_fence", "Shady Fence Dealer", TraderType.FenceDealer,
                priceModifier: 1.0f, minimumReputation: 0);
            trader.SetReputationGain(8);

            // Offer 1: Stolen artifacts
            var stolenArtifacts = TradeOffer.Create("fence_offer_stolen_artifacts", "shady_fence");
            (stolenArtifacts as TradeOffer)?.AddOfferedItem(new TradeItem("item_stolen_necklace", 1, 0));
            (stolenArtifacts as TradeOffer)?.AddOfferedItem(new TradeItem("item_stolen_ring", 2, 0));
            (stolenArtifacts as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 150, 0));
            (stolenArtifacts as TradeOffer)?.SetGoldAmount(-150);
            trader.AddOffer(stolenArtifacts);

            // Offer 2: Black market weapons
            var blackMarketWeapons = TradeOffer.Create("fence_offer_black_weapons", "shady_fence");
            (blackMarketWeapons as TradeOffer)?.AddOfferedItem(new TradeItem("item_poison_dagger", 1, 1));
            (blackMarketWeapons as TradeOffer)?.AddRequestedItem(new TradeItem("item_gold_coin", 300, 0));
            (blackMarketWeapons as TradeOffer)?.SetGoldAmount(-300);
            trader.AddOffer(blackMarketWeapons);

            // Offer 3: Sell anything (no questions asked)
            var buyAnything = TradeOffer.Create("fence_offer_buy_anything", "shady_fence");
            (buyAnything as TradeOffer)?.AddOfferedItem(new TradeItem("item_gold_coin", 50, 0));
            (buyAnything as TradeOffer)?.AddRequestedItem(new TradeItem("item_misc_random", 1, 0));
            (buyAnything as TradeOffer)?.SetGoldAmount(0);
            (buyAnything as TradeOffer)?.SetMaxUses(0); // Unlimited
            trader.AddOffer(buyAnything);

            return trader;
        }

        /// <summary>
        /// Create Quest Reward Trader - provides quest-locked special items.
        /// </summary>
        public static Trader CreateQuestRewardTrader()
        {
            var trader = new Trader("quest_reward_trader", "Quartermaster", TraderType.QuestGiver,
                priceModifier: 1.0f, minimumReputation: 0);
            trader.SetReputationGain(20);

            // Offer 1: Epic reward for quest completion
            var epicQuestReward = TradeOffer.Create("quest_offer_epic_reward", "quest_reward_trader");
            (epicQuestReward as TradeOffer)?.AddOfferedItem(new TradeItem("item_epic_chest_armor", 1, 3));
            (epicQuestReward as TradeOffer)?.AddCondition(TradeConditionData.RequireQuestComplete("main_quest_dragon_slayer"));
            (epicQuestReward as TradeOffer)?.SetGoldAmount(500); // Reward gold too
            (epicQuestReward as TradeOffer)?.SetMaxUses(1);
            trader.AddOffer(epicQuestReward);

            // Offer 2: Level-locked reward
            var levelLockedReward = TradeOffer.Create("quest_offer_level_reward", "quest_reward_trader");
            (levelLockedReward as TradeOffer)?.AddOfferedItem(new TradeItem("item_hero_ring", 1, 2));
            (levelLockedReward as TradeOffer)?.AddCondition(TradeConditionData.RequirePlayerLevel(30));
            (levelLockedReward as TradeOffer)?.SetGoldAmount(250);
            (levelLockedReward as TradeOffer)?.SetMaxUses(1);
            trader.AddOffer(levelLockedReward);

            // Offer 3: Unlimited quest item exchange
            var questItemExchange = TradeOffer.Create("quest_offer_item_exchange", "quest_reward_trader");
            (questItemExchange as TradeOffer)?.AddOfferedItem(new TradeItem("item_gold_coin", 100, 0));
            (questItemExchange as TradeOffer)?.AddRequestedItem(new TradeItem("item_quest_token", 5, 0));
            (questItemExchange as TradeOffer)?.SetGoldAmount(0);
            (questItemExchange as TradeOffer)?.SetMaxUses(0); // Unlimited
            trader.AddOffer(questItemExchange);

            return trader;
        }

        /// <summary>
        /// Register all sample traders to a registry.
        /// </summary>
        public static void RegisterAllSampleTraders(ITraderRegistry registry)
        {
            registry.RegisterTrader(CreateGeneralMerchant());
            registry.RegisterTrader(CreateMasterBlacksmith());
            registry.RegisterTrader(CreateAlchemistShop());
            registry.RegisterTrader(CreateShadyFenceDealer());
            registry.RegisterTrader(CreateQuestRewardTrader());
        }

        /// <summary>
        /// Register all sample traders with metadata.
        /// </summary>
        public static void RegisterAllSampleTradersWithMetadata(TradingSystemIntegration integration)
        {
            // General Merchant
            var merchantMeta = TraderMetadata.CreateMerchant("General Merchant",
                "UI/Portraits/Merchants/GeneralMerchant", "general_merchant_dialogue");
            integration.RegisterTrader(CreateGeneralMerchant(), merchantMeta);

            // Master Blacksmith
            var blacksmithMeta = TraderMetadata.CreateBlacksmith("Master Blacksmith",
                "UI/Portraits/Traders/MasterBlacksmith", "master_blacksmith_dialogue");
            integration.RegisterTrader(CreateMasterBlacksmith(), blacksmithMeta);

            // Alchemist
            var alchemistMeta = TraderMetadata.CreateAlchemist("Alchemist",
                "UI/Portraits/Traders/Alchemist", "alchemist_dialogue");
            integration.RegisterTrader(CreateAlchemistShop(), alchemistMeta);

            // Fence Dealer
            var fenceMeta = TraderMetadata.CreateFenceDealer("Shady Fence Dealer",
                "UI/Portraits/Traders/ShadyFence", "fence_dealer_dialogue");
            integration.RegisterTrader(CreateShadyFenceDealer(), fenceMeta);

            // Quest Reward Trader
            var questMeta = TraderMetadata.CreateMerchant("Quartermaster",
                "UI/Portraits/Traders/Quartermaster", "quartermaster_dialogue");
            integration.RegisterTrader(CreateQuestRewardTrader(), questMeta);
        }
    }
}
