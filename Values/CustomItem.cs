using System;

namespace SawTapes.Values
{
    public class CustomItem
    {
        public Type Type { get; internal set; }
        public Item Item { get; internal set; }
        public int Rarity { get; internal set; }
        public string Description { get; internal set; }
        public bool IsTileGame { get; internal set; }

        public CustomItem(Type type, Item item, int rarity, string description, bool isTileGame)
        {
            Type = type;
            Item = item;
            Rarity = rarity;
            Description = description;
            IsTileGame = isTileGame;
        }
    }
}
