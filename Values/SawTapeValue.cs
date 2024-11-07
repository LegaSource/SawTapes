using System;

namespace SawTapes.Values
{
    public class SawTapeValue
    {
        public Type Type { get; internal set; }
        public Item Item { get; internal set; }
        public int DefaultRarity { get; internal set; }
        public int Rarity { get; internal set; }
        public bool IsTileGame { get; internal set; }

        public SawTapeValue(Type type, Item item, int rarity, bool isTileGame)
        {
            Type = type;
            Item = item;
            DefaultRarity = rarity;
            Rarity = rarity;
            IsTileGame = isTileGame;
        }
    }
}
