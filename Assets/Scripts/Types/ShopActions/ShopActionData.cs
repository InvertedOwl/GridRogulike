using System;
using System.Collections.Generic;
using System.Linq;
using Entities;
using StateManager;

namespace Types.ShopActions
{
    public static class ShopActionData
    {
        public static Random shopActionRandom = RunInfo.NewRandom("shopaction".GetHashCode());
        
        public static List<ShopActionEntry> ShopActionEntries = new List<ShopActionEntry>
        {
            new (4, "Heal All", () =>
            {
                Player.Instance.Health += float.MaxValue;
            }),
            new (4, "Merge Cards", () =>
            {
                GameStateManager.Instance.GetCurrent<ShopState>().BuyCombineCards();
            }),
            new (4, "+1 Max Energy", () =>
            {
                RunInfo.Instance.MaxEnergy += 1;
                RunInfo.Instance.CurrentEnergy += 1;
            })
        };


        public static List<ShopActionEntry> GetThreeActions()
        {
            return ShopActionEntries.OrderBy(x => shopActionRandom.Next()).Take(3).ToList();
        }
    }
}