using System;
using System.Collections.Generic;

namespace HeroGame.Models
{
    public class RegisterHeroes
    {
        public int AccountId { get; set; }

        public string Name { get; set; }

        public int Level { get; set; }

        public int Experience { get; set; }

        public int HealthPoints { get; set; }

        public int MaxHealthPoints { get; set; }

        public int AttackPoints { get; set; }

        public int CurrentStage { get; set; }

        public int PositionX { get; set; }

        public int PositionY { get; set; }
    }
}
