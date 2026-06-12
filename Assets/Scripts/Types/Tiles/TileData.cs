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
                "When landing here, Look at the top 3 cards of the draw pile. Pick one to draw.",
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
                "First card played here that costs 2 energy or more, gain 1 energy. ",
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
                "When landing here, draw cards equal to number of tiles moved this turn.",
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
                "Gain 5 shield for each adjacent enemy when landing here.",
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
                "If starting turn here, gain 1 step.",
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
                "When landing here, gain 1 energy, take 5 damage.",
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
                "When landing here, Gain $5. When ending turn here, lose $7.",
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
                "Double damage from this tile, but you cannot block from this tile.",
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
                "Gain 4 energy, take damage equal to Energy × 5 at end of turn.",
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
                            new GainEnergyCardEvent(4),
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
                "No effect",
                new Color(32.0f/255.0f, 99.0f/255.0f, 155.0f/255.0f),
                false,
                Rarity.Common,
                TileType.Good,
                (e, card) => e,
                new SerializedDictionary<TriggerEventTime, Func<List<AbstractCardEvent>>>()),
            
            ["start"] = new (
                "Start", 
                "The starting tile", 
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
