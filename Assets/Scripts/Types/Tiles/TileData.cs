using System;
using System.Collections.Generic;
using System.Linq;
using Cards;
using Cards.CardEvents;
using Entities;
using Grid;
using StateManager;
using Types.Passives;
using UnityEngine;
using UnityEngine.Rendering;
using Util;

namespace Types.Tiles
{
    public class TileData
    {
        public static readonly Dictionary<string, TileEntry> tiles = new Dictionary<string, TileEntry>()
        {
            //Look at the top 3 cards of the draw pile. Pick one to draw.
            ["Foresight"] = new (
                "Foresight",
                "<b><u>On enter:</u></b> Look at the top 3 random cards in your draw pile. Draw one.",
                TileSetEnum.Purple,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) => new List<AbstractCardEvent>
                    {
                        new SeeCardsThenPickCardEvent(3)
                    }
                },
                icon: "foresight",
                pairedTilesEntry: new PairedTilesEntry(TileSetEnum.Purple, 2, null, null, null, "Draw a card.")),

            ["Stumble"] = new (
                "Stumble",
                "<b><u>On enter:</u></b> Once per turn gain 2 <sprite name=\"footsteps\">, move in a random direction.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) => new List<AbstractCardEvent>
                    {
                        new GainStepsCardEvent(2),
                        new RandomMoveCardEvent(1, context.GetRandom("randomdir"))
                    }
                },
                icon: "stumble", triggerLimit: TileTriggerLimit.OncePerTurn),

            ["Refund"] = new (
                "Refund",
                "<b><u>Once per turn:</u></b> When you play a card costing 2+ <sprite name=\"energyicon\"> here, gain 1 <sprite name=\"energyicon\">.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) =>
                {
                    if (card.Cost >= 2)
                    {
                        e.Add(new GainEnergyCardEvent(1));
                        PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                    }

                    return e;
                },
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> {},
                icon: "refund",
                triggerLimit: TileTriggerLimit.OncePerTurn,
                shouldMarkAsTriggered: (e, card, context) => { return card.Cost >= 3; }),

            ["Stride"] = new (
                "Stride",
                "<b><u>On enter:</u></b> Draw cards equal to tiles moved this turn.",
                TileSetEnum.Purple,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) => new List<AbstractCardEvent>
                    {
                        new DrawCardEvent(BattleStats.TilesMovedThisTurn + 1)
                    }
                },
                icon: "stride"),

            ["Cornered"] = new (
                "Cornered",
                "<b><u>On enter:</u></b> Gain 5 <sprite name=\"shield\"> for each adjacent enemy.",
                TileSetEnum.Blue,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) =>
                    {
                        PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                        int numberOfAdjacentEnemies = 0;


                        foreach (Vector2Int adjacentHex in HexGridManager.AdjacentHexes(playingState.player.positionRowCol))
                        {
                            if (playingState.EntitiesOnHex(adjacentHex, out var list))
                            {
                                numberOfAdjacentEnemies += 1;
                            }
                        }

                        return new List<AbstractCardEvent>
                        {
                            new ShieldCardEvent(numberOfAdjacentEnemies * 5)
                        };
                    }
                },
                icon: "cornered"),


            ["HeadStart"] = new (
                "Head Start",
                "<b><u>Start turn here:</u></b> Gain 1 <sprite name=\"footsteps\">.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.StartTurn] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainStepsCardEvent(1)
                        };
                    }
                },
                icon: "headstart"),

            ["Momentum"] = new (
                "Momentum",
                "<b><u>Played cards:</u></b> Deal 1 additional damage for each tile moved this combat.",
                TileSetEnum.Red,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) =>
                {
                    foreach (AbstractCardEvent cardEvent in e)
                    {
                        if (cardEvent is AttackCardEvent attackCardEvent)
                        {
                            attackCardEvent.amount += BattleStats.TilesMovedThisBattle;
                        }
                    }

                    return e;
                },
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {},
                icon: "momentum"),


            ["Stacked"] = new (
                "Stacked",
                "<b><u>Played cards:</u></b> When drawing more cards, draw 1 extra.",
                TileSetEnum.Purple,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) =>
                {
                    foreach (AbstractCardEvent cardEvent in e)
                    {
                        if (cardEvent is DrawCardEvent attackCardEvent)
                        {
                            attackCardEvent._cardsToDraw += 1;
                        }
                    }

                    return e;
                },
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                    {},
                icon: "stacked"),

            ["RestlessStep"] = new (
                "Restless Step",
                "<b><u>On enter:</u></b> Gain 1 <sprite name=\"footsteps\">. You must move before playing cards.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainStepsCardEvent(1),
                            new AddMoveBeforeCardRestrictionEvent()
                        };
                    }
                },
                icon: "restlessstep"),

            ["Ignite"] = new (
                "Ignite",
                "<b><u>On enter:</u></b> Gain 1 <sprite name=\"energyicon\">. A random card in your hand gains Burning,",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainEnergyCardEvent(1),
                            new AddCardStatusToRandomHandCardEvent("burning")
                        };
                    }
                },
                icon: "ignite"),

            ["Countdown"] = new (
                "Countdown",
                "<b><u>Counts down.</u></b>  At 0, deal 60 <sprite name=\"damage4\"> to anything here.",
                TileSetEnum.Red,
                true,
                Rarity.Common,
                TileType.Bad,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>(),
                icon: "countdown3",
                triggerLimit: TileTriggerLimit.None,
                shouldMarkAsTriggered: null,
                countdownEffect: new TileCountdownEffect(
                    3,
                    new[] { "countdown3", "countdown2", "countdown1" },
                    "Damage4",
                    "none",
                    60)),

            ["BloodyBattery"] = new (
                "Bloody Battery",
                "<b><u>On enter:</u></b> Gain 1 <sprite name=\"energyicon\"> and take 5 <sprite name=\"damage4\">.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainEnergyCardEvent(1),
                            new DamageSelfCardEvent(5)
                        };
                    }
                },
                icon: "bloodbattery"),

            ["HouseEdge"] = new (
                "House Edge",
                "<b><u>Played Cards:</u></b> 25% chance to play it twice, 75% to not play.",
                TileSetEnum.Purple,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) =>
                {
                    if (context.PreviewMode)
                    {
                        return e;
                    }

                    int val = context.GetRandom("dice").Next(100);
                    if (val < 25)
                    {
                        e.AddRange(CopyEventsForRepeat(e));
                        return e;
                    }
                    else
                    {
                        return new List<AbstractCardEvent>();
                    }
                },
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> {},
                icon: "houseedge"),

            ["GlassEdge"] = new (
                "Glass Edge",
                "<b><u>Played Cards:</u></b> Double attack damage from this tile, but increase damage received by 50%.",
                TileSetEnum.Red,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) =>
                {
                    foreach (AbstractCardEvent cardEvent in e)
                    {
                        if (cardEvent is AttackCardEvent attackCardEvent)
                        {
                            attackCardEvent.amount *= 2;
                        }
                    }

                    return e;
                },
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>> {},
                icon: "glassedge",
                incomingEventModifier: (e, context) =>
                {
                    if (context.Entity == null || context.Entity.entityType != EntityType.Player)
                    {
                        return e;
                    }

                    foreach (AbstractCardEvent cardEvent in e)
                    {
                        if (cardEvent is AttackCardEvent attackCardEvent)
                        {
                            attackCardEvent.amount = Mathf.CeilToInt(attackCardEvent.amount * 1.5f);
                        }
                    }

                    return e;
                }),

            ["LoanShark"] = new (
                "Loan Shark",
                "<b><u>On enter:</u></b> Gain $5.\n<b><u>End turn here:</u></b> Lose $7.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainMoneyCardEvent(5),
                        };
                    },
                    [TriggerEventTime.EndTurn] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainMoneyCardEvent(-7),
                        };
                    }
                },
                icon: "loanshark"),

            // ["HouseEdge"] = new (
            //     "House Edge",
            //     "When playing a card here, 75% chance to play it twice, 25% to not play.",
            //     HexColorUtility.HexToColor("#27AE60"),
            //     true,
            //     Rarity.Common,
            //     TileType.Good,
            //     (e, card) => e,
            //     new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
            //     {
            //         [TriggerEventTime.Land] = () =>
            //         {
            //             if ()
            //         },
            //         [TriggerEventTime.EndTurn] = () =>
            //         {
            //             return new List<AbstractCardEvent>
            //             {
            //                 new GainMoneyCardEvent(-7),
            //             };
            //         }
            //     },
            //     "houseedge"),
            ["RecklessStrike"] = new (
                "Reckless Strike",
                "<b><u>Played cards:</u></b> Deal double <sprite name=\"damage4\">. You cannot gain <sprite name=\"shield\">.",
                TileSetEnum.Red,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) =>
                {
                    foreach (AbstractCardEvent cardEvent in e)
                    {
                        if (cardEvent is AttackCardEvent attackCardEvent)
                        {
                            attackCardEvent.amount *= 2;
                        }

                        if (cardEvent is ShieldCardEvent shieldCardEvent)
                        {
                            shieldCardEvent.amount = 0;
                        }
                    }

                    return e;
                },
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {},
                icon: "recklessstrike"),
            ["Overcharge"] = new (
                "Overcharge",
                "<b><u>On enter:</u></b> Gain 2 <sprite name=\"energyicon\">.\n<b><u>End turn here:</u></b> Take <sprite name=\"damage4\"> equal to 5x current <sprite name=\"energyicon\">.",
                TileSetEnum.Green,
                true,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainEnergyCardEvent(2),
                        };
                    },
                    [TriggerEventTime.EndTurn] = (context) =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new DamageSelfCardEvent(RunInfo.Instance.CurrentEnergy * 5)
                        };
                    }
                },
                icon: "overcharge"),

            ["basic"] = new (
                "Basic",
                "No effect.",
                TileSetEnum.Unchanged,
                false,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>()),

            ["start"] = new (
                "Start",
                "Starting tile.",
                TileSetEnum.System,
                false,
                Rarity.Common,
                TileType.Good,
                (e, card, context) => e,
                new SerializedDictionary<TriggerEventTime, Func<TileContext, List<AbstractCardEvent>>>(),
                icon: "House"
                ),



        };

        public static IEnumerable<TileEntry> GetTilesByType(TileType tileType)
        {
            return tiles.Values.Where(tile => tile.tileType == tileType);
        }

        private static List<AbstractCardEvent> CopyEventsForRepeat(List<AbstractCardEvent> events)
        {
            return events.Select(CopyEventForRepeat).ToList();
        }

        private static AbstractCardEvent CopyEventForRepeat(AbstractCardEvent cardEvent)
        {
            switch (cardEvent)
            {
                case AttackCardEvent attackCardEvent:
                    return attackCardEvent.Copy();
                case ApplyStatusToEntityCardEvent applyStatusEvent:
                    return CopyPreviewSource(
                        new ApplyStatusToEntityCardEvent(applyStatusEvent.target, applyStatusEvent.status),
                        applyStatusEvent);
                case PushEntityAwayCardEvent pushEvent:
                    return pushEvent.Copy();
                default:
                    return cardEvent;
            }
        }

        private static T CopyPreviewSource<T>(T copy, AbstractCardEvent original) where T : AbstractCardEvent
        {
            copy.PreviewSourceActionIndex = original.PreviewSourceActionIndex;
            return copy;
        }

    }
}
