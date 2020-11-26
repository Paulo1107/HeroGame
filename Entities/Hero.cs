
namespace HeroGame.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;

    public class Hero
    {
        public int HeroID { get; set; }

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

        [ForeignKey( nameof( AccountId ) )]
        [InverseProperty( "Heroes" )]
        public virtual Account Account { get; set; }
    }
}

