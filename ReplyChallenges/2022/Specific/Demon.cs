using System.Collections.Generic;

namespace ReplyChallenge2022
{
    internal class Demon
    {
        public int Id { get; set; }
        public int StaminaToDefeat { get; set; }
        public int TurnBeforeStamina { get; set; }
        public int StaminaRecovered { get; set; }
        public List<int> Fragments { get; set; } = new List<int>();
    }
}
