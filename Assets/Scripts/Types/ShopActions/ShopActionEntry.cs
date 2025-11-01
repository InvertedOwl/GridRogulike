using System;

namespace Types.ShopActions
{
    public class ShopActionEntry
    {
        public int cost;
        public string title;
        public Action callback;

        public ShopActionEntry(int cost, string title, Action callback)
        {
            this.cost = cost;
            this.title = title;
            this.callback = callback;
        }
    }
}