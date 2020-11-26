
namespace HeroGame.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using System.Threading.Tasks;

    public class Account
    {
        public int AccountId { get; set; }

        public string UserName { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }

        [InverseProperty( nameof( Hero.Account ) )]
        public virtual ICollection<Hero> Heroes { get; set; }
    }
}
