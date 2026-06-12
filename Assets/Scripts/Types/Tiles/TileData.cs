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
                HexColorUtility.HexToColor("#8E44AD"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = () => new List<AbstractCardEvent>
                    {
                        new SeeCardsThenPickCardEvent(3)
                    }
                },
                "foresight"),
            
            ["Refund"] = new ( 
                "Refund",
                "<b><u>Once per turn:</u></b> When you play a card costing 2+ <sprite name=\"energyicon\"> here, gain 1 <sprite name=\"energyicon\">.",
                HexColorUtility.HexToColor("#2ECC71"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) =>
                {
                    if (card.Cost >= 2)
                    {
                        e.Add(new GainEnergyCardEvent(1));
                        PlayingState playingState = GameStateManager.Instance.GetCurrent<PlayingState>();
                    }

                    return e;
                },
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>> {},
                "refund", 
                TileTriggerLimit.OncePerTurn,
                (e, card) => { return card.Cost >= 3; }),
            
            ["Stride"] = new ( 
                "Stride",
                "<b><u>On enter:</u></b> Draw cards equal to tiles moved this turn.",
                HexColorUtility.HexToColor("#3498DB"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = () => new List<AbstractCardEvent>
                    {
                        new DrawCardEvent(BattleStats.TilesMovedThisTurn)
                    }
                },
                "stride", 
                TileTriggerLimit.OncePerTurn),
            
            ["Cornered"] = new ( 
                "Cornered",
                "<b><u>On enter:</u></b> Gain 5 <sprite name=\"shield\"> for each adjacent enemy.",
                HexColorUtility.HexToColor("#2471A3"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = () =>
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
                "cornered"),
            
            
            ["HeadStart"] = new ( 
                "Head Start",
                "<b><u>On turn start:</u></b> Gain 1 <sprite name=\"footsteps\">.",
                HexColorUtility.HexToColor("#1ABC9C"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.StartTurn] = () =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainStepsCardEvent(1)
                        };
                    }
                },
                "headstart"),
            
            ["BloodyBattery"] = new ( 
                "Bloody Battery",
                "<b><u>On enter:</u></b> Gain 1 <sprite name=\"energyicon\"> and take 5 <sprite name=\"damage4\">.",
                HexColorUtility.HexToColor("#27AE60"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = () =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainEnergyCardEvent(1),
                            new DamageSelfCardEvent(5)
                        };
                    }
                },
                "bloodbattery"),
            
            ["LoanShark"] = new ( 
                "Loan Shark",
                "<b><u>On enter:</u></b> Gain $5.\n<b><u>On turn end:</u></b> Lose $7.",
                HexColorUtility.HexToColor("#27AE60"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = () =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainMoneyCardEvent(5),
                        };
                    },
                    [TriggerEventTime.EndTurn] = () =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainMoneyCardEvent(-7),
                        };
                    }
                },
                "loanshark"),
            
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
                "<b><u>While here:</u></b> Deal double <sprite name=\"damage4\">. You cannot gain <sprite name=\"shield\">.",
                HexColorUtility.HexToColor("#E67E22"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) =>
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
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {},
                "recklessstrike"),
            ["Overcharge"] = new ( 
                "Overcharge",
                "<b><u>On enter:</u></b> Gain 2 <sprite name=\"energyicon\">.\n<b><u>On turn end:</u></b> Take <sprite name=\"damage4\"> equal to 5x current <sprite name=\"energyicon\">.",
                HexColorUtility.HexToColor("#229954"),
                true,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>
                {
                    [TriggerEventTime.Land] = () =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new GainEnergyCardEvent(2),
                        };
                    },
                    [TriggerEventTime.EndTurn] = () =>
                    {
                        return new List<AbstractCardEvent>
                        {
                            new DamageSelfCardEvent(RunInfo.Instance.CurrentEnergy * 5)
                        };
                    }
                },
                "overcharge"),
            
            ["basic"] = new ( 
                "Basic",
                "No effect.",
                new Color(32.0f/255.0f, 99.0f/255.0f, 155.0f/255.0f),
                false,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>()),
            
            ["start"] = new (
                "Start", 
                "Starting tile.",
                new Color(173.0f/255.0f,173.0f/255.0f,173.0f/255.0f), 
                false, 
                Rarity.Common, 
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>(),
                "House"
                ),
            
            
            
        };
        
        public static IEnumerable<TileEntry> GetTilesByType(TileType tileType)
        {
            return tiles.Values.Where(tile => tile.tileType == tileType);
        }

    }
}
