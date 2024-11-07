using System.Collections.Generic;

namespace SawTapes
{
    public class STUtilities
    {
        public static void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = UnityEngine.Random.Range(0, i + 1);
                (list[randomIndex], list[i]) = (list[i], list[randomIndex]);
            }
        }
    }
}
