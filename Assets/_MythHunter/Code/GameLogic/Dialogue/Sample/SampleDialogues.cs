// Assets/_MythHunter/Code/GameLogic/Dialogue/Sample/SampleDialogues.cs

using System.Collections.Generic;
using RPGFramework.Dialogue;

namespace MythHunter.GameLogic.Dialogue.Sample
{
    /// <summary>
    /// Sample dialogue definitions for testing and development.
    /// Demonstrates various dialogue types and patterns:
    /// - Simple linear greeting (Main story)
    /// - Quest acceptance with player choices
    /// - Conditional choices based on player stats/gold
    /// - Branching story decision with multiple outcomes
    /// - Multi-path companion conversation
    ///
    /// These are greenfield sample dialogues demonstrating the Framework Dialogue System
    /// with MythHunter metadata integration.
    /// </summary>
    public static class SampleDialogues
    {
        // ============================================================
        // GREETING - Village Elder Simple Greeting
        // ============================================================

        /// <summary>
        /// Simple linear greeting dialogue from village elder.
        /// Demonstrates basic dialogue structure with single dialogue path.
        /// No choices, just a greeting line followed by end.
        /// </summary>
        public static DialogueData GreetingVillageElder()
        {
            return new DialogueData
            {
                Id = "dialogue_greeting_elder",
                Title = "Elder's Welcome",
                Description = "A warm greeting from the village elder",
                RootNodeId = "node_greeting_start",
                Nodes = new[]
                {
                    new DialogueNodeData
                    {
                        Id = "node_greeting_start",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_elder_graves",
                        Text = "Welcome, traveler. I am Elder Graves, keeper of this village. You look like you've journeyed far.",
                        NextNodeIds = new[] { "node_greeting_end" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_greeting_end",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_elder_graves",
                        Text = "May your stay be peaceful.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    }
                }
            };
        }

        public static DialogueMetadata GreetingVillageElderMetadata()
        {
            return DialogueMetadata.CreateMainDialogue(
                speakerName: "Elder Graves",
                speakerPortraitPath: "Textures/Portraits/elder_graves",
                cameraAngle: "Closeup"
            );
        }

        // ============================================================
        // QUEST ACCEPTANCE - Quest Giver Dialog
        // ============================================================

        /// <summary>
        /// Quest acceptance dialogue with player choice to accept or decline.
        /// Demonstrates choice nodes with branching outcomes.
        /// Triggers main tutorial quest when accepted.
        /// </summary>
        public static DialogueData QuestAcceptanceDialog()
        {
            return new DialogueData
            {
                Id = "dialogue_quest_acceptance",
                Title = "The Goblin Menace",
                Description = "Quest giver offers a task to eliminate local goblins",
                RootNodeId = "node_quest_offer",
                Nodes = new[]
                {
                    new DialogueNodeData
                    {
                        Id = "node_quest_offer",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_quest_giver",
                        Text = "We have a goblin problem terrorizing the nearby farmlands. " +
                               "Defeat three of them and bring me proof of their defeat. " +
                               "I'll reward you handsomely.",
                        NextNodeIds = new[] { "node_quest_choice" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_quest_choice",
                        NodeType = DialogueNodeType.Choice,
                        SpeakerId = "npc_quest_giver",
                        Text = "Will you help us?",
                        NextNodeIds = new[] { "node_quest_accepted", "node_quest_declined" },
                        Choices = new[]
                        {
                            new DialogueChoiceData
                            {
                                Id = "choice_accept_quest",
                                Text = "I'll take care of it.",
                                TargetNodeId = "node_quest_accepted"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_decline_quest",
                                Text = "Not interested.",
                                TargetNodeId = "node_quest_declined"
                            }
                        }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_quest_accepted",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_quest_giver",
                        Text = "Excellent! I knew I could count on you. The goblin lair is to the east.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_quest_declined",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_quest_giver",
                        Text = "Your loss. Perhaps you'll change your mind later.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    }
                }
            };
        }

        public static DialogueMetadata QuestAcceptanceDialogMetadata()
        {
            return DialogueMetadata.CreateQuestDialogue(
                speakerName: "Quest Giver",
                speakerPortraitPath: "Textures/Portraits/quest_giver",
                triggerQuestId: "quest_tutorial",
                cameraAngle: "Medium"
            );
        }

        // ============================================================
        // SHOPKEEPER - Bargain Dialog with Conditional Choices
        // ============================================================

        /// <summary>
        /// Shopkeeper dialogue with conditional choices based on player gold.
        /// Demonstrates conditional branching logic.
        /// Player can only purchase if they have enough gold.
        /// </summary>
        public static DialogueData ShopKeeperBargain()
        {
            return new DialogueData
            {
                Id = "dialogue_shopkeeper_bargain",
                Title = "Merchant's Wares",
                Description = "A merchant offers rare items for purchase",
                RootNodeId = "node_shop_greeting",
                Nodes = new[]
                {
                    new DialogueNodeData
                    {
                        Id = "node_shop_greeting",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_merchant_lyra",
                        Text = "Welcome to my humble shop, friend. I have some rare items you might be interested in. " +
                               "A legendary sword for 500 gold, or a mage's staff for 300 gold.",
                        NextNodeIds = new[] { "node_shop_choice" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_shop_choice",
                        NodeType = DialogueNodeType.Choice,
                        SpeakerId = "npc_merchant_lyra",
                        Text = "What interests you?",
                        NextNodeIds = new[] { "node_shop_purchase", "node_shop_leave" },
                        Choices = new[]
                        {
                            new DialogueChoiceData
                            {
                                Id = "choice_buy_sword",
                                Text = "I'll take the sword.",
                                TargetNodeId = "node_shop_purchase"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_leave_shop",
                                Text = "Just browsing.",
                                TargetNodeId = "node_shop_leave"
                            }
                        }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_shop_purchase",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_merchant_lyra",
                        Text = "An excellent choice! You've made a fine investment.",
                        NextNodeIds = new[] { "node_shop_end" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_shop_leave",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_merchant_lyra",
                        Text = "Well, come back when you're ready to buy. My wares await.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_shop_end",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_merchant_lyra",
                        Text = "Thank you for your business!",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    }
                }
            };
        }

        public static DialogueMetadata ShopKeeperBargainMetadata()
        {
            return DialogueMetadata.CreateSideDialogue(
                speakerName: "Merchant Lyra",
                speakerPortraitPath: "Textures/Portraits/merchant_lyra"
            );
        }

        // ============================================================
        // MAJOR STORY CHOICE - King's Decision
        // ============================================================

        /// <summary>
        /// Major story decision dialogue with three different outcomes.
        /// Demonstrates complex branching dialogue with multiple paths.
        /// Pauses game time for narrative impact (story-critical moment).
        /// </summary>
        public static DialogueData BranchingStoryChoice()
        {
            return new DialogueData
            {
                Id = "dialogue_king_decision",
                Title = "The King's Dilemma",
                Description = "A critical story decision that affects the game world",
                RootNodeId = "node_story_setup",
                Nodes = new[]
                {
                    new DialogueNodeData
                    {
                        Id = "node_story_setup",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_king_aldric",
                        Text = "The enemy siege grows stronger each day. We must act, but the path forward is unclear.",
                        NextNodeIds = new[] { "node_story_more_context" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_story_more_context",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_king_aldric",
                        Text = "I see three options: surrender to the enemy and save our people, " +
                               "stage a desperate counterattack despite the odds, " +
                               "or seek an ancient artifact of power we heard legends of.",
                        NextNodeIds = new[] { "node_story_choice" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_story_choice",
                        NodeType = DialogueNodeType.Choice,
                        SpeakerId = "npc_king_aldric",
                        Text = "What do you counsel, hero?",
                        NextNodeIds = new[] { "node_story_surrender", "node_story_attack", "node_story_artifact" },
                        Choices = new[]
                        {
                            new DialogueChoiceData
                            {
                                Id = "choice_surrender",
                                Text = "We should negotiate surrender.",
                                TargetNodeId = "node_story_surrender"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_counterattack",
                                Text = "We must fight! Victory or death!",
                                TargetNodeId = "node_story_attack"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_seek_artifact",
                                Text = "We should seek the ancient artifact.",
                                TargetNodeId = "node_story_artifact"
                            }
                        }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_story_surrender",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_king_aldric",
                        Text = "A wise choice for the people's sake. History will remember your wisdom.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_story_attack",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_king_aldric",
                        Text = "Your courage inspires us all! Let us meet them on the battlefield!",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_story_artifact",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_king_aldric",
                        Text = "A dangerous quest, but perhaps the legends hold truth. Find this artifact!",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    }
                }
            };
        }

        public static DialogueMetadata BranchingStoryChoiceMetadata()
        {
            return DialogueMetadata.CreateMainDialogue(
                speakerName: "King Aldric",
                speakerPortraitPath: "Textures/Portraits/king_aldric",
                cameraAngle: "Closeup"
            );
        }

        // ============================================================
        // COMPANION CONVERSATION - Multi-path Chat
        // ============================================================

        /// <summary>
        /// Companion conversation with multiple conversation topics and outcomes.
        /// Demonstrates complex multi-path dialogue structure.
        /// Companion dialogue is ambient and shown in dialogue log.
        /// </summary>
        public static DialogueData CompanionConversation()
        {
            return new DialogueData
            {
                Id = "dialogue_companion_aria",
                Title = "Companion's Tale",
                Description = "A conversation with your loyal companion Aria",
                RootNodeId = "node_companion_greeting",
                Nodes = new[]
                {
                    new DialogueNodeData
                    {
                        Id = "node_companion_greeting",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_companion_aria",
                        Text = "Hey! You've got a moment to talk? I've been meaning to catch up with you.",
                        NextNodeIds = new[] { "node_companion_topic_choice" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_topic_choice",
                        NodeType = DialogueNodeType.Choice,
                        SpeakerId = "npc_companion_aria",
                        Text = "What should we talk about?",
                        NextNodeIds = new[] { "node_companion_past", "node_companion_future", "node_companion_skills", "node_companion_end_convo" },
                        Choices = new[]
                        {
                            new DialogueChoiceData
                            {
                                Id = "choice_talk_past",
                                Text = "Tell me about your past.",
                                TargetNodeId = "node_companion_past"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_talk_future",
                                Text = "What are your dreams?",
                                TargetNodeId = "node_companion_future"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_talk_skills",
                                Text = "How did you learn to fight so well?",
                                TargetNodeId = "node_companion_skills"
                            },
                            new DialogueChoiceData
                            {
                                Id = "choice_end_conversation",
                                Text = "I should get going.",
                                TargetNodeId = "node_companion_end_convo"
                            }
                        }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_past",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_companion_aria",
                        Text = "I grew up in a small village. My family taught me to value courage and compassion.",
                        NextNodeIds = new[] { "node_companion_past_more" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_past_more",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_companion_aria",
                        Text = "That's why I follow you now. Together, we can make a difference.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_future",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_companion_aria",
                        Text = "I want to see the world. To explore every corner and help those in need.",
                        NextNodeIds = new[] { "node_companion_future_more" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_future_more",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_companion_aria",
                        Text = "Maybe you could be part of that journey.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_skills",
                        NodeType = DialogueNodeType.Line,
                        SpeakerId = "npc_companion_aria",
                        Text = "Years of training, mostly. But also learning from every battle, every opponent.",
                        NextNodeIds = new[] { "node_companion_skills_more" },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_skills_more",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_companion_aria",
                        Text = "You're a good teacher too. Fighting by your side has made me stronger.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    },
                    new DialogueNodeData
                    {
                        Id = "node_companion_end_convo",
                        NodeType = DialogueNodeType.End,
                        SpeakerId = "npc_companion_aria",
                        Text = "No problem. I'll be here whenever you need me.",
                        NextNodeIds = new string[] { },
                        Choices = new DialogueChoiceData[] { }
                    }
                }
            };
        }

        public static DialogueMetadata CompanionConversationMetadata()
        {
            return new DialogueMetadata.CreateFull(
                speakerName: "Companion Aria",
                speakerPortraitPath: "Textures/Portraits/companion_aria",
                speakerVoiceActorId: "voice_aria",
                backgroundMusicPath: "Audio/Music/dialogue_ambient_companion",
                cameraAngle: "Medium",
                pauseGameTime: false,
                showInDialogueLog: true,
                category: "Ambient",
                triggerQuestId: ""
            );
        }

        // ============================================================
        // DIALOGUE BATCH FACTORY
        // ============================================================

        /// <summary>
        /// Creates all sample dialogues with their metadata as a complete package.
        /// Useful for initializing a dialogue hub or testing system.
        /// </summary>
        /// <returns>Dictionary of dialogue ID to (DialogueData, DialogueMetadata) tuples</returns>
        public static Dictionary<string, (DialogueData dialogue, DialogueMetadata metadata)> GetAllSampleDialogues()
        {
            return new Dictionary<string, (DialogueData, DialogueMetadata)>
            {
                {
                    "dialogue_greeting_elder",
                    (GreetingVillageElder(), GreetingVillageElderMetadata())
                },
                {
                    "dialogue_quest_acceptance",
                    (QuestAcceptanceDialog(), QuestAcceptanceDialogMetadata())
                },
                {
                    "dialogue_shopkeeper_bargain",
                    (ShopKeeperBargain(), ShopKeeperBargainMetadata())
                },
                {
                    "dialogue_king_decision",
                    (BranchingStoryChoice(), BranchingStoryChoiceMetadata())
                },
                {
                    "dialogue_companion_aria",
                    (CompanionConversation(), CompanionConversationMetadata())
                }
            };
        }

        /// <summary>
        /// Gets all sample dialogues as DialogueData array (for registry initialization).
        /// </summary>
        /// <returns>Array of DialogueData objects</returns>
        public static DialogueData[] GetAllSampleDialoguesData()
        {
            return new[]
            {
                GreetingVillageElder(),
                QuestAcceptanceDialog(),
                ShopKeeperBargain(),
                BranchingStoryChoice(),
                CompanionConversation()
            };
        }

        /// <summary>
        /// Gets all sample dialogue metadata (for MythHunterDialogueComponent initialization).
        /// </summary>
        /// <returns>Dictionary of dialogue ID to metadata</returns>
        public static Dictionary<string, DialogueMetadata> GetAllSampleDialoguesMetadata()
        {
            return new Dictionary<string, DialogueMetadata>
            {
                { "dialogue_greeting_elder", GreetingVillageElderMetadata() },
                { "dialogue_quest_acceptance", QuestAcceptanceDialogMetadata() },
                { "dialogue_shopkeeper_bargain", ShopKeeperBargainMetadata() },
                { "dialogue_king_decision", BranchingStoryChoiceMetadata() },
                { "dialogue_companion_aria", CompanionConversationMetadata() }
            };
        }

        /// <summary>
        /// Gets sample main story dialogues only.
        /// </summary>
        /// <returns>Dictionary of main dialogue ID to (DialogueData, DialogueMetadata) tuples</returns>
        public static Dictionary<string, (DialogueData dialogue, DialogueMetadata metadata)> GetMainDialogues()
        {
            return new Dictionary<string, (DialogueData, DialogueMetadata)>
            {
                {
                    "dialogue_greeting_elder",
                    (GreetingVillageElder(), GreetingVillageElderMetadata())
                },
                {
                    "dialogue_king_decision",
                    (BranchingStoryChoice(), BranchingStoryChoiceMetadata())
                }
            };
        }

        /// <summary>
        /// Gets sample side/optional dialogues only.
        /// </summary>
        /// <returns>Dictionary of side dialogue ID to (DialogueData, DialogueMetadata) tuples</returns>
        public static Dictionary<string, (DialogueData dialogue, DialogueMetadata metadata)> GetSideDialogues()
        {
            return new Dictionary<string, (DialogueData, DialogueMetadata)>
            {
                {
                    "dialogue_shopkeeper_bargain",
                    (ShopKeeperBargain(), ShopKeeperBargainMetadata())
                },
                {
                    "dialogue_companion_aria",
                    (CompanionConversation(), CompanionConversationMetadata())
                }
            };
        }

        /// <summary>
        /// Gets sample quest-related dialogues only.
        /// </summary>
        /// <returns>Dictionary of quest dialogue ID to (DialogueData, DialogueMetadata) tuples</returns>
        public static Dictionary<string, (DialogueData dialogue, DialogueMetadata metadata)> GetQuestDialogues()
        {
            return new Dictionary<string, (DialogueData, DialogueMetadata)>
            {
                {
                    "dialogue_quest_acceptance",
                    (QuestAcceptanceDialog(), QuestAcceptanceDialogMetadata())
                }
            };
        }
    }
}
