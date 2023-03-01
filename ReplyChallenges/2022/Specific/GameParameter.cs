using System.Collections.Generic;

namespace ReplyChallenge2022
{
    internal static class GameParameter
    {
        public static int InitialStamina { get; set; }
        public static int MaxStamina { get; set; }
        public static int Turns { get; set; }
        public static List<Demon> Demons { get; set; } = new List<Demon>();
    }
}
