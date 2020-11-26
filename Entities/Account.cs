
namespace HeroGame.Entities
{
    public class Account
    {
        public int AccountId { get; set; }

        public string UserName { get; set; }

        public byte[] PasswordHash { get; set; }

        public byte[] PasswordSalt { get; set; }
    }
}
